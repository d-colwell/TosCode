using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricentis.Automation.AutomationInstructions.TestActions;
using Tricentis.Automation.Creation;
using Tricentis.Automation.Engines;
using Tricentis.Automation.Engines.SpecialExecutionTasks;
using Tricentis.Automation.Engines.SpecialExecutionTasks.Attributes;
using Tricentis.Automation.Execution.Results;
using Tricentis.TCAddIns.XDefinitions.Modules;
using System.Reflection;
using TosCode;
using System.ComponentModel;
using Tricentis.Automation.AutomationInstructions.Dynamic.Values;
using TosCode.Helpers;

namespace Tricentis.Automation.AutomationInstructions.TestActions
{
    [SpecialExecutionTaskName("Execute")]
    public class CodeExecutor : SpecialExecutionTask
    {
        public CodeExecutor(Creation.Validator validator) : base(validator)
        {
        }

        public override ActionResult Execute(ISpecialExecutionTaskTestAction testAction)
        {
            var action = testAction as SpecialExecutionTaskTestAction;
            var module = action.TCItem.GetModule();
            XModule xMod = module as XModule;
            var xParams = xMod.GetXParams();
            ModuleType moduleType = ModuleType.Method;
            //if (!Enum.TryParse(xParams.Single(x => x.Name.ToLower() == "type").Value, out moduleType))
            //{
            //    throw new InvalidOperationException("Invalid value in 'Type' Technical Parameter");
            //}

            ReflectionConfig config = new ReflectionConfig
            {
                ClassName = xParams.Single(x => x.Name.ToLower() == "classname").Value,
                LibraryFile = xParams.Single(x => x.Name.ToLower() == "libraryfile").Value,
                MethodName = xParams.Single(x => x.Name.ToLower() == "methodname").Value
            };

            var classObject = ActivateObject(config);
            ActionResult result = null;
            switch (moduleType)
            {
                case ModuleType.Class:
                    result = ExecuteClass(action, classObject);
                    break;
                case ModuleType.Method:
                    result = ExecuteMethod(action, classObject, config);
                    break;
            }
            return result;
        }

        private ActionResult ExecuteMethod(SpecialExecutionTaskTestAction action, object classObject, ReflectionConfig config)
        {
            var parameters = action.Parameters;
            ActionResult r = null;
            Type type = classObject.GetType();
            //Check for a method with the same name, number of parameters, and parameter names match the Technical ID[Name] value of the attached module
            IEnumerable<MethodInfo> methods = type.GetMethods()
                .Where(x => x.GetParameters().Count() == parameters.Where(p=>!IsResultParameter(p)).Count()
                && x.Name == config.MethodName
                && x.GetParameters().All(p => parameters.Any(xp => GetParameterCodeName(xp) == p.Name)));
            MethodInfo methodInfo = null;
            if (!methods.Any())
                throw new InvalidOperationException($"No method found with name {config.MethodName} and parameters matching the input parameters");
            if (methods.Count() > 1)
            {
                if (action.Children.Any())
                {
                    if (methods.Where(x => x.ReturnType != typeof(void)).Count() == 1)
                        methodInfo = methods.First(x => x.ReturnType != typeof(void));
                }
            }
            else
                methodInfo = methods.First();
            if(methodInfo == null)
                throw new InvalidOperationException($"Unable to locate method {config.MethodName}, with the correct signature. Type overload on method footprints is currently not supported.");

            ParameterInfo[] methodParameters = methodInfo.GetParameters();

            List<object> instanceParameters = new List<object>();
            foreach (var parameter in methodParameters)
            {
                IParameter testParameter = parameters.FirstOrDefault(p => GetParameterCodeName(p) == parameter.Name);
                if (testParameter == null)
                    throw new Exception($"Unable to find Test value for method parameter {parameter.Name}");
                object result = GetObjectFromParameter(testParameter, parameter.ParameterType);
                instanceParameters.Add(result);
            }
            try
            {
                object methodResult = methodInfo.Invoke(classObject, instanceParameters.ToArray());
                var resultAction = parameters.FirstOrDefault(x => IsResultParameter(x));
                if (resultAction == null)
                    return new PassedActionResult($"Executed method {methodInfo.Name} successfuly. No result expected.");
                else
                {
                    if (!resultAction.Parameters.Any())
                    {
                        HandleActualValue(action, resultAction, methodResult);
                    }
                    else
                    {
                        var resultType = methodResult.GetType();
                        foreach (var property in resultType.GetProperties().Where(x => x.SetMethod.IsPublic)) 
                        {
                            var childParameter = resultAction.Parameters.FirstOrDefault(x => GetParameterCodeName(x) == property.Name);
                            if (childParameter != null)
                                HandleActualValue(action, childParameter, property.GetValue(methodResult));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string message = e.Message;
                if (e.InnerException != null)
                    message = e.InnerException.Message;
                r = new UnknownFailedActionResult(message);
                return r;
            }
            return new PassedActionResult();
        }

        private object GetObjectFromParameter(IParameter parameter, Type type)
        {
            if (type.IsSimple())
            {
                if (parameter.Parameters.Count() > 0)
                    throw new Exception($"Unable to create simple type {type.Name} with children attributes for parameter {parameter.Name}");
                var converter = TypeDescriptor.GetConverter(type);
                InputValue val = parameter.Value as InputValue;
                object result = converter.ConvertFrom(val.Value);
                return result;
            }
            var parentParameter = Activator.CreateInstance(type);
            var properties = type.GetProperties().Where(x => x.SetMethod.IsPublic);
            foreach (var property in properties)
            {
                var matchingParameter = parameter.Parameters.FirstOrDefault(x => GetParameterCodeName(x) == property.Name);
                if (matchingParameter == null)
                    continue;
                object propertyValue = GetObjectFromParameter(matchingParameter, property.PropertyType);
                property.SetValue(parentParameter, propertyValue);
            }
            return parentParameter;
        }

        private string GetParameterCodeName(IParameter parameter)
        {
            var configParam = parameter.XParameters.ConfigurationParameters.FirstOrDefault(x => x.Name == "Name");
            if (configParam == null)
                return null;
            return configParam.UnparsedValue;
        }
        private bool IsResultParameter(IParameter child)
        {
            var configParam = child.XParameters.ConfigurationParameters.FirstOrDefault(x => x.Name == "Result");
            if (configParam == null)
                return false;
            bool result = false;
            if (!bool.TryParse(configParam.UnparsedValue, out result))
                return false;
            return result;
        }

        private ActionResult ExecuteClass(SpecialExecutionTaskTestAction action, object classObject)
        {
            throw new InvalidOperationException("Class based functionality not built yet.");
        }

        private object ActivateObject(ReflectionConfig config)
        {
            Assembly assembly = null;
            Type type = null;
            //Load the assembly and execute
            if (!string.IsNullOrEmpty(config.LibraryFile))
            {
                assembly = Assembly.LoadFrom(config.LibraryFile);
                type = assembly.GetType(config.ClassName);
            }
            else
            {
                //Search loaded assemblies for the type
                type = Type.GetType(config.ClassName, true);
            }
            //Check if it has a parameterless constructor
            if (!type.GetConstructors().Any(x => x.GetParameters().Length == 0))
                throw new InvalidOperationException(string.Format("Unable to create the class {0} as it doesn't have a parameterless constructor", config.ClassName));
            return InstanceManager.Instance.GetInstance(type);
        }

        private MethodInfo FindMethod(Type type, string name, IEnumerable<IParameter> parameters)
        {
            var similarMethods = type.GetMethods().Where(m => m.Name == name);
            if (similarMethods.Count() == 1)
                return similarMethods.First();
            var inputParameters = parameters.Where(x => !IsResultParameter(x));
            var resultParameter = parameters.FirstOrDefault(x => IsResultParameter(x));
            throw new NotImplementedException();
        }

    }
}

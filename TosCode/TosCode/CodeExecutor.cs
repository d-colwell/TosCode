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

namespace Tricentis.Automation.AutomationInstructions.TestActions
{
    [SpecialExecutionTaskName("Execute")]
    public class CodeExecutor : SpecialExecutionTask
    {
        public CodeExecutor(Validator validator) : base(validator)
        {
        }

        public override ActionResult Execute(ISpecialExecutionTaskTestAction testAction)
        {
            var action = testAction as SpecialExecutionTaskTestAction;
            var module = action.TCItem.GetModule();
            XModule xMod = module as XModule;
            var xParams = xMod.GetXParams();
            ModuleType moduleType;
            if (!Enum.TryParse(xParams.Single(x => x.Name.ToLower() == "type").Value, out moduleType))
            {
                throw new InvalidOperationException("Invalid value in 'Type' Technical Parameter");
            }

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
                .Where(x => x.GetParameters().Count() == parameters.Count 
                && x.Name == config.MethodName 
                && x.GetParameters().All(p => parameters.Any(xp => xp.XParameters.TechnicalIdParameters.Single(tp => tp.Name == "Name").UnparsedValue == p.Name)));

            if (!methods.Any())
                throw new InvalidOperationException($"No method found with name {config.MethodName} and parameters matching the input parameters");
            if(methods.Count() > 1)
                throw new InvalidOperationException($"More than one method found with name {config.MethodName}, which has the same parameter names. Type overload on method footprints is currently not supported.");
            MethodInfo methodInfo = methods.First();
            ParameterInfo[] methodParameters = methodInfo.GetParameters();



            List<object> instanceParameters = new List<object>();
            foreach (var parameter in methodParameters)
            {
                IParameter testValue = parameters.First(p => p.XParameters.TechnicalIdParameters.Single(tp => tp.Name == "Name").UnparsedValue == parameter.Name);
                var converter = TypeDescriptor.GetConverter(parameter.ParameterType);
                InputValue val = testValue.Value as InputValue;
                
                object result = converter.ConvertFrom(val.Value);
                instanceParameters.Add(result);
            }
            try
            {
                object methodResult = methodInfo.Invoke(classObject, instanceParameters.ToArray());
                r = new PassedActionResult("Method executed successfuly");
            }
            catch (Exception e)
            {
                r = new UnknownFailedActionResult(e.Message);
            }
            return r;
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
            return Activator.CreateInstance(type);
        }

    }
}

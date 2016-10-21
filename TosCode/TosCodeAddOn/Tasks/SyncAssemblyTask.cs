using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TosCode.Helpers;
using TosCode.Scanner;
using TosCode.Scanner.ViewModels;
using Tricentis.TCAddOns;
using Tricentis.TCAPIObjects.Objects;

namespace TosCodeAddOn.Tasks
{
    public class SyncAssemblyTask : TCAddOnTask
    {
        public const int MAX_DEPTH = 7;
        public override Type ApplicableType => typeof(TCFolder);
        public override string Name => Resources.Text.SyncAssemblyTaskName;


        public override bool IsTaskPossible(TCObject obj)
        {
            TCFolder fldr = (TCFolder)obj;
            return fldr.PossibleContent.Contains("Module");
        }


        public override TCObject Execute(TCObject objectToExecuteOn, TCAddOnTaskContext taskContext)
        {


            TCFolder folder = objectToExecuteOn as TCFolder;
            //var window = new MainWindow();
            //window.ShowDialog();

            ParameterizedThreadStart pts = new ParameterizedThreadStart(ThreadStart);
            Thread t = new Thread(ThreadStart);
            t.SetApartmentState(ApartmentState.STA);
            t.Start(folder);
            t.Join();
            return folder;
        }

        private void ThreadStart(object target)
        {
            TCFolder folder = target as TCFolder;
            var connector = new Connector(folder);
            var assemblyModels = connector.GenerateAssemblyModels();
            var window = new MainWindow(assemblyModels);
            window.ShowDialog();
            foreach (var assembly in window.Assemblies)
            {
                var matches = connector.MatchMethodModules(assembly.Model());
                foreach (var match in matches)
                {
                    match.MethodModel.FriendlyName = match.XModule.Name;
                    match.MethodModel.IsSelected = true;
                }
                var assemblyFolder = GetOrCreateChildFolder(folder, assembly.Name);
                foreach (var cls in assembly.Classes)
                {
                    var classFolder = GetOrCreateChildFolder(assemblyFolder, cls.Name);
                    foreach (var method in cls.Methods.Where(x => x.IsSelected))
                    {
                        //TODO:
                        //Fix this matching algorithm
                        if (!matches.Any(x => x.MethodModel.MethodName == method.Name))
                        {
                            var module = CreateModule(assembly, cls, method, classFolder);
                        }
                    }
                }
            }

        }
        #region Helpers
        private XModule CreateModule(AssemblyViewModel assembly, ClassViewModel cls, MethodViewModel method, TCFolder parent)
        {
            var module = parent.CreateXModule();
            module.Name = method.FriendlyName ?? method.Name;
            var assemblyParam = module.CreateTechnicalIDParam();
            var classNameParam = module.CreateTechnicalIDParam();
            var methodNameParam = module.CreateTechnicalIDParam();
            var engineParam = module.CreateConfigurationParam();
            var setParam = module.CreateConfigurationParam();

            assemblyParam.Name = "LibraryFile";
            assemblyParam.Value = assembly.FilePath;
            classNameParam.Name = "ClassName";
            classNameParam.Value = cls.FullName;
            methodNameParam.Name = "MethodName";
            methodNameParam.Value = method.Name;
            engineParam.Name = "Engine";
            engineParam.Value = "TosCode";
            setParam.Name = "SpecialExecutionTask";
            setParam.Value = "Execute";

            foreach (var parameter in method.Parameters)
            {
                CreateModuleAttribute(module, parameter);
            }
            if (method.ReturnType != typeof(void))
            {
                var returnAttribute = module.CreateModuleAttribute();
                BuildModuleAttribute(returnAttribute, "Result", method.ReturnType, 1, true, XTestStepActionMode.Verify);
                var returnParam = returnAttribute.CreateConfigurationParam();
                returnParam.Name = "Result";
                returnParam.Value = "true";
            }

            return module;
        }

        private XModuleAttribute CreateModuleAttribute(XModule parent, ParameterViewModel parameter)
        {
            var moduleAttribute = parent.CreateModuleAttribute();
            BuildModuleAttribute(moduleAttribute, parameter.Name, parameter.ParameterType, 1, true);
            return moduleAttribute;
        }

        private void BuildModuleAttribute(XModuleAttribute attribute, string name, Type type, int currentDepth, bool isParameter, XTestStepActionMode defaultMode = XTestStepActionMode.Input)
        {
            attribute.Name = name;
            if (isParameter)
            {
                var paramParam = attribute.CreateConfigurationParam();
                paramParam.Name = "Parameter";
                paramParam.Value = "True";
            }
            var nameParam = attribute.CreateConfigurationParam();
            nameParam.Name = "Name";
            nameParam.Value = attribute.Name;

            if (type.IsSimple()) //This is a simple type
            {
                if (IsNumericType(type))
                    attribute.DefaultDataType = ModuleAttributeDataType.Numeric;
                else if (type == typeof(DateTime))
                    attribute.DefaultDataType = ModuleAttributeDataType.Date;
                else if (type == typeof(bool))
                {
                    attribute.DefaultDataType = ModuleAttributeDataType.Boolean;
                    attribute.ValueRange = "true;false";
                }
                else if (type.IsEnum)
                {
                    attribute.DefaultDataType = ModuleAttributeDataType.String;
                    attribute.ValueRange = Enum.GetNames(type).Aggregate((x, y) => $"{x};{y}");
                }
                else
                    attribute.DefaultDataType = ModuleAttributeDataType.String;
                attribute.DefaultActionMode = defaultMode;
            }
            else //class
            {
                attribute.DefaultActionMode = XTestStepActionMode.Select;
                var structureModules = CreateClassStructure(attribute, type, defaultMode, currentDepth, isParameter).ToArray();
            }
        }
        //TODO:
        //When you run this you get an exception. Run in Tosca and fix it derp
        private IEnumerable<XModuleAttribute> CreateClassStructure(XModuleAttribute parent, Type type, XTestStepActionMode defaultMode, int currentDepth, bool isParameter)
        {
            currentDepth++;
            if (currentDepth > MAX_DEPTH)
                yield break;
            //Loop through properties
            var properties = type.GetProperties().Where(x => x.SetMethod.IsPublic);
            foreach (var property in properties)
            {
                var childAttribute = parent.CreateModuleAttribute();
                BuildModuleAttribute(childAttribute, property.Name, property.PropertyType, currentDepth, isParameter, defaultMode);
                yield return childAttribute;
            }
        }

        private TCFolder GetOrCreateChildFolder(TCFolder parent, string name)
        {
            var folder = parent.Search($"=>SUBPARTS:TCFolder[Name==\"{name}\"]").Where(f => f.OwningObject == parent).FirstOrDefault() as TCFolder;
            if (folder == null)
            {
                folder = parent.CreateFolder();
                folder.Name = name;
            }
            return folder;
        }
        public bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        #endregion
    }
}

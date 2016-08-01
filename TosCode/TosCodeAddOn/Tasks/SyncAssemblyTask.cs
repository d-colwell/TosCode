using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TosCode.Scanner;
using Tricentis.TCAddOns;
using Tricentis.TCAPIObjects.Objects;

namespace TosCodeAddOn.Tasks
{
    public class SyncAssemblyTask : TCAddOnTask
    {
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
                    foreach (var method in cls.Methods.Where(x=>x.IsSelected))
                    {
                        //TODO:
                        //Fix this matching algorithm
                        if(!matches.Any(x=>x.MethodModel.MethodName == method.Name))
                        {
                            var module = classFolder.CreateXModule();
                            module.Name = method.FriendlyName ?? method.Name;
                            var assemblyParam = module.CreateTechnicalIDParam();
                            var classNameParam = module.CreateTechnicalIDParam();
                            var methodNameParam = module.CreateTechnicalIDParam();
                            var engineParam = module.CreateConfigurationParam();
                            var setParam = module.CreateConfigurationParam();

                            assemblyParam.Name = "LibraryFile";
                            assemblyParam.Value = assembly.FilePath;
                            classNameParam.Name = "ClassName";
                            classNameParam.Value = cls.Name;
                            methodNameParam.Name = "MethodName";
                            methodNameParam.Value = method.Name;
                            engineParam.Name = "Engine";
                            engineParam.Value = "TosCode";
                            setParam.Name = "SpecialExecutionTask";
                            setParam.Value = "Execute";

                            foreach (var parameter in method.Parameters)
                            {
                                var moduleAttribute = module.CreateModuleAttribute();
                                moduleAttribute.Name = parameter.Name;
                                var paramParam = moduleAttribute.CreateConfigurationParam();
                                paramParam.Name = "Parameter";
                                paramParam.Value = "True";
                                var nameParam = moduleAttribute.CreateConfigurationParam();
                                nameParam.Name = "Name";
                                nameParam.Value = parameter.Name;
                                Type type = parameter.ParameterType;
                                if (IsNumericType(type))
                                    moduleAttribute.DefaultDataType = ModuleAttributeDataType.Numeric;
                                else if (type == typeof(DateTime))
                                    moduleAttribute.DefaultDataType = ModuleAttributeDataType.Date;
                                else if (type == typeof(bool))
                                    moduleAttribute.DefaultDataType = ModuleAttributeDataType.Boolean;
                                else if (type.IsEnum)
                                {
                                    moduleAttribute.DefaultDataType = ModuleAttributeDataType.String;
                                    moduleAttribute.ValueRange = Enum.GetNames(type).Aggregate((x, y) => $"{x};{y}");
                                }
                                else
                                    moduleAttribute.DefaultDataType = ModuleAttributeDataType.String;

                            }
                        }
                    }
                }
            }

        }
        #region Helpers

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

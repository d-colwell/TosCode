using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TosCode.Connector.Models;
using Tricentis.TCAPIObjects.Objects;

namespace TosCodeAddOn
{
    public class Connector
    {
        private TCFolder folder;

        public Connector(TCFolder rootFolder)
        {
            this.folder = rootFolder;
        }

        public IEnumerable<MethodModuleMatch> MatchMethodModules(AssemblyModel assembly)
        {
            IEnumerable<XModule> methodModules = folder.Search("=>SUBPARTS:XModule=>SUBPARTS:XParam[Name==\"Engine\" AND Value==\"TosCode\"]=>OwningObject").Cast<XModule>();
            var matches = from mm in methodModules
                          join m in assembly.Classes.SelectMany(c => c.Methods)
                             on new { c = mm.XParams.Single(x => x.Name.ToLower() == "classname").Value, m = mm.XParams.Single(x => x.Name.ToLower() == "methodname").Value } equals new { c = m.DeclaringType.ClassName, m = m.MethodName }
                          select new MethodModuleMatch { XModule = mm, MethodModel = m };

            return matches;
        }

        public IEnumerable<AssemblyModel> GenerateAssemblyModels()
        {
            IEnumerable<XModule> methodModules = folder.Search("=>SUBPARTS:XModule=>SUBPARTS:XParam[Name==\"Engine\" AND Value==\"TosCode\"]=>OwningObject").Cast<XModule>();
            var assemblies = methodModules.GroupBy(am => am.XParams.Single(xp => xp.Name.ToLower() == "libraryfile").Value)
                .Select(ag => new AssemblyModel
                {
                    AssemblyFilePath = ag.Key,
                    Classes = ag.GroupBy(cm => cm.XParams?.SingleOrDefault(xp => xp.Name.ToLower() == "classname").Value)
                                .Where(x=>x.Key != null)
                                .Select(cg => new ClassModel
                                {
                                    ClassName = cg.Key,
                                    Methods = cg.Select(mm => new MethodModel
                                    {
                                        FriendlyName = mm.Name,
                                        IsSelected = true,
                                        MethodName = mm.XParams.First(xp => xp.Name.ToLower() == "methodname").Value
                                    })
                                })
                });

            foreach (var cls in assemblies.SelectMany(a => a.Classes))
            {
                foreach (var method in cls.Methods)
                {
                    method.DeclaringType = cls;
                }
            }
            return assemblies;
        }

    }
}

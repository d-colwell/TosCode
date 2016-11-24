using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricentis.TCAddIns.XDefinitions.Modules;

namespace TosCodeAddOn
{
    public static class ModuleHelpers
    {
        public static XParam CreateTechnicalIDParam(this XModule module)
        {
            var xp = XParam.Create();
            xp.ParamType = XParam.ParamTypeE.TechnicalID;
            module.Properties.Add(xp);
            return xp;
        }

        public static XParam CreateTechnicalIDParam(this XModuleAttribute moduleAttribute)
        {
            var xp = XParam.Create();
            xp.ParamType = XParam.ParamTypeE.TechnicalID;
            moduleAttribute.Properties.Add(xp);
            return xp;
        }

        public static XParam CreateConfigurationParam(this XModule module)
        {
            var xp = XParam.Create();
            xp.ParamType = XParam.ParamTypeE.Configuration;
            module.Properties.Add(xp);
            return xp;
        }
        public static XParam CreateConfigurationParam(this XModuleAttribute moduleAttribute)
        {
            var xp = XParam.Create();
            xp.ParamType = XParam.ParamTypeE.Configuration;
            moduleAttribute.Properties.Add(xp);
            return xp;
        }
        public static XModuleAttribute CreateModuleAttribute(this XModule module)
        {
            var ma = XModuleAttribute.Create();
            ma.Module.Set(module);
            //module.Attributes.Add(ma);
            return ma;
        }
        public static XModuleAttribute CreateModuleAttribute(this XModuleAttribute moduleAttribute)
        {
            var ma = XModuleAttribute.Create();
            moduleAttribute.Attributes.Add(ma);
            //ma.Module.Set(moduleAttribute.Module.Get());
            return ma;
        }
    }
}

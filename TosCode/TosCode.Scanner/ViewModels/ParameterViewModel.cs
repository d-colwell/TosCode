using System;
using System.Reflection;

namespace TosCode.Scanner.ViewModels
{
    public class ParameterViewModel
    {
        private ParameterInfo parameter;

        public ParameterViewModel() { }

        public ParameterViewModel(ParameterInfo parameter)
        {
            this.parameter = parameter;
        }

        public string Name => parameter.Name;
        public string TypeName => parameter.ParameterType.Name;
        public Type ParameterType => parameter.ParameterType;
    }
}
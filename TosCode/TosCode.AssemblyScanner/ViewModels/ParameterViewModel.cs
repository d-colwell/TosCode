using System.Reflection;

namespace TosCode.AssemblyScanner.ViewModels
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
        public string Type => parameter.ParameterType.Name;
    }
}
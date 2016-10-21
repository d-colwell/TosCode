using System.Collections.Generic;

namespace TosCode.Connector.Models
{
    public class MethodModel
    {
        public ClassModel DeclaringType { get; set; }
        public string MethodName { get; set; }
        public string FriendlyName { get; set; }
        public List<ParameterModel> Parameters { get; set; }
        public bool IsSelected { get; set; }
    }
}
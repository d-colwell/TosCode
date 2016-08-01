using System;
using System.Collections.Generic;

namespace TosCode.Connector.Models
{
    public class ClassModel
    {
        public string ClassName { get; set; }
        public IEnumerable<MethodModel> Methods { get; set; }
    }
}
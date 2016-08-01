using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TosCode.Connector.Models
{
    public class AssemblyModel
    {
        public string AssemblyFilePath { get; set; }
        public IEnumerable<ClassModel> Classes { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TosCode
{
    ///Included for reference and testing only
    public class ExampleClass
    {
        public string ExampleProperty { get; set; }
        public int ExampleInt { get; set; }
        public DateTime ExampleDateTime { get; set; }
        public string ExampleReadOnly { get; private set; }

        public void SendEmail(string to, string subject, DateTime sendDate)
        {
            return;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BddSampleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var testdefs = new BddExample.TestStepDefinitions();
            testdefs.OpenSampleApp("http://sampleapp.tricentis.com/101/");
        }
    }
}

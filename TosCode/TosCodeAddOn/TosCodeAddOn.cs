using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricentis.TCAddOns;

namespace TosCodeAddOn
{
    public class TosCodeAddOn : TCAddOn
    {
        public override string UniqueName => "TosCodeAddOn";
        public override string DisplayedName => Resources.Text.AddOnName;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricentis.TCAddOns;
using Tricentis.TCAPIObjects.Objects;

namespace TosCodeAddOn.Tasks
{
    public class SyncAssemblyTask : TCAddOnTask
    {
        public override Type ApplicableType => typeof(TCFolder);
        public override string Name => Resources.Text.SyncAssemblyTaskName;
        public override bool IsTaskPossible(TCObject obj)
        {
            TCFolder fldr = (TCFolder)obj;
            return fldr.PossibleContent.Contains("Module");
        }


        public override TCObject Execute(TCObject objectToExecuteOn, TCAddOnTaskContext taskContext)
        {
            throw new NotImplementedException();
            //Scan assembly
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TosCodeAddOn.Tasks;
using Tricentis.TCCore.BusinessObjects.Folders;
using Tricentis.TCCore.Persistency;
using Tricentis.TCCore.Persistency.AddInManager;

namespace TosCodeAddOn
{
    public class TosCodeTaskInterceptor : TaskInterceptor
    {
        public TosCodeTaskInterceptor(TCFolder folder)
        {

        }
        public override void GetTasks(PersistableObject obj, List<Tricentis.TCCore.Persistency.Task> tasks)
        {
            if(obj is TCFolder)
            {
                TCFolder fldr = (TCFolder)obj;
                if(fldr.PossibleContent.Contains("Module"))
                {
                    tasks.Add(new SyncAssemblyTask());
                }
            }
        }
    }
}

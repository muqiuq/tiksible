using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater.Playbooks
{
    public class InstallSshPubKeyPlaybook : IPlaybook
    {
        List<FullExecutionOrder> orderList = new List<FullExecutionOrder>();

        public const string FileName = "tiksible-user-pubkey.pub";
        public const string Output = "output";

        public InstallSshPubKeyPlaybook(string user)
        {
            orderList.Add(ExecutionOrderHelper.UploadFile(FileName));
            orderList.Add(ExecutionOrderHelper.Sleep(1000));
            orderList.Add(ExecutionOrderHelper.CmdWithOutput($" /user/ssh-keys/import public-key-file={FileName} user={user}", Output));
        }

        public List<FullExecutionOrder> GetExecutionOrders()
        {
            return orderList;
        }
    }
}

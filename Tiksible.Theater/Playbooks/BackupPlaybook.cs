using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater.Playbooks
{
    public class BackupPlaybook : IPlaybook
    {
        public const string TmpBackupFileName = "tiksible-tmpbackup.backup";
        
        List<FullExecutionOrder> orderList = new List<FullExecutionOrder>();

        public BackupPlaybook()
        {
            orderList.Add(ExecutionOrderHelper.Cmd($"/system/backup/save dont-encrypt=yes name={TmpBackupFileName}"));
            orderList.Add(ExecutionOrderHelper.DownloadFile($"{TmpBackupFileName}"));
            orderList.Add(ExecutionOrderHelper.DeleteFileSFTP($"{TmpBackupFileName}"));
        }

        public List<FullExecutionOrder> GetExecutionOrders()
        {
            return orderList;
        }
    }
}

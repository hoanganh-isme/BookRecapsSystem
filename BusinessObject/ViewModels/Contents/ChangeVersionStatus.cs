using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Recaps
{
    public class ChangeVersionStatus
    {
        public Guid RecapVersionId {  get; set; }
        public RecapStatus Status { get; set; }
    }
}

using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Recaps
{
    public class CreateVersionWithRecap
    {
        public decimal VersionNumber { get; set; }
        public RecapStatus Status { get; set; }
        public Guid RecapId { get; set; }
        public Recap Recap { get; set; }
    }
}

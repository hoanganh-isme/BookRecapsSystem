using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class RecapEarning : BaseEntity
    {
        public Guid RecapId { get; set; }
        public decimal EarningAmount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Recap Recap { get; set; } 
    }
}

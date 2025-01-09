using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class BookEarning : BaseEntity
    {
        public Guid BookId { get; set; }
        public decimal EarningAmount {  get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Book Book { get; set; }

    }
}

using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class PublisherPayout : BaseEntity
    {
        public string? ImageURL {  get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public PayoutStatus Status { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid PublisherId { get; set; }
        public Publisher Publisher { get; set; }
        public ICollection<BookEarning> BookEarnings { get; set; }
    }
}

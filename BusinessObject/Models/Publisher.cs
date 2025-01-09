using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Publisher : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public string? PublisherName { get; set; }
        public string? ContactInfo { get; set; }
        public string? BankAccount {  get; set; }
        public decimal RevenueSharePercentage {  get; set; }
        public ICollection<Contract>? Contracts { get; set; }
        public ICollection<Book> Books { get; set; }
        public ICollection<PublisherPayout> PublisherPayouts { get; set;}
    }
}

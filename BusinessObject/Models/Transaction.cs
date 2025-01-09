using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BusinessObject.Models
{
    public class Transaction : BaseEntity
    {
        public Guid UserId {  get; set; }
        public Guid SubscriptionPackageId {  get; set; }
        public decimal Price {  get; set; }
        public TransactionsStatus Status { get; set; }
        public int OrderCode { get; set; }
        public string? PaymentMethod {  get; set; }
        public Guid? SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }
        public User User { get; set; }
        public SubscriptionPackage SubscriptionPackage { get; set; }
    }
}

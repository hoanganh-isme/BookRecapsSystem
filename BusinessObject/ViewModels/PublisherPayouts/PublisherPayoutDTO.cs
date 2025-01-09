using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;

namespace BusinessObject.ViewModels.PublisherPayouts
{
    public class PublisherPayoutDTO
    {
        public class PublisherPayoutDto
        {
            public Guid PayoutId { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public List<BookEarningDto> BookEarnings { get; set; }
            public DateTime CreateAt { get; set; }
        }
        public class PublisherHistoryDto
        {
            public Guid payoutId { get; set; }
            public Guid publisherId { get; set; }
            public string PublisherName { get; set; }
            public decimal? TotalEarnings { get; set; }
            public string Description { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public string Status { get; set; }
            public DateTime CreateAt {  get; set; }
        }
        public class PublisherDto
        {
            public Guid payoutId { get; set; }
            public Guid Id { get; set; }
            public string PublisherName { get; set; }
            public string? ContactInfo { get; set; }
            public string? BankAccount { get; set; }
            public decimal RevenueSharePercentage { get; set; }
            public decimal? TotalEarnings { get; set; }
            public DateTime? Fromdate { get; set; }
            public DateTime? Todate { get; set; }
            public string Status { get; set; }
        }
        public class PublisherEarningsDto
        {
            public string PublisherName { get; set; }
            public Guid PublisherId { get; set; }
            public string? ContactInfo { get; set; }
            public string? BankAccount { get; set; }
            public decimal TotalEarnings { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public List<BookEarningsDto> BookDetails { get; set; }
        }
        public class BookEarningsDto
        {
            public Guid BookId { get; set; }
            public string BookTitle { get; set; }
            public decimal BookEarnings { get; set; }
        }
        public class BookEarningDto
        {
            public Guid BookId { get; set; }
            public string BookTitle { get; set; }
            public decimal TotalEarnings { get; set; }
        }
    }
}

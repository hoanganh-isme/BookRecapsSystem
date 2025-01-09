using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Identity;

namespace BusinessObject.Models
{
    public class User : IdentityUser<Guid>
    {
        public required string FullName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Earning {  get; set; } = 0;
        public bool? isContributor {  get; set; }
        public bool? isWithdrawalLocked { get; set; }
        public bool? isOnboarded { get; set; }
        public string? BankAccount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public virtual Publisher Publisher { get; set; }
        public ICollection<Recap> Recaps { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<SupportTicket> SupportTickets { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
        public ICollection<Appeal> Appeals { get; set; }
        public ICollection<Category> FollowedCategories { get; set; }
        public ICollection<Author> FollowedAuthors { get; set; }
        public ICollection<Book> FollowedBooks { get; set; }
        public ICollection<PlayList> PlayLists { get; set; }
        public ICollection<Highlight> Highlights { get; set; }
        public ICollection<ReadingPosition> ReadingPositions { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<ViewTracking> ViewTrackings { get; set; }
        public ICollection<ContributorWithdrawal> ContributorWithdrawals { get; set; }
        public ICollection<ContributorPayout> ContributorPayouts { get; set; }
        public ICollection<Transaction> Transactions { get; set; }

    }
}
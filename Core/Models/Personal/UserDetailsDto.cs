using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Subscription;
using Core.Enums;

namespace Core.Models.Personal
{
    public class UserDetailsDto
    {
        public Guid Id { get; set; }

        public string? UserName { get; set; }

        public string? FullName { get; set; }

        public Gender? Gender { get; set; }
        public string? Address { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Email { get; set; }
        public string? BankAccount { get; set; }
        public decimal? Earning { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? PhoneNumber { get; set; }
        public bool isContributor { get; set; }
        public bool isOnboarded { get; set; }

        public bool? PhoneNumberConfirmed { get; set; }
        public string? ImageUrl { get; set; }
        public Roles? RoleType { get; set; }
        public bool? IsAccountLocked { get; set; }
        public ICollection<ViewSubscription> Subscriptions { get; set; }
    }
}
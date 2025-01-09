using BusinessObject.Enums;

namespace Core.Models.UserModels
{
    public class GetAllUser
    {
        public required string FullName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
    }
}

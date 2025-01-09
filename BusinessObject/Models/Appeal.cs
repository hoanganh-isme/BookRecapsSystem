using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Appeal : BaseEntity
    {
        public Guid? ContributorId { get; set; }
        public Guid? StaffId { get; set; }
        public Guid ReviewId { get; set; }
        public string Reason { get; set; }
        public string? Response {  get; set; }
        public AppealStatus AppealStatus { get; set; }
        public Review Review { get; set; }
        public User? Staff {  get; set; }
        public User? Contributor { get; set; }
    }
}

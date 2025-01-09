using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Review
{
    public class ReviewDTO
    {
        public Guid? StaffId { get; set; }
        public Guid RecapVersionId { get; set; }
        public string Comments { get; set; }
        public string StaffName { get; set; }
        public RecapVersion RecapVersion { get; set; }
        public ICollection<ReviewNote> ReviewNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

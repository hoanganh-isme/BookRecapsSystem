using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Review : BaseEntity
    {
        public Guid? StaffId { get; set; }
        public Guid RecapVersionId {  get; set; }
        public string Comments {  get; set; }
        public User? Staff { get; set; }
        public RecapVersion RecapVersion { get; set; }
        public ICollection<ReviewNote> ReviewNotes { get; set; }
        public ICollection<Appeal> Appeals { get; set;}

    }
}

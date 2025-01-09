using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class CategoryStaff
    {
        public Guid StaffId { get; set; }
        public Guid CategoryId { get; set; }
        public User Staff {  get; set; }
        public Category Category { get; set; }
    }
}

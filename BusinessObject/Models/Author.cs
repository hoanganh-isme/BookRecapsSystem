using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Author : BaseEntity
    {
        public string Name { get; set; }
        public string Image {  get; set; }
        public string Description { get; set; }
        public ICollection<Book> Books { get; set; }
        public ICollection<User> Users { get; set; }

    }
}

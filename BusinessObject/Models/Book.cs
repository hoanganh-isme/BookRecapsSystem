using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Book : BaseEntity
    {
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Description { get; set; }
        public int PublicationYear {  get; set; }
        public string CoverImage {  get; set; }
        public int AgeLimit {  get; set; }
        public string? ISBN_13 {  get; set; }
        public string? ISBN_10 {  get; set; }
        public Guid? PublisherId { get; set; } 
        public Publisher? Publisher { get; set; }
        public ICollection<Author> Authors { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<Recap> Recaps { get; set; }
        public ICollection<BookEarning> BookEarnings { get; set; }
        public ICollection<Contract> Contracts { get; set; }
        public ICollection<User> Users { get; set; }

    }
}

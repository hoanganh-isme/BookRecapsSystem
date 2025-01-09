using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Books
{
    public class BookContributorDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Description { get; set; }
        public int PublicationYear { get; set; }
        public string CoverImage { get; set; }
        public string? ISBN_13 { get; set; }
        public string? ISBN_10 { get; set; }
        public int AgeLimit { get; set; }
        public decimal ContributorSharePercentage { get; set; }
        public int TotalPublishedRecaps {  get; set; }
        public Guid? PublisherId {  get; set; }
        public string PublisherName { get; set; } 
        public List<string> AuthorNames { get; set; }
        public List<Guid> CategoryIds { get; set; }
        public List<string> CategoryNames { get; set; }  
    }
}

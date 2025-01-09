using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Books
{
    public class BookUpdateRequest
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? OriginalTitle { get; set; }
        public string? Description { get; set; }
        public int PublicationYear { get; set; }
        public string? CoverImage { get; set; }
        public int AgeLimit { get; set; }
        public List<Guid>? CategoryIds { get; set; }
    }
}

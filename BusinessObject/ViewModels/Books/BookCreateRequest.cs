using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Books
{
    public class BookCreateRequest
    {
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string? ISBN_13 { get; set; }
        public string? ISBN_10 { get; set; }
        public string Description { get; set; }
        public int PublicationYear { get; set; }
        public string? CoverImage { get; set; }
        public int AgeLimit { get; set; }
        public List<AuthorRequest> Authors { get; set; } = new List<AuthorRequest>();
        public List<Guid> CategoryIds { get; set; } // Cho phép nhiều thể loại
    }
    public class AuthorRequest
    {
        public Guid? Id { get; set; } // ID tác giả (nếu đã tồn tại)
        public string? Name { get; set; } // Tên tác giả (nếu tạo mới)
        public string? Description { get; set; }
        public string? Image { get; set; }
    }


}

using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Books
{
    public class BooksDTO
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
        public string PublisherName { get; set; }  // Chỉ trả về tên publisher
        public List<string> AuthorNames { get; set; }  // Chỉ trả về danh sách tên tác giả
        public List<string> CategoryNames { get; set; }  // Chỉ trả về danh sách tên thể loại
    }

}

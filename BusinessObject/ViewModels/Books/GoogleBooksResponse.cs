using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Books
{
    public class GoogleBooksResponse
    {
        public string Kind { get; set; }
        public int TotalItems { get; set; }
        public List<BookItem> Items { get; set; }
    }

    public class BookItem
    {
        public string Id { get; set; }
        public VolumeInfo VolumeInfo { get; set; }
    }

    public class VolumeInfo
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string PublishedDate { get; set; }
        public string? Description { get; set; }
        public List<IndustryIdentifier> IndustryIdentifiers { get; set; }
    }

    public class IndustryIdentifier
    {
        public string Type { get; set; }
        public string Identifier { get; set; }
    }


}

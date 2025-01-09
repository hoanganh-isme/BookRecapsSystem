using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Viewtrackings
{
    public class ViewTrackingDTO
    {
        public Guid RecapId { get; set; }
        public string RecapName { get; set; }
        public bool isPublished { get; set; }
        public bool isPremium { get; set; }
        public int? LikesCount { get; set; }
        public int? ViewsCount { get; set; }
        public string ContributorName {  get; set; }
        public string ContributorImage {  get; set; }
        public BookDTO Book { get; set; }
        public int? Durations { get; set; }
        public DeviceType DeviceType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BookDTO
    {
        public Guid bookId { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string CoverImage { get; set; }
        public List<string> Authors { get; set; } // Giả sử tác giả là danh sách các tên tác giả
    }

}

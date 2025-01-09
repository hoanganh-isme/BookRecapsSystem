using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.Dashboards
{
    public class PublisherDashboardDto
    {
        public decimal TotalIncomeFromViewTracking { get; set; }
        public decimal LastPayoutAmount { get; set; }
        public int NewRecapsCount { get; set; }
        public int OldRecapsCount { get; set; }
        public int NewViewCount { get; set; }
        public int OldViewCount { get; set; }
        public List<BookDashboardDto> Books { get; set; } // Danh sách các BookDashboardDto
    }

    public class BookDashboardDto
    {
        public Guid BookId { get; set; }           // Book ID để dễ dàng xác định sách
        public string Title { get; set; }          // Tên sách
        public string ISBN_10 {  get; set; }
        public string ISBN_13 { get; set; }
        public int PublicationYear { get; set; }   // Năm xuất bản
        public string CoverImage { get; set; }     // Hình ảnh bìa sách
        public int RecapCount { get; set; }        // Số bài recap của sách
        public decimal PaidEarnings { get; set; } // Tổng số tiền từ sách
        public decimal UnPaidEarnings { get; set; }
    }

}

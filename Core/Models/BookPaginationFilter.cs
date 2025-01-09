namespace Core.Models
{
    public class BookPaginationFilter : PaginationFilter
    {
        public Guid? CategoryId { get; set; }  // Sửa lại thành CategoryId
        public Guid? PublisherId { get; set; }  // Sửa lại PublisherId
        public decimal? ContributorRevenueShareMin { get; set; }  // Để điều kiện lọc theo phần trăm doanh thu tối thiểu của contributor

        public BookPaginationFilter() : base()
        {
        }

        // Khởi tạo constructor với các tham số
        public BookPaginationFilter(int pageNumber, int pageSize, string sortBy = null, string sortOrder = "asc",
            string searchTerm = null, string filterBy = null, string filterValue = null, Guid? categoryId = null, Guid? publisherId = null, decimal? contributorRevenueShareMin = null)
            : base(pageNumber, pageSize, sortBy, sortOrder, searchTerm)  // Gọi constructor của lớp cha PaginationFilter
        {
            CategoryId = categoryId;  // Khởi tạo các thuộc tính của BookPaginationFilter
            PublisherId = publisherId;  // Nếu publisher không null, convert thành Guid
            ContributorRevenueShareMin = contributorRevenueShareMin;  // Khởi tạo theo giá trị truyền vào
        }
    }



}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Service;

namespace Core.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [AllowAnonymous]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IBookEarningService _bookEarningService;
        private readonly IRecapEarningService _recapEarningService;

        public DashboardController(IDashboardService dashboardService, IBookEarningService bookEarningService, IRecapEarningService recapEarningService)
        {
            _dashboardService = dashboardService;
            _bookEarningService = bookEarningService;
            _recapEarningService = recapEarningService;
        }
        [HttpGet("admindashboard")]
        public async Task<IActionResult> GetAdminDashboardSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest("fromDate must be earlier than or equal to toDate.");
            }

            try
            {
                var dashboardDataResponse = await _dashboardService.GetAdminDashboardAsync(fromDate, toDate);
                if (!dashboardDataResponse.Succeeded)
                {
                    return BadRequest(new { Message = dashboardDataResponse.Message, Errors = dashboardDataResponse.Errors });
                }
                var packageSales = await _dashboardService.GetPackageSalesAsync(fromDate, toDate);
                dashboardDataResponse.Data.PackageSales = packageSales;
                return Ok(dashboardDataResponse);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework like Serilog)
                return StatusCode(500, new { Message = "An error occurred while fetching the dashboard data.", Error = ex.Message });
            }
        }
        [HttpGet("getadminchart")]
        public async Task<IActionResult> GetAdminChartAsync([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest(new { Message = "fromDate must be earlier than or equal to toDate." });
            }

            try
            {
                var response = await _dashboardService.GetAdminChartAsync(fromDate, toDate);
                if (!response.Succeeded)
                {
                    return BadRequest(response);
                }

                return Ok(response); // Trả về kết quả thống kê lượt xem
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the view chart data.", Error = ex.Message });
            }
        }
        [HttpGet("getrecapdetailforadmin/{recapId}")]
        public async Task<IActionResult> GetRecapDetailForAdmin(Guid recapId, DateTime fromDate, DateTime toDate)
        {
            var response = await _recapEarningService.GetRecapDetailForAdminAsync(recapId, fromDate, toDate);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("getbookdetailforadmin/{bookId}")]
        public async Task<IActionResult> GetBookDetailForAdmin(Guid bookId, DateTime fromDate, DateTime toDate)
        {
            var response = await _bookEarningService.GetBookDetailForAdminAsync(bookId, fromDate, toDate);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("publisherdashboard/{publisherId}")]
        public async Task<IActionResult> GetPublisherDashboardSummary(Guid publisherId)
        {           
            try
            {
                var dashboardDataResponse = await _dashboardService.GetPublisherDashboardAsync(publisherId);
                if (!dashboardDataResponse.Succeeded)
                {
                    return BadRequest(new { Message = dashboardDataResponse.Message, Errors = dashboardDataResponse.Errors });
                }
                return Ok(dashboardDataResponse);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework like Serilog)
                return StatusCode(500, new { Message = "An error occurred while fetching the dashboard data.", Error = ex.Message });
            }
        }
        [HttpGet("getpublisherchart/{publisherId}")]
        public async Task<IActionResult> GetPublisherChart(Guid publisherId, DateTime fromDate, DateTime toDate)
        {
            var response = await _dashboardService.GetPublisherChartDashboardAsync(publisherId, fromDate, toDate);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("contributordashboard/{contributorId}")]
        public async Task<IActionResult> GetContributorDashboardSummary(Guid contributorId)
        {
            try
            {
                var dashboardDataResponse = await _dashboardService.GetContributorDashboardAsync(contributorId);
                if (!dashboardDataResponse.Succeeded)
                {
                    return BadRequest(new { Message = dashboardDataResponse.Message, Errors = dashboardDataResponse.Errors });
                }
                return Ok(dashboardDataResponse);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework like Serilog)
                return StatusCode(500, new { Message = "An error occurred while fetching the dashboard data.", Error = ex.Message });
            }
        }
        [HttpGet("getcontributorchart/{contributorId}")]
        public async Task<IActionResult> GetContributorChart(Guid contributorId, DateTime fromDate, DateTime toDate)
        {
            var response = await _dashboardService.GetContributorChartDashboardAsync(contributorId, fromDate, toDate);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("getrecapdetail/{recapId}")]
        public async Task<IActionResult> GetRecapDetail(Guid recapId, DateTime fromDate, DateTime toDate)
        {
            var response = await _recapEarningService.GetRecapDetailAsync(recapId, fromDate, toDate);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("getbookdetail/{bookId}")]
        public async Task<IActionResult> GetBookDetail(Guid bookId, DateTime fromDate, DateTime toDate)
        {
            var response = await _bookEarningService.GetBookDetailAsync(bookId, fromDate, toDate);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}

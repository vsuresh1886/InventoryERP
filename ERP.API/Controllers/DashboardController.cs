using ERP.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class DashboardController : Controller
    {
        public readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("Card-summary")]
        public async Task<IActionResult> GetInventorySummary()
        {
           
                var summary = await _dashboardService.GetCardSummaryAsync();
                if (summary != null)
                {
                    return Ok(new { success = true, message = "Quotation Id's  Fetched Successfully", data = summary });
                }
                else
                {
                    return Unauthorized();
                }
           

        }


    }
}

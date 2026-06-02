using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class AIController : ControllerBase
    {

        private readonly IAiService _aiService;

        public AIController(AiService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] AiSearchRequestDto request)
        {
            if (request.Module == "quotation")
            {
                var filters =
                    await _aiService
                        .ParseQuotationQuery(
                            request.Query);

                return Ok(
                    ApiResponseHelper.Success(
                        filters,
                        "AI filters generated"
                    )
                );
            }

            return BadRequest(
                ApiResponseHelper.Fail<object>(
                    "Unsupported module"
                )
            );
        }

    }
}

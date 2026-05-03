using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Quotation;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/{controller}")]
    public class SalesController : ControllerBase
    {
        public readonly ISalesService _salesService;

        public SalesController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        [HttpGet("getInvoiceids")]
        public async Task<IActionResult> getInvoiceids()
        {
            try
            {
                var result = await _salesService.FetchInvoiceids();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Sales/Invoice Id's  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(new ApiResponse<object>(
                                 false,
                                 "Invalid  configuration",
                                 null
                            ));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getInvoices")]
        public async Task<IActionResult> getInvoices([FromQuery] InvoiceGridRequestDto invdto)
        {
            try
            {
                var result = await _salesService.FetchInvoices(invdto);
                if (result != null)
                {
                    return Ok(new { success = true, message = "Quotation Id's  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(new ApiResponse<object>(
                                 false,
                                 "Invalid  configuration",
                                 null
                            ));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getInvoiceById")]
        public async Task<IActionResult> getInvoiceById([FromQuery] long id)
        {
            var result = await _salesService.getInvoiceById(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpPost("CreateUpdateInvoice")]
        public async Task<IActionResult> CreateUpdateInvoice(InvoiceDto InvoiceItem)
        {
            var result = await _salesService.CreateUpdateInvoice(InvoiceItem);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }
    }
}

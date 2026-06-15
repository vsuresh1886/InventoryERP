using ERP.Application.DTOs;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Interfaces.Repositories;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class SalesReturnController : Controller
    {
        public readonly ISalesReturnService _salesrtnService;

        public SalesReturnController(ISalesReturnService salesrtnService)
        {
            _salesrtnService = salesrtnService;
        }

        [HttpGet("getCustInv")]
        public async Task<IActionResult> getCustInv([FromQuery] long id)
        {
            try
            {
                var result = await _salesrtnService.FetchInvoiceCust(id);
                if (result != null)
                {
                    return Ok(new { success = true, message = "Sales/Invoice Id's  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized();
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
            var result = await _salesrtnService.getInvoiceById(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpGet("getReturnsById")]
        public async Task<IActionResult> getReturnsById([FromQuery] long id)
        {
            var result = await _salesrtnService.getReturnsById(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }




        [HttpGet("getSalesReturns")]
        public async Task<IActionResult> getSalesReturns([FromQuery] SRGridRequestsDto invdto)
        {
            try
            {
                var result = await _salesrtnService.FetchReturndtls(invdto);
                if (result != null)
                {
                    return Ok(new { success = true, message = "Quotation Id's  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpPost("CreateUpdateSaleRtn")]
        public async Task<IActionResult> CreateUpdateSaleRtn(SalesReturnDto InvoiceItem)
        {
            var result = await _salesrtnService.CreateUpdateSaleReturn(InvoiceItem);
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

using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Quotation;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationController : Controller
    {
        public readonly IQuotationService _quotationService;
        public readonly IPdfService _pdfservice;
        
        public QuotationController(IQuotationService quotationService,IPdfService pedfservice)
        {
            _quotationService = quotationService;
            _pdfservice = pedfservice;
        }



        [HttpGet("getQuotationids")]
        public async Task<IActionResult> getQuotationids()
        {
            try
            {
                var result = await _quotationService.Fetchquotationid();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Quotation Id's  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Quutation"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getQuotations")]
        public async Task<IActionResult> getQuotations([FromQuery] QuotationGridRequestDto quotationdto)
        {
            try
            {
                var result = await _quotationService.Fetchquotations(quotationdto);
                if (result != null)
                {
                    return Ok(new { success = true, message = "Quotation Id's  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Quotation"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }




        [HttpPost("CreateUpdateQuot")]
        public async Task<IActionResult> CreateUpdateQuot(QuotationDto QuotationItem)
        {
            var result = await _quotationService.CreateUpdateQuot(QuotationItem);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpGet("getQuotationById")]
        public async Task<IActionResult> getQuotationById([FromQuery] string id)
        {
            var result = await _quotationService.getQuotationbyid(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> getPdf(int id)
        {
            var model = await _quotationService.quotationpdfdata(id);
            var pdf =  _pdfservice.GenerateQuotationPdf(model);
            return File(pdf, "application/pdf", $"Quotation1.pdf");
        }
       
    }
}

using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.Notification;
using ERP.Application.Models;
using ERP.Application.Models.Notification;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class SoaController : Controller
    {
        public readonly ISoaService _soaservice;
        public readonly IExcelService _excelservice;
        public readonly IPdfService _pdfservice;
        private readonly IEmailService  _emailService;
        public SoaController(ISoaService soaservice, IExcelService excelService, IPdfService pdfservice,IEmailService emailService )
        {
            _soaservice = soaservice;
            _excelservice = excelService;
            _pdfservice = pdfservice;
            _emailService = emailService;
        }

        [HttpGet("CustSOA")]
        public async Task<IActionResult> GetCustSOA([FromQuery] CustomerSOAFilterDto filters)
        {
            var result = await _soaservice.FetchCustomer_rep(filters);

            return Ok(
                     ApiResponseHelper.Success(
                         result,
                         "Cust SOA generated"
                     ));

        }

        [HttpGet("CustSOA_Excel")]
        public async Task<IActionResult> CustSOA_Excel([FromQuery] CustomerSOAFilterDto filters)
        {
            var result = await _soaservice.FetchCustomer_rep(filters);

            var bytes = await _excelservice.Gen_customerSOA(result); //     _excelService.GenerateExcel(result);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"CustomerSOA_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );

        }

        [HttpGet("CustSOA_PDF")]
        public async Task<IActionResult> CustSOA_PDF([FromQuery] CustomerSOAFilterDto filters)
        {
            var result = await _soaservice.FetchCustomer_rep(filters);

            var pdfbytes = await _pdfservice.Gen_customerSOAPdf(result); //     _excelService.GenerateExcel(result);

            return File(
                         pdfbytes,
                         "application/pdf",
                         $"CustomerSOA_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                     );

        }

        [HttpPost("email")]
        public async Task<IActionResult> TestEmail()

        {
            //await _emailService.SendAsync( );

            return Ok(
                "Email sent successfully"
            );
        }

        //for single customer email. 
        [HttpPost("CustSOA_Email")]
        public async Task<IActionResult> CustSOA_Email([FromBody] CustomerSOAFilterDto filters)
        {
            var result = await _soaservice.EmailCustomer_rep(filters);
            return Ok(
                    ApiResponseHelper.Success(
                        result,
                        "Cust email generated"
                    ));

        }



    }
}

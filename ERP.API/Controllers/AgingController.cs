using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.Notification;
using ERP.Application.Models;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class AgingController : ControllerBase
    {
        public readonly IAgingService _agingservice;
        public readonly IExcelService _excelservice;
        public readonly IPdfService _pdfservice;
        public readonly IEmailService _emailservice;
        public AgingController(IAgingService agingservice, IExcelService excelservice, IPdfService pdfservice, IEmailService emailservice)
        {
            _agingservice = agingservice;
            _excelservice = excelservice;
            _pdfservice = pdfservice;
            _emailservice = emailservice;
        }

        [HttpGet("CustAging")]
        public async Task<IActionResult> GetCustAging([FromQuery] CustomerAgingFilterDto filters)
        {
            var result = await _agingservice.CustomerAging_rep(filters);

            return Ok(
                     ApiResponseHelper.Success(
                         result,
                         "Cust SOA generated"
                     ));

        }

        [HttpGet("CustAging_Excel")]
        public async Task<IActionResult> CustSOA_Excel([FromQuery] CustomerAgingFilterDto filters)
        {
            var result = await _agingservice.CustomerAging_rep(filters);

            var bytes = await _excelservice.Gen_customerAging(result); //     _excelService.GenerateExcel(result);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"CustomerAging_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );

        }

        [HttpGet("CustAging_PDF")]
        public async Task<IActionResult> CustSOA_PDF([FromQuery] CustomerAgingFilterDto filters)
        {
            var result = await _agingservice.CustomerAging_rep(filters);

            var pdfbytes = await _pdfservice.Gen_customerAgingPdf(result); //     _excelService.GenerateExcel(result);

            return File(
                         pdfbytes,
                         "application/pdf",
                         $"CustomerAging_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                     );

        }



        //for single customer email. 
        [HttpPost("CustAging_Email")]
        public async Task<IActionResult> CustAging_Email([FromBody] CustomerAgingFilterDto filters)
        {
            var result = await _agingservice.CustomerAgingEmail_rep(filters);
            return Ok(
                     ApiResponseHelper.Success(
                         result,
                         "email sent successfully"
                     ));


        }



    }
}

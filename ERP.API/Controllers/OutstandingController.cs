using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class OutstandingController : Controller
    {

        private readonly IOutstandingService _outstandingservice;
        private readonly IPdfService _pdfService;
        private readonly IExcelService _excelService;

        public OutstandingController(IOutstandingService outstandingService, IExcelService excelService,IPdfService pdfService)
        {
            _outstandingservice = outstandingService;
            _excelService = excelService;
            _pdfService = pdfService;
        }
        [HttpGet("CustOutstanding")]
        public async Task<IActionResult> GetCustSOA([FromQuery] ReceivableOutstandingFiltersDto filters)
        {
            var result = await _outstandingservice.FetchCustomer_OS(filters);

            return Ok(
                     ApiResponseHelper.Success(
                         result,
                         "Cust Outstanding generated"
                     ));

        }
        [HttpGet("CustOS_Excel")]
        public async Task<IActionResult> CustSOA_Excel([FromQuery] ReceivableOutstandingFiltersDto filters)
        {
            var result = await _outstandingservice.FetchCustomer_OS(filters);

            var bytes = await _excelService.Gen_customerOS(result); //     _excelService.GenerateExcel(result);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"CustomerSOA_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );

        }

        [HttpGet("CustOS_PDF")]
        public async Task<IActionResult> CustSOA_PDF([FromQuery] ReceivableOutstandingFiltersDto filters)
        {
            var result = await _outstandingservice.FetchCustomer_OS(filters);

            var pdfbytes = await _pdfService.Gen_customerOSPdf(result); //     _excelService.GenerateExcel(result);

            return File(
                         pdfbytes,
                         "application/pdf",
                         $"CustomerSOA_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                     );

        }


        //for single customer email. 
        [HttpPost("CustOS_Email")]
        public async Task<IActionResult> CustSOA_Email([FromBody] ReceivableOutstandingFiltersDto filters)
        {
            var result = await _outstandingservice.EmailCustomer_OS(filters);
            return Ok(
                    ApiResponseHelper.Success(
                        result,
                        "Cust email generated"
                    ));

        }

    }
}

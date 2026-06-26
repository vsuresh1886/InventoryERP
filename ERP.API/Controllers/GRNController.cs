using ERP.Application.DTOs.GoodsReceiptNote;
using ERP.Application.DTOs.Purchase;
using ERP.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class GRNController : ControllerBase
    {
        public readonly IGRNService _grnService;

        public GRNController(IGRNService grnService)
        {
            _grnService = grnService;
        }

        [HttpGet("getGRNids")]
        public async Task<IActionResult> getGRNids()
        {
            var result = await _grnService.getGRNids();
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpGet("getgrns")]
        public async Task<IActionResult> getgrns([FromQuery] GRNGridRequestDto podtls)
        {
            var result = await _grnService.GetGRNGrid(podtls);
            if (result != null)
            {
                return Ok(new { success = true, message = "Purchase order Id's  Fetched Successfully", data = result });
            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpGet("getPODet")]
        public async Task<IActionResult> getPODet([FromQuery] long poId)
        {
            var result = await _grnService.GetPOForGRNByIdAsync(poId);
            if (result != null)
            {
                return Ok(new { success = true, message = "Purchase order Id's  Fetched Successfully", data = result });
            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpPost("Createupdategrn")]
        public async Task<IActionResult> Createupdatepo([FromBody] GoodsReceiptDto grndto)
        {
            var result = await _grnService.CreateupdateGRN(grndto);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }



        [HttpGet("getGRNById")]
        public async Task<IActionResult> getGRNById(long id)
        {
            var result = await _grnService.GetGRNById(id);
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

using ERP.Application.DTOs.Purchase;
using ERP.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class PurchaseOrderController : ControllerBase
    {
        public readonly IPurchaseOrderSerice _purchaseOrder;

        public PurchaseOrderController(IPurchaseOrderSerice purchaseOrder)
        {
            _purchaseOrder = purchaseOrder; 
        }

        [HttpGet("getpos")]
        public async Task<IActionResult> getpos([FromQuery]PurchaseGridRequestDto podtls)
        {
            var result = await _purchaseOrder.GetPurchaseOrderGrid(podtls);
            if (result != null)
            {
                return Ok(new { success = true, message = "Purchase order Id's  Fetched Successfully", data = result });
            }
            else
            {
                return Unauthorized();
            }

        }
        [HttpGet("getposids")]
        public async Task<IActionResult> getposids()
        {
            var result = await _purchaseOrder.GetPoIds();
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpPost("Createupdatepo")]
        public async Task<IActionResult> Createupdatepo([FromBody]PurchaseOrderDto podto)
        {
            var result = await _purchaseOrder.Createupdatepo(podto);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpGet("Getpobyid")]
        public async Task<IActionResult> Getpobyid (long id)
        {
            var result = await _purchaseOrder.GetPurchaseOrderById(id);
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

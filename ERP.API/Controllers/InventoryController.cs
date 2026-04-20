using ERP.Application.DTOs.Inventory;
using ERP.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : Controller
    {

        public readonly IInventoryService _inventoryservice;
        
        public InventoryController(IInventoryService inventoryservice)
        {
            _inventoryservice = inventoryservice;
        }

        [HttpGet("getinventorys")]
        public async Task<IActionResult> Getinventory([FromQuery] InvDataGridRequestDto gridRequestDto)
        {
            var result = await _inventoryservice.Getinventory(gridRequestDto);
            if(result!=null)
            {
                return Ok(result);
            }else
            {
                return BadRequest("Bad request");
            }
        }

        [HttpGet("getInvdtl")]
        public async Task<IActionResult> GetInvdtl(int id)
        {
            var result= await _inventoryservice.GetInvdtl(id);
            if (result != null)
                return Ok(result);
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpPost("Invitem")]
        public async Task<IActionResult> CreateUpdateInv(InventoryItemDto inventoryItem)
        {
            var result = await _inventoryservice.CreateUpdateInv(inventoryItem);
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

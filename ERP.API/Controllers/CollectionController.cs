using ERP.Application.DTOs.Accounts;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Quotation;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/{controller}")]
    public class CollectionController : ControllerBase
    {
        public readonly ICollectionService _collectionService;

        public CollectionController(ICollectionService collectionservice)
        {
            _collectionService = collectionservice;
        }

        [HttpGet("getCollectionids")]
        public async Task<IActionResult> getCollectionids()
        {
            try
            {
                var result = await _collectionService.FetchCollectionids();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Sales/Invoice Id's  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized("NULL");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }


        [HttpGet("FetchCollections")]
        public async Task<IActionResult> FetchCollections([FromQuery]CollectionGridDto collectiondto)
        {
            var result = await _collectionService.FetchCollections(collectiondto);
            if(result != null)
            {
                return Ok(new { success = true, message = "Collections details  Fetched Successfully", data = result });
            }
            else
            {
                return Unauthorized();
            }

        }

        
        [HttpGet("getCollectionById")]
        public async Task<IActionResult> getCollectionById([FromQuery] long id)
        {
            var result = await _collectionService.FetchCollectionById(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpPost("CreateUpdateCollection")]
        public async Task<IActionResult> CreateUpdateCollection(CollectionSaveDto collectionItem)
        {
            var result = await _collectionService.CreateUpdateCollection(collectionItem);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        }

        [HttpGet("outstanding-invoices")]
        public async Task<IActionResult> GetOutstandingbyCustomer([FromQuery] long id)
        {
            var result = await _collectionService.GetOutstandingbyCustomer(id);
            if(result != null)
            {
                return Ok(new { success = true, message = "Pending Invoices Fetched Successfully", data = result });
            }
            else

            {
                return BadRequest("Bad Request");
            }
        }

    }
}

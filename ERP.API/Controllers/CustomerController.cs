using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerservice;

        public CustomerController(ICustomerService customerservice) 
        {
            _customerservice = customerservice;
        }


        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers([FromQuery] CDataGridRequestDto requestDto)
        {
            try
            {
                var result = await _customerservice.FetchGridData(requestDto);
                if (result != null)
                {
                    // return Ok(new { success = true, message = "Grid Data Fetched Successfully", result });
                    return Ok(result);
                }
                else
                {
                    return Unauthorized(new ApiResponse<object>(
                                 false,
                                 "Invalid Grid configuration",
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
        [HttpGet("GetCustomer")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var result = await _customerservice.FetchCustomer(id);
            if(result !=null)
            {
                return Ok(result);

            }
            else
            {
                return NotFound(new ApiResponse<object>(
                                 false,
                                 "Invalid customer",
                                 null
                            ));
            }

        }
        [HttpPost("Customer")]
        public async Task<IActionResult> CreateUpdateCustomer([FromBody] CustomerDto customer)
        {
            var result = await _customerservice.CreateUpdateCustomer(customer);
            if(result != null)
                return Ok(result);
            return Ok(new ApiResponse<object>(
                                 false,
                                 "Invalid customer",
                                 null
                            ));
        }

    }
}

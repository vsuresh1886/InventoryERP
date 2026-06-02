using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public UserController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [HttpGet("griddata")]
        public async Task<IActionResult> griddata([FromQuery] UDataGridRequestDto  uDataGridRequest)
        {
            try
            {
                var result = await _userService.FetchGridData(uDataGridRequest);

                if (result != null)
                {
                   // return Ok(new { success = true, message = "Grid Data Fetched Successfully", result });
                    return Ok(result);
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid Data"));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser([FromQuery] int employeeCode)
        {
            var result = await _userService.FetchUser(employeeCode);
            if(result !=null)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(ApiResponseHelper.Fail<object>("Invalid User"));
            }
        }

        [HttpPost("Saveuser")]
        public async Task<IActionResult> SaveUser(EmployeeSaveDto employeeDto)
        {
            var result = await _userService.CreateUpdateUser(employeeDto) ;
            if(result !=null)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(ApiResponseHelper.Fail<object>("Somnething went wrong"));
            }
        }
        
    }
}

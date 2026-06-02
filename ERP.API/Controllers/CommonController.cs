using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using Microsoft.AspNetCore.Mvc;
using static ERP.Application.DTOs.Auth;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CommonController : ControllerBase
    {
        private readonly ICommonService _commonService;
        private readonly IConfiguration _config;

        public CommonController(ICommonService userService, IConfiguration config)
        {
            _commonService = userService;
            _config = config;
        }

        [HttpGet("gridheader")]
        public async Task<IActionResult> gridheader([FromQuery] string formName, [FromQuery] string gridId)
        {
            try
            {
                var result = await _commonService.FetchGridHeader(formName, gridId);

                if(result != null)
                {
                    return Ok(new { success = true, message = "Grid Data Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception  ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getRoles")]
        public async Task<IActionResult> getRoles()
        {
            try { 
            var result = await _commonService.FetchRoles();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Roles Fetched Successfully", data =result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }
        [HttpGet("getDepartments")]
        public async Task<IActionResult> getDepartments()
        {
            try
            {
                var result = await _commonService.FetchDepartment();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Department Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>( "Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }
        [HttpGet("getStatus")]
        public async Task<IActionResult> getStatus()
        {
            try
            {
                var result = await _commonService.FetchStatus();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Status Fetched Successfully", data= result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }
        [HttpGet("getStatuses")]
        public async Task<IActionResult> getStatuses(int form_id, string type)
        {
            try
            {
                var result = await _commonService.FetchStatuses(form_id, type);
                if (result != null)
                {
                    return Ok(new { success = true, message = "Status Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getCountries")]
        public async Task<IActionResult> getCountries()
        {
            try
            {
                var result = await _commonService.FetchCountry();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Status Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getPartytype")]
        public async Task<IActionResult> getPartytype()
        {
            try
            {
                var result = await _commonService.FetchPartyType();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Party Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }
        #region "Inventory"

        [HttpGet("getDomain")]
        public async Task<IActionResult> getDomain()
        {
            try
            {
                var result = await _commonService.FetchDomain();
                if (result != null)
                {
                    return Ok(new { success = true, message = "domain Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getCategory")]
        public async Task<IActionResult> getCategory()
        {
            try
            {
                var result = await _commonService.FetchCategory();
                if (result != null)
                {
                    return Ok(new { success = true, message = "category Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getUnits")]
        public async Task<IActionResult> getUnits()
        {
            try
            {
                var result = await _commonService.FetchUnits();
                if (result != null)
                {
                    return Ok(new { success = true, message = "category Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getSupplier")]
        public async Task<IActionResult> getSupplier()
        {
            try
            {
                var result = await _commonService.FetchSupplier();
                if (result != null)
                {
                    return Ok(new { success = true, message = "supplier  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }
        [HttpGet("getSubcategory")]
        public async Task<IActionResult> getSubcategory()
        {
            try
            {
                var result = await _commonService.FetchSubcategory();
                if (result != null)
                {
                    return Ok(new { success = true, message = "supplier  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }
        #endregion

        #region "quotation"
        [HttpGet("getCustomer")]
        public async Task<IActionResult> getCustomer()
        {
            try
            {
                var result = await _commonService.FetchCustomer();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Customer  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }

        [HttpGet("getSalesperson")]
        public async Task<IActionResult> getSalesperson()
        {
            try
            {
                var result = await _commonService.FetchSalesperson();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Customer  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }


        [HttpGet("getItems")]
        public async Task<IActionResult> getItems()
        {
            try
            {
                var result = await _commonService.FetchInvItems();
                if (result != null)
                {
                    return Ok(new { success = true, message = "Customer  Fetched Successfully", data = result });
                }
                else
                {
                    return Unauthorized(ApiResponseHelper.Fail<object>("Invalid Grid configuration"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User error: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });

            }
        }
        #endregion


    }
}

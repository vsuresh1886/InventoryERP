using ERP.Application.DTOs.company;
using ERP.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route ("api/{controller}")]
    public class SignupController : ControllerBase
    {
        public readonly ISignupService _signupService;

        public SignupController(ISignupService signupService)
        {
            _signupService = signupService; 
        }

        [HttpPost("signupCompany")]
        public async Task<IActionResult> signupCompany([FromBody]CompanySignupDto singupDto)
        {
            var result = await _signupService.SignupCompany(singupDto);
            if (result!= null)
            {
                return Ok(result);
            }else
            {
                return BadRequest("Bad Request");
            }

        }


    }
}

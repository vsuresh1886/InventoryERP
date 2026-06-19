using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using Microsoft.AspNetCore.Mvc;
using static ERP.Application.DTOs.Auth;


namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            if(result == null)
            {
                return Unauthorized(ApiResponseHelper.Fail<object>("Invalid email or password"));
            }

            return Ok(ApiResponseHelper.Success(result, "Login successful"));

        }       
    }
}

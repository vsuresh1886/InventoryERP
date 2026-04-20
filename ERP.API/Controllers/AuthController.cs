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
                return Unauthorized(new ApiResponse<object>(
                                 false,
                                 "Invalid email or password",
                                 null
                            ));
            }

            return Ok(new ApiResponse<AuthResponseDto>(
                                    true,
                                    "Login successful",
                                    result
                                ));

        }       
    }
}

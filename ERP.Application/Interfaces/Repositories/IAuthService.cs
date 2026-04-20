using System;
using System.Collections.Generic;
using System.Text;
using static ERP.Application.DTOs.Auth;


namespace ERP.Application.Interfaces.Repositories
{
    public interface IAuthService
    {
        public Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    }
}

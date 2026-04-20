using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ERP.Application.Interfaces.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using ERP.Application.DTOs;
using static ERP.Application.DTOs.Auth;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using ERP.Domain.Entities;

namespace ERP.Infrastructure.Repositories
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AuthDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.employee_masters.FirstOrDefaultAsync(x => x.email == request.username);
            if (user == null || user.password != request.password)
                return null;

            var token = GenerateJwtToken(user);

            return new AuthResponseDto 
            { 
             Token = token,
             Email = user.email
            };

        }


        private string GenerateJwtToken(employee_master user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.email),
                new Claim("UserId",user.employee_pk.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static ERP.Application.DTOs.Auth;

namespace ERP.Infrastructure.Repositories
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IPasswordService passwordService, IConfiguration config)
        {
            _context = context;
            _passwordService = passwordService;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.companies
             .IgnoreQueryFilters() // IMPORTANT: Login bypasses global multi-tenant filters because we don't know the company context yet!
             .SelectMany(c => _context.employees.Where(e => e.email == request.username))
             .FirstOrDefaultAsync();
            if (user == null )
                return null;

            //  Validate the incoming plain-text password against the secure cryptographic hash stored in the DB
            bool isPasswordValid = _passwordService.VerifyPassword(request.password, user.password);
            
            if (!isPasswordValid)
                return null; // Password mismatch


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
                new Claim("CompanyId", user.company_id.ToString())
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

using ERP.Application.Models;
using ERP.Application.Models.Notification;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories;
using ERP.Infrastrucure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using Twilio;
using Microsoft.Extensions.Caching.Memory;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// 1. Add CORS service with a named policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        //policy.WithOrigins("http://localhost:5173") // frontend dev server
        policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
              //.AllowCredentials(); // only if you use cookies/auth headers
    });
});
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddHttpContextAccessor();
// DB context and DI's
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();


// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddHttpClient<AiService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("smtpSettings"));
// --- INITIALIZE TWILIO SDK ---
var twilioSection = builder.Configuration.GetSection("Twilio");
string accountSid = twilioSection["AccountSid"] ?? throw new ArgumentNullException("Twilio AccountSid is missing");
string authToken = twilioSection["AuthToken"] ?? throw new ArgumentNullException("Twilio AuthToken is missing");

TwilioClient.Init(accountSid, authToken);
// -----------------------------
var app = builder.Build();

// 2. Enable CORS before MapControllers
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler(_ => { });

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

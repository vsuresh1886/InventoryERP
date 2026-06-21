using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Cryptography;
//using Twilio.Rest.Verify.V2.Service;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using static ERP.Application.DTOs.Auth;


namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly string _verifyServiceSid;
        private readonly IMemoryCache _cache;


        public AuthController(IAuthService authService,  IConfiguration configuration, IMemoryCache  cache)
        {
            _authService = authService;
            _verifyServiceSid = configuration["Twilio:VerifyServiceSid"]
            ?? throw new ArgumentNullException("Twilio VerifyServiceSid is missing in appsettings.json");
            _cache = cache;
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

        /// <summary>
        /// Step 1: Send OTP to user phone via SMS or WhatsApp
        /// </summary>
        //[HttpPost("send-otp1")]
        //public async Task<IActionResult> SendOtp1([FromBody] OtpRequestDto request)
        //{
        //    // Normalize channel naming for Twilio ("sms" or "whatsapp")
        //    string channel = request.Channel.ToLower() == "whatsapp" ? "whatsapp" : "sms";

        //    // Twilio handles generating, hashing, and storing the code automatically
        //    var verification = await VerificationResource.CreateAsync(
        //        to: request.PhoneNumber, // E.164 formatting required (e.g., +91XXXXXXXXXX)
        //        channel: channel,
        //        pathServiceSid: _verifyServiceSid
        //    );

        //    return Ok(new { success = true, status = verification.Status });
        //}


        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] OtpRequestDto request)
        {
            // 1. Generate the random key yourself since we aren't using the 'Verify' product
            string rawCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // TODO: Save 'rawCode' to your database or an in-memory cache to check against later!

            var cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
            _cache.Set(request.PhoneNumber, rawCode, cacheOptions);

            string channel = request.Channel.ToLower();

            if (channel == "whatsapp")
            {
                // ✅ This routes directly through your joined WhatsApp Sandbox number for FREE
                var message = await MessageResource.CreateAsync(
                    body: $"Your ERP verification code is {rawCode}",
                    from: new PhoneNumber("whatsapp:+14155238886"), // The universal Twilio Sandbox number
                    to: new PhoneNumber($"whatsapp:{request.PhoneNumber}") // Must have joined your sandbox
                );
            }
            else
            {
                // Standard SMS route
                var message = await MessageResource.CreateAsync(
                    body: $"Your ERP verification code is {rawCode}",
                    from: new PhoneNumber("YOUR_TWILIO_TRIAL_SMS_NUMBER"),
                    to: new PhoneNumber(request.PhoneNumber)
                );
            }

            return Ok(new { success = true, message = "OTP sent successfully via transit gateway." });
        }

        /// <summary>
        /// Step 2: Manually check the submitted code against our in-memory store
        /// </summary>
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] OtpVerificationDto request)
        {
            // 1. Fetch the code we saved in memory for this specific phone number
            if (_cache.TryGetValue(request.PhoneNumber, out string? storedCode))
            {
                // 2. Compare what the user entered in Postman vs what we saved
                if (storedCode == request.Code)
                {
                    // Invalidate the code immediately so it can't be reused
                    _cache.Remove(request.PhoneNumber);

                    // SUCCESS! Generate your app's JWT token here
                    return Ok(new { success = true, message = "Authenticated successfully", token = "YOUR_JWT_TOKEN" });
                }
            }

            // If code doesn't match or expired out of memory cache
            return BadRequest(new { success = false, message = "Invalid or expired confirmation code." });
        }
    }



    /// <summary>
    /// Step 2: Verify user submitted OTP code
    /// </summary>
    //[HttpPost("verify-otp")]
    //public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationDto request)
    //{
    //    var verificationCheck = await VerificationCheckResource.CreateAsync(
    //        to: request.PhoneNumber,
    //        code: request.Code,
    //        pathServiceSid: _verifyServiceSid
    //    );

    //    if (verificationCheck.Status == "approved")
    //    {
    //        // The OTP matches perfectly! 
    //        // 1. Query your PostgreSQL database via your repositories to find/create the user
    //        // 2. Generate and return your ERP application's JWT session token

    //        return Ok(new { success = true, message = "Authenticated successfully", token = "GENERATED_JWT_TOKEN_HERE" });
    //    }

    //    return BadRequest(new { success = false, message = "Invalid or expired OTP code." });
    //}
}


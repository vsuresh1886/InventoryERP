using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public record OtpRequestDto(string PhoneNumber, string Channel);
    // Channel should be passed as either "sms" or "whatsapp" from the frontend

    public record OtpVerificationDto(string PhoneNumber, string Code);
}

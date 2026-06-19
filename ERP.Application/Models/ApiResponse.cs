using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Models
{
    //public  class ApiResponse<T>
    //{
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //    public T Data { get; set; }

    //    public ApiResponse(bool success, string message, T data)
    //    {
    //        Success = success;
    //        Message = message;
    //        Data = data;
    //    }
    //}


    public class ApiResponse<T>
    {
        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public List<string>? Errors { get; set; }
    }


    public static class ApiResponseHelper
    {
        public static ApiResponse<T> Success<T>(
            T data,
            string message = "Success")
        {
            return new ApiResponse<T>
            {
                Status = "success",
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail<T>(
            string message)
        {
            return new ApiResponse<T>
            {
                Status = "fail",
                Message = message,
                Data = default
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public ApiResponse()
        {
            Success = true;
            Errors = new List<string>();
        }

        public ApiResponse(T data, string message = "Thành công")
        {
            Success = true;
            Message = message;
            Data = data;
            Errors = new List<string>();
        }

        public ApiResponse(string error, bool success = false)
        {
            Success = success;
            Message = error;
            Errors = new List<string> { error };
        }

        public ApiResponse(List<string> errors)
        {
            Success = false;
            Message = "Có lỗi xảy ra";
            Errors = errors;
        }
    }
}

using System;

namespace Common.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess;
        public int StatusCode;
        public String ReturnMessage;
        public T Data;
    }
}

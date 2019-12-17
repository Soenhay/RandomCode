using Microsoft.AspNetCore.Http;//Microsoft.AspNetCore.Http.Abstractions
using System;
using System.Collections.Generic;

namespace Common.Helpers
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.statuscodes?view=aspnetcore-3.0

    public static class StatusCodeHelper
    {
        public static List<int> SuccessStatusCodes = new List<int>()
        {
              StatusCodes.Status200OK
            , StatusCodes.Status201Created
            , StatusCodes.Status202Accepted
            , StatusCodes.Status203NonAuthoritative
            , StatusCodes.Status204NoContent
            , StatusCodes.Status205ResetContent
            , StatusCodes.Status206PartialContent
            , StatusCodes.Status207MultiStatus
            , StatusCodes.Status208AlreadyReported
            , StatusCodes.Status226IMUsed
        };

        public static Boolean IsStatusOK(int code)
        {
            return SuccessStatusCodes.Contains(code);
        }
    }
}

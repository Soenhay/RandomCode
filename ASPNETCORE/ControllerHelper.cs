using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Dtos;

namespace WebApi.Helpers
{
    public static class ControllerHelper
    {
        public static ControllerInfo GetControllerInfo(this ControllerContext controllerContext)
        {
            ControllerInfo ci = new ControllerInfo();
            //ci.controllerActionDescriptor = ControllerContext.ActionDescriptor;
            //ci.clientUN = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            //ci.clientIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            //ci.AppName = _httpContextAccessor.HttpContext.Request.Headers["AppName"];

            ci.controllerName = controllerContext.ActionDescriptor.ControllerName;
            ci.actionName = controllerContext.ActionDescriptor.ActionName;
            ci.clientUsername = controllerContext.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            ci.clientUserId = 0;
            int userId;
            if (int.TryParse(controllerContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                ci.clientUserId = userId;
            }
            ci.clientIP = controllerContext.HttpContext.Connection.RemoteIpAddress.ToString();
            ci.AppName = controllerContext.HttpContext.Request.Headers["AppName"];

            return ci;
        }
    }
}

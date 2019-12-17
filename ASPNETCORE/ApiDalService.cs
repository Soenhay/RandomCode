using DataAccessLayer.Interface;
using System;
using System.Data.SqlClient;

namespace DataAccessLayer.Services
{
    public class ApiDalService : DalServiceBase, IApiDalService
    {
        public ApiDalService(string connectionString) : base(connectionString)
        {
        }

        public void spLogInsert(string type, string controller, string action, string message, string exception, string requestor, string appName)
        {
            //[api].[spLogInsert]
            //@type VARCHAR(32)
            //, @controller VARCHAR(32)
            //, @action VARCHAR(32)
            //, @thread VARCHAR(255)
            //, @level VARCHAR(50)
            //, @logger VARCHAR(255)
            //, @message VARCHAR(4000)
            //, @exception VARCHAR(2000)
            //, @RequestTime DATETIME = NULL
            //, @ResponseMillis BIGINT = NULL
            //, @StatusCode INT = NULL
            //, @Method         VARCHAR(16) = NULL
            //, @Path VARCHAR(128)  = NULL
            //, @QueryString VARCHAR(128)  = NULL
            //, @RequestBody VARCHAR(128)  = NULL
            //, @ResponseBody VARCHAR(128)  = NULL
            //, @AppName VARCHAR(32)   = NULL

            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@type", type),
                new SqlParameter("@controller", controller),
                new SqlParameter("@action",action),
                new SqlParameter("@thread", String.Empty),
                new SqlParameter("@level", String.Empty),
                new SqlParameter("@logger", String.Empty),
                new SqlParameter("@message", message),
                new SqlParameter("@exception", exception),
                new SqlParameter("@Requestor", requestor),
                new SqlParameter("@ResponseMillis", DBNull.Value),
                new SqlParameter("@StatusCode", DBNull.Value),
                new SqlParameter("@Method", DBNull.Value),
                new SqlParameter("@Path", DBNull.Value),
                new SqlParameter("@QueryString", DBNull.Value),
                new SqlParameter("@RequestBody", DBNull.Value),
                new SqlParameter("@ResponseBody", DBNull.Value),
                new SqlParameter("@AppName", appName)
            };
            db.ExecuteNonQueryAsync("api.spLogInsert", sqlParameters);
        }

        public void spLogInsert(string type, string controller, string action, string thread, string level, string logger,
            string message, string exception,
            string requestor, DateTime? requestTime = null, long responseMillis = 0, int statusCode = 0, string method = "", string path = "", string queryString = "", string requestBody = "", string responseBody = "", string appName = "")
        {

            //[api].[spLogInsert]
            //@type VARCHAR(32)
            //, @controller VARCHAR(32)
            //, @action VARCHAR(32)
            //, @message VARCHAR(255)
            //, @ResponseMillis BIGINT = NULL
            //, @StatusCode     INT = NULL
            //, @Path VARCHAR(128) = NULL
            //, @QueryString VARCHAR(128) = NULL
            //, @RequestBody VARCHAR(128) = NULL
            //, @ResponseBody VARCHAR(128) = NULL
            //, @AppName VARCHAR(32) = NULL

            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@type", type),
                new SqlParameter("@controller", controller),
                new SqlParameter("@action",action),
                new SqlParameter("@thread", thread),
                new SqlParameter("@level", level),
                new SqlParameter("@logger", logger),
                new SqlParameter("@message", message),
                new SqlParameter("@exception", exception),
                new SqlParameter("@Requestor", requestor),
                new SqlParameter("@RequestTime", (object)requestTime ?? (object)DBNull.Value),
                new SqlParameter("@ResponseMillis", (responseMillis <= 0) ? (object)DBNull.Value : (object)responseMillis),
                new SqlParameter("@StatusCode", (statusCode <= 0) ? (object)DBNull.Value : (object)statusCode),
                new SqlParameter("@Method",  String.IsNullOrEmpty(method) ? (object)DBNull.Value : (object)method),
                new SqlParameter("@Path", String.IsNullOrEmpty(path) ? (object)DBNull.Value : (object)path),
                new SqlParameter("@QueryString", String.IsNullOrEmpty(queryString) ? (object)DBNull.Value : (object)queryString),
                new SqlParameter("@RequestBody", String.IsNullOrEmpty(requestBody) ? (object)DBNull.Value : (object)requestBody),
                new SqlParameter("@ResponseBody", String.IsNullOrEmpty(responseBody) ? (object)DBNull.Value : (object)responseBody),
                new SqlParameter("@AppName", String.IsNullOrEmpty(appName) ? (object)DBNull.Value : (object)appName)
            };
            db.ExecuteNonQueryAsync("api.spLogInsert", sqlParameters);
        }
    }
}

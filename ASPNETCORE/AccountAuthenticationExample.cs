using Swashbuckle.AspNetCore.Filters;

namespace WebApi.Swagger.Examples
{
    public class AccountAuthenticationExample: IExamplesProvider<object>
    {
        public object GetExamples()
        {
            //Based on UserDto but without the unnecessary params.
            return new
            {
                Username = "Username",
                Password = "Password"
            };
        }
    }
}

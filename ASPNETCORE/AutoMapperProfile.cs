using AutoMapper;
using WebApi.Dtos.Account;
using DataAccessLayer.Entities.Account;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace WebApi.Mappings
{
    /// <summary>
    /// The automapper profile contains the mapping configuration used by the application, it enables mapping of user entities to dtos and dtos to entities.
    /// </summary>
    /// <remarks>
    /// https://jasonwatmore.com/post/2018/06/26/aspnet-core-21-simple-api-for-authentication-registration-and-user-management
    /// https://stackoverflow.com/questions/54617417/how-to-use-automapper-with-asp-net-core-2-2-api
    /// https://dotnetcoretutorials.com/2017/09/23/using-automapper-asp-net-core/
    /// </remarks>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {            
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();

            CreateMap<UserRole, UserRoleDto>();
            CreateMap<UserToken, UserTokenDto>();
        }
    }

    public static class AutoMapperProfileServicesConfiguration
    {
        /// <summary>
        /// Wrapper for configuring the AutoMapperProfile.cs class. Called in Startup.ConfigServices.
        /// </summary>
        /// <param name="services"></param>
        /// <remarks>https://dotnetcoretutorials.com/2017/01/24/servicecollection-extension-pattern/</remarks>
        public static void AddAutoMapperProfile(this IServiceCollection services)
        {
            // Auto Mapper Configurations
            //https://stackoverflow.com/questions/54344172/configuring-automapper-in-asp-net-core
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}

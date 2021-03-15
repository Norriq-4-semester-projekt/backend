using DataAccess.Interfaces;
using DataAccess.Repositories;
using DataAccess.Service;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
        }
    }
}
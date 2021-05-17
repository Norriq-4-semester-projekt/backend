using DataAccess.Interfaces;
using DataAccess.Repositories;
using DataAccess.Service;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    public static class ServiceRegistration
    {
        //Tilføjer services UserRepo og UnitOfWork
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<INetworkRepository, NetworkRepository>();
            services.AddTransient<IHttpStatusRepository, HttpStatusRepository>();
            services.AddTransient<ICpuRepository, CpuRepository>();
            services.AddTransient<IMemoryRepository, MemoryRepository>();
            services.AddTransient<ISystemLoadRepository, SystemLoadRepository>();
            services.AddTransient<ILogDetectionRepository, LogDetectionRepository>();
            services.AddTransient<ITestClass, TestClassRepository>();
        }
    }
}
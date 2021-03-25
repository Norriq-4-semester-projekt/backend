using DataAccess.Interfaces;

namespace DataAccess.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; }
        public IDataRepository Data { get; }

        public UnitOfWork(IUserRepository userRepository, IDataRepository dataRepository)
        {
            Users = userRepository;
            Data = dataRepository;
        }
    }
}
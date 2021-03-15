using DataAccess.Interfaces;

namespace DataAccess.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; }

        public UnitOfWork(IUserRepository userRepository)
        {
            Users = userRepository;
        }
    }
}
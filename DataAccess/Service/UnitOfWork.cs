using DataAccess.Interfaces;

namespace DataAccess.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; }
        public IDataRepository Data { get; }
        public IHttpStatusRepository HttpStatus { get; set; }

        public UnitOfWork(IUserRepository userRepository, IDataRepository dataRepository, IHttpStatusRepository httpstatusRepository)
        {
            Users = userRepository;
            Data = dataRepository;
            HttpStatus = httpstatusRepository;
        }
    }
}
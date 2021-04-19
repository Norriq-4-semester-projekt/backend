using DataAccess.Service;

namespace DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        public IUserRepository Users { get; }
        public IDataRepository Data { get; }
        public IHttpStatusRepository HttpStatus { get; set; }
    }
}
using DataAccess.Service;

namespace DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        public IUserRepository Users { get; }
        public INetworkRepository Data { get; }
        public IHttpStatusRepository HttpStatus { get; set; }
        public ICpuRepository CpuData { get; }
        public IMemoryRepository MemoryData { get; }

    }
}
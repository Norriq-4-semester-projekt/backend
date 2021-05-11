using DataAccess.Interfaces;

namespace DataAccess.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; }
        public INetworkRepository NetworkData { get; }
        public ICpuRepository CpuData { get; set; }
        public IHttpStatusRepository HttpStatus { get; set; }
        public IMemoryRepository MemoryData { get; set; }
        public ISystemLoadRepository SystemLoadData { get; set; }
        public ILogDetectionRepository DetectionLogging { get; set; }

        public UnitOfWork(IUserRepository userRepository,
                          INetworkRepository networkRepository,
                          IHttpStatusRepository httpstatusRepository,
                          IMemoryRepository memoryRepository,
                          ISystemLoadRepository systemloadRepository,
                          ICpuRepository cpuRepository,
                          ILogDetectionRepository logDetectionRepository)
        {
            Users = userRepository;
            NetworkData = networkRepository;
            HttpStatus = httpstatusRepository;
            CpuData = cpuRepository;
            MemoryData = memoryRepository;
            SystemLoadData = systemloadRepository;
            DetectionLogging = logDetectionRepository;
        }
    }
}
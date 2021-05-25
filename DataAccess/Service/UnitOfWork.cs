using DataAccess.Interfaces;

namespace DataAccess.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; }
        public INetworkRepository NetworkData { get; }
        public ICpuRepository CpuData { get; }
        public IMemoryRepository MemoryData { get; }
        public ISystemLoadRepository SystemLoadData { get; }
        public ILogDetectionRepository DetectionLogging { get; set; }

        public ITestClass TestClass { get; set; }

        public UnitOfWork(IUserRepository userRepository,
                          INetworkRepository networkRepository,
                          IMemoryRepository memoryRepository,
                          ISystemLoadRepository systemLoadRepository,
                          ICpuRepository cpuRepository,
                          ILogDetectionRepository logDetectionRepository,
                          ITestClass testClass)
        {
            Users = userRepository;
            NetworkData = networkRepository;
            CpuData = cpuRepository;
            MemoryData = memoryRepository;
            SystemLoadData = systemLoadRepository;
            DetectionLogging = logDetectionRepository;
            TestClass = testClass;
        }
    }
}
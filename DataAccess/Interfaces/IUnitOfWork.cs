namespace DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        public IUserRepository Users { get; }
        public INetworkRepository NetworkData { get; }
        public ICpuRepository CpuData { get; }
        public IMemoryRepository MemoryData { get; }
        public ISystemLoadRepository SystemLoadData { get; }
        public ILogDetectionRepository DetectionLogging { get; }
        public ITestClass TestClass { get; }
    }
}
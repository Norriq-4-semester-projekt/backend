using DataAccess.Interfaces;

namespace DataAccess.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; }
        public IDataRepository Data { get; }
        public ICpuRepository CpuCalc { get; set; }
        public IHttpStatusRepository HttpStatus { get; set; }

        public UnitOfWork(IUserRepository userRepository,
                          IDataRepository dataRepository,
                          IHttpStatusRepository httpstatusRepository,
                          ICpuRepository cpuRepository)
        {
            Users = userRepository;
            Data = dataRepository;
            HttpStatus = httpstatusRepository;
            CpuCalc = cpuRepository;
        }
    }
}
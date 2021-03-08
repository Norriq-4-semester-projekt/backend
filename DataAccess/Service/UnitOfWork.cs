using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

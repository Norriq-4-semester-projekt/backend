namespace DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        public IUserRepository Users { get; }
    }
}
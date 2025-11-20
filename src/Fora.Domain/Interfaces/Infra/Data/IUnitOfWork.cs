namespace Fora.Domain.Interfaces.Infra.Data
{
    public interface IUnitOfWork
    {
        void BeginTransaction();
        void Commit();
        Task CommitAsync();
    }
}

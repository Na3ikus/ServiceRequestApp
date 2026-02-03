namespace ServiceDeskSystem.Data.Repository.Templates
{
    public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T>
        where T : class
    {
    }
}

using System.Linq.Expressions;

namespace BloggingAPI.Domain.Repository.Interface
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> condition);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);

    }
}

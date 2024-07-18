using BloggingAPI.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BloggingAPI.Persistence.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected ApplicationDbContext _applicationDbContext;
        protected BaseRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public void Create(T entity)
        {
            _applicationDbContext.Set<T>().Add(entity);
        }

        public void Delete(T entity)
        {
            _applicationDbContext.Set<T>().Remove(entity);
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> condition)
        {
            return _applicationDbContext.Set<T>().Where(condition).AsNoTracking();
        }

        public IQueryable<T> GetAll()
        {
            return _applicationDbContext.Set<T>().AsNoTracking();
        }

        public void Update(T entity)
        {
            _applicationDbContext.Set<T>().Update(entity);
        }
    }
}

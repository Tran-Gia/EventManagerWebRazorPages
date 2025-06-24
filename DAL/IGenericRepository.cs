using EventManagerWebRazorPage.Data;
using System.Linq.Expressions;

namespace EventManagerWebRazorPage.DAL
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>>? filter = null,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
    string includeProperties = "");
        TEntity? GetById(object id);
        void Insert(TEntity entity);
        void Delete(TEntity entity);
        void Delete(object id);
        void Update(TEntity entity);
    }
}

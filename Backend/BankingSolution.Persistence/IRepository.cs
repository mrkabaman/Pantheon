using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BankingSolution.Persistence
{
    public interface IRepository<T> where T:class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> FindByIdAsync(string id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
       
    }
}
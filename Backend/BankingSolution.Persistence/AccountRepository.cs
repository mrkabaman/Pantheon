using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BankingSolution.Logic.Poco;
using Microsoft.EntityFrameworkCore;

namespace BankingSolution.Persistence
{
    public class AccountRepository: IRepository<Account>
    {
        private readonly DataContext _context;

        public AccountRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _context.Set<Account>().ToListAsync();
        }

        public async Task<Account> FindByIdAsync(string id)
        {
            return await _context.Set<Account>().FindAsync(id);
        }

        public async Task<IEnumerable<Account>> FindAsync(Expression<Func<Account, bool>> expression)
        {
            return await _context.Set<Account>().Where(expression).ToListAsync();
        }

        public async Task<Account> AddAsync(Account entity)
        {
            _context.Set<Account>().Add(entity);
            
            return await Task.FromResult(entity);
        }

        public async Task<Account> UpdateAsync(Account entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            
            return await Task.FromResult(entity);
        }
    }
}
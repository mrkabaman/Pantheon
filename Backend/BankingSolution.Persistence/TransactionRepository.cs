using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BankingSolution.Logic.Poco;
using Microsoft.EntityFrameworkCore;

namespace BankingSolution.Persistence
{
    public class TransactionRepository: IRepository<Transaction>
    {
        private readonly DataContext _context;

        public TransactionRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Set<Transaction>().ToListAsync();
        }

        public async Task<Transaction> FindByIdAsync(string id)
        {
            return await _context.Set<Transaction>().FindAsync(id);
        }

        public async Task<IEnumerable<Transaction>> FindAsync(Expression<Func<Transaction, bool>> expression)
        {
            return await _context.Set<Transaction>().Where(expression).ToListAsync();
        }

        public async Task<Transaction> AddAsync(Transaction entity)
        {
            _context.Set<Transaction>().Add(entity);
            
            return await Task.FromResult(entity);
        }

        public async Task<Transaction> UpdateAsync(Transaction entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            
            return await Task.FromResult(entity);
        }
    }
}
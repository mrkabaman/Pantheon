using System;
using System.Threading.Tasks;
using BankingSolution.Logic.Poco;

namespace BankingSolution.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;

        public UnitOfWork(DataContext context, IRepository<Account> accounts, IRepository<Transaction> transactions)
        {
            _context = context;
            Accounts = accounts;
            Transactions = transactions;
        }
       
        public IRepository<Account> Accounts { get; }
        public IRepository<Transaction> Transactions { get; }

        public async Task<int> Complete()
        {
            return await _context.SaveChangesAsync();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
                _context.Dispose();
        }
    }
}
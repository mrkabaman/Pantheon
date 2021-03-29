using System;
using System.Threading.Tasks;
using BankingSolution.Logic.Poco;

namespace BankingSolution.Persistence
{
    public interface IUnitOfWork: IDisposable
    {
        IRepository<Account> Accounts { get; }
        IRepository<Transaction> Transactions { get; }
        Task<int> Complete();
    }
}
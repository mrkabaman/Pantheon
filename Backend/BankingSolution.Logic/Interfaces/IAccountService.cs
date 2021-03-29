using System.Collections.Generic;
using System.Threading.Tasks;
using BankingSolution.Logic.Poco;

namespace BankingSolution.Logic.Interfaces
{
    public interface IAccountService
    {
        Task<AccountCreationSummary> CreateAccount(CreateAccount account);
        Task<BalanceResult> GetBalance(AccountInfo accountInfo);

        Task<WithdrawalResult> WithDraw(CreateWithdrawal withdrawalInfo);

        Task<DepositResult> Deposit(CreateDeposit depositInfo);
        
        Task<TransactionResult> GetTransactions(string accountId);
    }
}
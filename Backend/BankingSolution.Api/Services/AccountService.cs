using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankingSolution.Logic.Interfaces;
using BankingSolution.Logic.Poco;
using BankingSolution.Logic.ValueTypes;
using BankingSolution.Persistence;
using Microsoft.Extensions.Logging;

namespace BankingSolution.Api.Services
{
    public class AccountService: IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenerateId _idGenerator;

        public AccountService(ILogger<AccountService> logger, IUnitOfWork unitOfWork, IGenerateId idGenerator)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
        }
        public async Task<AccountCreationSummary> CreateAccount(CreateAccount account)
        {
            _logger.LogInformation($"{nameof(AccountService)}.{nameof(CreateAccount)} - {account}");
            
            string accountId = _idGenerator.New();
            Account newAccount = new Account {Id = accountId, Name = account.Name, Balance = account.Amount.Value};
            Transaction transaction = new Transaction {AccountId = accountId, Type = TransactionType.Deposit.ToString(), Amount = account.Amount.Value, Balance = account.Amount.Value, Timestamp = DateTime.Now};

            await _unitOfWork.Accounts.AddAsync(newAccount);

            await _unitOfWork.Transactions.AddAsync(transaction);

            await _unitOfWork.Complete();

            return new AccountCreationSummary(account.Name, accountId, Amount.FromInput(account.Amount.Value));
        }

        public async Task<BalanceResult> GetBalance(AccountInfo accountInfo)
        {
            _logger.LogInformation($"{nameof(AccountService)}.{nameof(GetBalance)} - {accountInfo}");
            
            Account account = await _unitOfWork.Accounts.FindByIdAsync(accountInfo.AccountId);
            
            if(account == null)
                return new BalanceResult(accountInfo.AccountId,string.Empty,AccountStatus.Error,"Account id not be found", Currency.None, Amount.None);

            return new BalanceResult(accountInfo.AccountId, account.Name, AccountStatus.Success, $"Current balance for account",
                Currency.FromInput(Currencies.GBP), Amount.FromInput(account.Balance));

        }

        public async Task<WithdrawalResult> WithDraw(CreateWithdrawal withdrawalInfo)
        {
            _logger.LogInformation($"{nameof(AccountService)}.{nameof(WithDraw)} - {withdrawalInfo}");
            
            Account account = await _unitOfWork.Accounts.FindByIdAsync(withdrawalInfo.AccountId);
            
            if(account == null)
                return new WithdrawalResult(withdrawalInfo.AccountId,AccountStatus.Error,"Account id not be found", Currency.None, Amount.FromInput(withdrawalInfo.Amount.Value), Amount.None, Amount.None);
            
            if (canWithDraw() == false)
                return new WithdrawalResult(withdrawalInfo.AccountId,AccountStatus.Error, "You've exceeded withdrawal limit or amount is greater than balance", Currency.None, Amount.FromInput(withdrawalInfo.Amount.Value),Amount.FromInput(account.Balance),Amount.FromInput(account.Balance));

            decimal oldBalance = account.Balance;
            
            decimal newBalance = account.Balance - withdrawalInfo.Amount.Value;
            account.Balance = newBalance;
            
            
            await _unitOfWork.Accounts.UpdateAsync(account);

            Transaction transaction = new Transaction {AccountId = withdrawalInfo.AccountId, Type = TransactionType.Withdrawal.ToString(), Amount = withdrawalInfo.Amount.Value, Balance = account.Balance, Timestamp = DateTime.Now};

            await _unitOfWork.Transactions.AddAsync(transaction);

            await _unitOfWork.Complete();
          
            return new WithdrawalResult(withdrawalInfo.AccountId,AccountStatus.Success, AccountStatus.Success.ToString(),Currency.FromInput(Currencies.GBP), Amount.FromInput(withdrawalInfo.Amount.Value),Amount.FromInput(oldBalance),Amount.FromInput(account.Balance));

            bool canWithDraw()
            {
                return (withdrawalInfo.Amount.Value <= account.Balance && account.Balance > 0);
            }
        }

        public async Task<DepositResult> Deposit(CreateDeposit depositInfo)
        {
            _logger.LogInformation($"{nameof(AccountService)}.{nameof(Deposit)} - {depositInfo}");
            
            Account account = await _unitOfWork.Accounts.FindByIdAsync(depositInfo.AccountId);
            
            if(account == null)
                return new DepositResult(depositInfo.AccountId,AccountStatus.Error,"Account id not be found", Currency.None, Amount.FromInput(depositInfo.Amount.Value), Amount.None, Amount.None);

            
            decimal oldBalance = account.Balance;
            account.Balance += depositInfo.Amount.Value;

            await _unitOfWork.Accounts.UpdateAsync(account);
            
            Transaction transaction = new Transaction {AccountId = depositInfo.AccountId, Type = TransactionType.Deposit.ToString(), Amount = depositInfo.Amount.Value, Balance = account.Balance, Timestamp = DateTime.Now};
            await _unitOfWork.Transactions.AddAsync(transaction);
            
            await _unitOfWork.Complete();
            
            return new DepositResult(account.Id, 
                AccountStatus.Success, $"Successfully deposited {depositInfo.Amount.Value} into account", 
                Currency.FromInput(depositInfo.Currency.Value), Amount.FromInput(depositInfo.Amount.Value),Amount.FromInput(oldBalance),Amount.FromInput(account.Balance));
            
        }

        public async Task<TransactionResult> GetTransactions(string accountId)
        {
            _logger.LogInformation($"{nameof(AccountService)}.{nameof(GetTransactions)} - {accountId}");
            
            Account account = await _unitOfWork.Accounts.FindByIdAsync(accountId);
            
            if(account == null)
                return new TransactionResult(accountId,Enumerable.Empty<Transaction>(), AccountStatus.Error,"Account id not be found", Currency.None, Amount.None);

            IEnumerable<Transaction> transactions = await _unitOfWork.Transactions.FindAsync(a => a.AccountId.ToUpperInvariant() == account.Id.ToUpperInvariant());
            if(transactions == null || !transactions.Any())
                return new TransactionResult(accountId,Enumerable.Empty<Transaction>(), AccountStatus.Error,$"No transactions found for account: {accountId}", Currency.None, Amount.None);
            
            return new TransactionResult(accountId,transactions.ToList(), AccountStatus.Success,"Transactions for account", Currency.None, Amount.None);
            
        }
    }
}
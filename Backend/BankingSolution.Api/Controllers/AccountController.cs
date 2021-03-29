using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingSolution.Api.Contracts.Request;
using BankingSolution.Api.Contracts.Response;
using BankingSolution.Logic.Interfaces;
using BankingSolution.Logic.Poco;
using BankingSolution.Logic.ValueTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BankingSolution.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;
        private readonly IRatesService _ratesService;
        private readonly RatesApiOption _options;

        public AccountController(ILogger<AccountController> logger, IAccountService accountService,
            IRatesService ratesService, RatesApiOption options)
        {
            _logger = logger;
            _accountService = accountService;
            _ratesService = ratesService;
            _options = options;
        }

        [HttpPost]
        [Route("/account/create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountCreateResponse))]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        {
            _logger.LogInformation($"{nameof(AccountController)}.{nameof(CreateAccount)} - {request}");
            
            if (!ModelState.IsValid)
            {
                return BadRequest($"Problem with CreateAccountRequest");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest($"Cannot created with an empty name");
            }
            
            CreateAccount createAccount =
                new CreateAccount(request.Name, Amount.FromInput(request.InitialDeposit));

            AccountCreationSummary accountCreationSummary = await _accountService.CreateAccount(createAccount);

            AccountCreateResponse response = new AccountCreateResponse(accountCreationSummary.AccountId,
                accountCreationSummary.Amount.Value, accountCreationSummary.Name);
                
            return Ok(response);
        }

        [HttpPost]
        [Route("/account/deposit")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDepositResponse))]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Deposit(CreateDepositRequest request, CancellationToken token)
        {
            _logger.LogInformation($"{nameof(AccountController)}.{nameof(Deposit)} - {request}");
            
            if (!ModelState.IsValid)
            {
                return BadRequest($"Problem with CreateDepositRequest");
            }

            if(!Currencies.IsValid(request.Currency.ToUpperInvariant()))
            {
                return BadRequest($"Currency symbol entered is invalid!");
            }

            CreateDeposit deposit = await CreateDepositFromLocalCurrency(request, token);
            
            if (!_options.BaseCurrency.Equals(request.Currency, StringComparison.InvariantCultureIgnoreCase))
            {
                deposit = await CreateDepositFromForeignCurrency(request, token);
            }
            
            DepositResult depositResult = await _accountService.Deposit(deposit);

            if (depositResult.Status == AccountStatus.Error)
            {
                return BadRequest($"{depositResult.Message}");
            }

            AccountDepositResponse response = new AccountDepositResponse(depositResult.AccountId,
                depositResult.Amount.Value, depositResult.Currency.Value, depositResult.OldBalance.Value,
                depositResult.NewBalance.Value);
                
            return Ok(response);
        }

        [HttpPost]
        [Route("/account/withdraw")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountWithdrawalResponse))]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Withdraw(CreateWithdrawalRequest request, CancellationToken token)
        {
            _logger.LogInformation($"{nameof(AccountController)}.{nameof(Withdraw)} - {request}");
            
            if (!ModelState.IsValid)
            {
                return BadRequest($"Problem with CreateWithdrawalRequest");
            }

            CreateWithdrawal withdraw = new CreateWithdrawal(request.AccountId, Amount.FromInput(request.Amount));

            WithdrawalResult withdrawalResult = await _accountService.WithDraw(withdraw);

            if (withdrawalResult.Status == AccountStatus.Error)
            {
                return BadRequest($"{withdrawalResult.Message}");
            }

            AccountWithdrawalResponse response = new AccountWithdrawalResponse(withdrawalResult.AccountId,
                withdrawalResult.Amount.Value, withdrawalResult.OldBalance.Value, withdrawalResult.NewBalance.Value);
                
            return Ok(response);
        }

        [HttpGet]
        [Route("/account/balance/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountBalanceResponse))]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Balance(string id)
        {
            _logger.LogInformation($"{nameof(AccountController)}.{nameof(Balance)} - {id}");
            
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest($"Account id cannot be null");
            }


            BalanceResult balanceResult = await _accountService.GetBalance(new AccountInfo(id, Amount.None));

            if (balanceResult.Status == AccountStatus.Error)
            {
                return BadRequest($"{balanceResult.Message}");
            }

            AccountBalanceResponse response = new AccountBalanceResponse(balanceResult.AccountId,
                balanceResult.Amount.Value, balanceResult.Name);
                
            return Ok(response);
        }

        [HttpGet]
        [Route("/account/transactions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AccountTransactionResponse>))]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Transactions(string id)
        {
            _logger.LogInformation($"{nameof(AccountController)}.{nameof(Transactions)} - {id}");
            
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest($"Account id cannot be null");
            }

            TransactionResult transactionResult = await _accountService.GetTransactions(id);

            if (transactionResult.Status == AccountStatus.Error)
            {
                return BadRequest($"{transactionResult.Message}");
            }

          
            AccountTransactionResponse[] transactions = transactionResult.Transactions.Select(a =>
                new AccountTransactionResponse(a.AccountId,a.Amount,a.Type,a.Balance,a.Timestamp)
               ).ToArray();

            return Ok(transactions);
        }

        [HttpGet]
        [Route("/account/export")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Export(string id)
        {
            _logger.LogInformation($"{nameof(AccountController)}.{nameof(Export)} - {id}");
            
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest($"Account id cannot be null");
            }

            TransactionResult transactionResult = await _accountService.GetTransactions(id);

            if (transactionResult.Status == AccountStatus.Error)
            {
                return BadRequest($"{transactionResult.Message}");
            }
         
            AccountTransactionResponse[] transactions = transactionResult.Transactions.Select(a =>
                new AccountTransactionResponse(a.AccountId,a.Amount,a.Type,a.Balance,a.Timestamp)
            ).ToArray();
           
            string fileDownloadName = $"Transactions_{DateTime.Now:yyyyMMddHHmmss}.csv";
            
            return new TransactionCsvResult(transactions, fileDownloadName);
        }
        
        [HttpGet]
        [Route("/currency/all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllSymbols()
        {
            _logger.LogInformation($"{nameof(AccountController)}.{nameof(GetAllSymbols)}");
            
            string[] currencies = await Task.FromResult(Currencies.ValidSymbols);
            
            return Ok(currencies);
        }
        
        
        private async Task<CreateDeposit> CreateDepositFromForeignCurrency(CreateDepositRequest request, CancellationToken token)
        {
            Rate rate = await _ratesService.GetExchangeRate(Currency.FromInput(request.Currency),
                Currency.FromInput(_options.BaseCurrency),
                token);

            decimal convertedAmount = request.Amount * rate.ConversionValue;

            CreateDeposit deposit = new CreateDeposit(request.AccountId, Amount.FromInput(convertedAmount),
                Currency.FromInput(request.Currency));

            return deposit;
        }
        
        private async Task<CreateDeposit> CreateDepositFromLocalCurrency(CreateDepositRequest request, CancellationToken token)
        {
            CreateDeposit deposit = new CreateDeposit(request.AccountId, Amount.FromInput(request.Amount),
                Currency.FromInput(request.Currency));

            return await Task.FromResult(deposit);
        }
    }
}
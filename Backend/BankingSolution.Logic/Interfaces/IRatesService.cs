using System.Threading;
using System.Threading.Tasks;
using BankingSolution.Logic.ValueTypes;

namespace BankingSolution.Logic.Interfaces
{
    public interface IRatesService
    {
        Task<Rate> GetExchangeRate(Currency fromCurrency, Currency targetCurrency, CancellationToken stoppingToken);
    }
}
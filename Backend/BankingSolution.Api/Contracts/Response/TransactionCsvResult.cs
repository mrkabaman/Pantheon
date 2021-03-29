using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BankingSolution.Api.Contracts.Response
{
    public class TransactionCsvResult : FileResult
    {
        private readonly IEnumerable<AccountTransactionResponse> _transactions;
        private readonly string _fileDownloadName;

        public TransactionCsvResult(IEnumerable<AccountTransactionResponse> transactions, string fileDownloadName) :
            base("text/csv")
        {
            _transactions = transactions;
            _fileDownloadName = fileDownloadName;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            context.HttpContext.Response.Headers.Add("Content-Disposition",
                new[] {"attachment; filename=" + _fileDownloadName});
            using (var streamWriter = new StreamWriter(response.Body))
            {
                await streamWriter.WriteLineAsync(
                    $"AccountId, TransactionType, Amount, Balance, Timestamp"
                );
                foreach (var p in _transactions)
                {
                    await streamWriter.WriteLineAsync(
                        $"{p.AccountId}, {p.TransactionType}, {p.Amount}, {p.Balance}, {p.Timestamp:yyyy-MM-dd HH:mm:ss}"
                    );
                    await streamWriter.FlushAsync();
                }

                await streamWriter.FlushAsync();
            }
        }
    }
}
using System;

namespace BankingSolution.Logic.ValueTypes
{
    public record Rate
    {
        public string BaseRate { get; }
        public string TargetRate { get; }
        public decimal ConversionValue { get; }
        public DateTime Date { get;  }

        public Rate(string baseRate, string targetRate, decimal conversionValue, DateTime date)
        {
            BaseRate = baseRate;
            TargetRate = targetRate;
            ConversionValue = conversionValue;
            Date = date;
        }

        public static Rate None = new(string.Empty, string.Empty, 0m, DateTime.MinValue);

    }
}
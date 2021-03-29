using System;

namespace BankingSolution.Logic.ValueTypes
{
    public record Currency
    {
        public string Value { get; }

        private Currency(string value)
        {
            Value = value;
        }

        public static Currency FromInput(string value) => new (value);

        public static Currency None = new(string.Empty);

        public override string ToString()
        {
            return $"[{nameof(Value)}: {Value}]";
        }
    }
}
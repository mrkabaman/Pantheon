namespace BankingSolution.Logic.ValueTypes
{
    public record Amount
    { 
        public decimal Value { get; }

        private Amount(decimal value)
        {
            Value = value;
        }
        public static Amount FromInput(decimal value) => new(value);
        
        public static Amount None = new(0m);

        public override string ToString()
        {
            return $"[{nameof(Value)}: {Value}]";
        }
    }
}
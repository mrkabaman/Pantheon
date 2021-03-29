namespace BankingSolution.Logic.Poco
{
    public class RatesApiOption
    {
        public string Url { get; }
        public string BaseCurrency { get; }

        public RatesApiOption(string url, string baseCurrency)
        {
            Url = url;
            BaseCurrency = baseCurrency;
        }

        public override string ToString()
        {
            return $"[{nameof(Url)}: {Url}, {nameof(BaseCurrency)}: {BaseCurrency}]";
        }
    }
}
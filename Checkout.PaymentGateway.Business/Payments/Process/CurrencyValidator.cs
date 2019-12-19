using System.Linq;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class CurrencyValidator : ICurrencyValidator
	{
		private readonly string[] SupportedCurrencies = new[] { "GBP", "HKD" };

		public bool IsCurrencySupported(string currencyCode)
			=> SupportedCurrencies.Any(x => string.Equals(x, currencyCode, System.StringComparison.OrdinalIgnoreCase));
	}
}

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public interface ICurrencyValidator
	{
		bool IsCurrencySupported(string currencyCode);
	}
}

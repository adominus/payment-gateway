namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public interface ICreditCardNumberValidator
	{
		bool IsCreditCardNumberValid(string creditCardNumber);
	}
}

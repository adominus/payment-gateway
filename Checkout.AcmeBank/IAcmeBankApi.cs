using Checkout.AcmeBank.Models;
using Refit;
using System.Threading.Tasks;

namespace Checkout.AcmeBank
{
	public interface IAcmeBankApi
	{
		[Post("payment/process")]
		Task<AcmeProcessPaymentResult> ProcessPayment(
			string creditCardNumber,
			string cvv,
			int expiryMonth,
			int expiryYear,
			decimal amount,
			string currency,
			string customerName);
	}
}

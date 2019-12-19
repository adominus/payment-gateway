using System;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Business.Payments.Get
{
	public interface IGetPaymentRequestQuery
	{
		Task<PaymentRequestModel> ExecuteAsync(Guid paymentRequestId);
	}
}
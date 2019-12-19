using System;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public interface IDateTimeProvider
	{
		DateTime UtcNow();
	}
}

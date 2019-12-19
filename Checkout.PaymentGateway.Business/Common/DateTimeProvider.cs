using System;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow()
			=> DateTime.UtcNow;
	}
}

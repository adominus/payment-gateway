using System;

namespace Checkout.PaymentGateway.AcceptanceTests.Payments
{
	public class CreatePaymentRequestResponseModel
	{
		public Guid PaymentRequestId { get; set; }

		public int Status { get; set; }
	}
}

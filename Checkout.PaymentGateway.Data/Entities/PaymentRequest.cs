using System;

namespace Checkout.PaymentGateway.Data.Entities
{
	public class PaymentRequest
	{
		public Guid Id { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}

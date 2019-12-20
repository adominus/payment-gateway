using Checkout.PaymentGateway.Domain.Enums;
using System;

namespace Checkout.PaymentGateway.Data.Entities
{
	public class PaymentRequest
	{
		public Guid Id { get; set; }

		public PaymentRequestStatus Status { get; set; }

		public Guid? BankTransactionId { get; set; }
		public string BankErrorDescription { get; set; }

		public string CreditCardNumber { get; set; }
		public string CVV { get; set; }

		public int ExpiryMonth { get; set; }
		public int ExpiryYear { get; set; }

		public decimal Amount { get; set; }
		public string Currency { get; set; }

		public string CustomerName { get; set; }

		public string Reference { get; set; }
	}
}

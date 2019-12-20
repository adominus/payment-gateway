using System;

namespace Checkout.PaymentGateway.AcceptanceTests.Payments
{
	public class GetPaymentRequestResponseModel
	{
		public Guid PaymentRequestId { get; set; }

		public int Status { get; set; }

		public Guid? BankTransactionId { get; set; }
		public string BankErrorDescription { get; set; }

		public string MaskedCreditCardNumber { get; set; }

		public int ExpiryMonth { get; set; }
		public int ExpiryYear { get; set; }

		public decimal Amount { get; set; }
		public string Currency { get; set; }

		public string CustomerName { get; set; }

		public string Reference { get; set; }
	}
}

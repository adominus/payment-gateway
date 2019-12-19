﻿namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class ProcessPaymentCommandRequest
	{
		public string CreditCardNumber { get; set; }
		public string CVV { get; set; }

		public int ExpiryMonth { get; set; }
		public int ExpiryYear { get; set; }

		public decimal Amount { get; set; }
		public string Currency { get; set; }

		// TODO: Not tested 
		public string CustomerName { get; set; }

		public string Reference { get; set; }
	}
}

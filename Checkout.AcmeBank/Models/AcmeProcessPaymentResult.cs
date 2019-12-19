using System;

namespace Checkout.AcmeBank.Models
{
	public class AcmeProcessPaymentResult
	{
		public Guid Id { get; set; }

		public bool WasSuccessful { get; set; }

		public string Error { get; set; }
	}
}

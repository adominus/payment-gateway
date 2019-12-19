using Checkout.PaymentGateway.Business.Common;
using Checkout.PaymentGateway.Domain.Enums;
using System;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class ProcessPaymentCommandResultModel : CommonResultModel
	{
		public Guid PaymentRequestId { get; set; }

		public PaymentRequestStatus Status { get; set; }
	}
}

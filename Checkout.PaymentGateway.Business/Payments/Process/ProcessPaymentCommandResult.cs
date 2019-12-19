using Checkout.PaymentGateway.Business.Common;
using System;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class ProcessPaymentCommandResult : CommonResult
	{
		public Guid PaymentRequestId { get; set; }


		//  "response_code": "10000",
		//"response_summary": "Approved",
	}
}

using Checkout.PaymentGateway.Business.Common;
using System.Collections.Generic;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public interface IProcessPaymentCommandRequestValidator
	{
		IEnumerable<ValidationError> Validate(ProcessPaymentCommandRequestModel request);
	}
}


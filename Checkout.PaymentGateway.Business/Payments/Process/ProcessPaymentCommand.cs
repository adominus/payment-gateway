using Checkout.AcmeBank;
using Checkout.PaymentGateway.Business.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class ProcessPaymentCommand
	{
		private readonly IProcessPaymentCommandRequestValidator _processPaymentCommandRequestValidator;
		private readonly IAcmeBankApi _acmeBankApi;

		public ProcessPaymentCommand(
			IProcessPaymentCommandRequestValidator processPaymentCommandRequestValidator,
			IAcmeBankApi acmeBankApi)
		{
			_processPaymentCommandRequestValidator = processPaymentCommandRequestValidator;
			_acmeBankApi = acmeBankApi;
		}

		public async Task<ProcessPaymentCommandResult> ExecuteAsync(ProcessPaymentCommandRequest request)
		{
			var errors = _processPaymentCommandRequestValidator.Validate(request);

			if (errors.Any())
			{
				return new ProcessPaymentCommandResult
				{
					Notification = new Notification(errors)
				};
			}

			await _acmeBankApi.ProcessPayment(request.CreditCardNumber,
				request.CVV,
				request.ExpiryMonth,
				request.ExpiryYear,
				request.Amount,
				request.Currency,
				request.CustomerName);

			return new ProcessPaymentCommandResult();
		}
	}
}

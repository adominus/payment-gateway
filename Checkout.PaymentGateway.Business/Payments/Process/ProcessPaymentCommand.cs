using Checkout.AcmeBank;
using Checkout.PaymentGateway.Business.Common;
using Checkout.PaymentGateway.Data;
using Checkout.PaymentGateway.Data.Entities;
using Checkout.PaymentGateway.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class ProcessPaymentCommand : IProcessPaymentCommand
	{
		private readonly IProcessPaymentCommandRequestValidator _processPaymentCommandRequestValidator;
		private readonly PaymentGatewayDatabaseContext _dbContext;
		private readonly IAcmeBankApi _acmeBankApi;

		public ProcessPaymentCommand(
			IProcessPaymentCommandRequestValidator processPaymentCommandRequestValidator,
			PaymentGatewayDatabaseContext dbContext,
			IAcmeBankApi acmeBankApi)
		{
			_processPaymentCommandRequestValidator = processPaymentCommandRequestValidator;
			_dbContext = dbContext;
			_acmeBankApi = acmeBankApi;
		}

		public async Task<ProcessPaymentCommandResultModel> ExecuteAsync(ProcessPaymentCommandRequestModel request)
		{
			var errors = _processPaymentCommandRequestValidator.Validate(request);

			if (errors.Any())
			{
				return new ProcessPaymentCommandResultModel
				{
					Notification = new Notification(errors)
				};
			}

			var (bankTransactionId, status, bankError) = await PostRequestToBankAsync(request);

			var paymentRequest = new PaymentRequest
			{
				Amount = request.Amount,
				CreditCardNumber = request.CreditCardNumber,
				Currency = request.Currency,
				CustomerName = request.CustomerName,
				CVV = request.CVV,
				ExpiryMonth = request.ExpiryMonth,
				ExpiryYear = request.ExpiryYear,
				Reference = request.Reference,

				Status = status,

				BankErrorDescription = bankError,
				BankTransactionId = bankTransactionId
			};
			_dbContext.PaymentRequests.Add(paymentRequest);

			await _dbContext.SaveChangesAsync();

			return new ProcessPaymentCommandResultModel
			{
				PaymentRequestId = paymentRequest.Id,
				Status = status
			};
		}

		private async Task<(Guid? Id, PaymentRequestStatus Status, string Error)> PostRequestToBankAsync(ProcessPaymentCommandRequestModel request)
		{
			try
			{
				var bankResult = await _acmeBankApi.ProcessPayment(request.CreditCardNumber,
					request.CVV,
					request.ExpiryMonth,
					request.ExpiryYear,
					request.Amount,
					request.Currency,
					request.CustomerName);

				return (bankResult.Id,
					bankResult.WasSuccessful ? PaymentRequestStatus.Successful : PaymentRequestStatus.Unsuccessful,
					bankResult.Error);
			}
			catch (Exception)
			{
				return (null, PaymentRequestStatus.UnableToProcess, "Unable to process with bank");
			}
		}
	}
}

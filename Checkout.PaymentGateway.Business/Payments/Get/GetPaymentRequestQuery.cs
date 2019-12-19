using Checkout.PaymentGateway.Business.Common.Exceptions;
using Checkout.PaymentGateway.Data;
using Checkout.PaymentGateway.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Business.Payments.Get
{
	public class GetPaymentRequestQuery : IGetPaymentRequestQuery
	{
		private readonly PaymentGatewayDatabaseContext _dbContext;

		public GetPaymentRequestQuery(PaymentGatewayDatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<PaymentRequestModel> ExecuteAsync(Guid paymentRequestId)
		{
			var paymentRequest = await _dbContext.PaymentRequests.SingleOrDefaultAsync(x => x.Id == paymentRequestId);

			if (paymentRequest == null)
			{
				throw new NotFoundException();
			}

			return new PaymentRequestModel
			{
				Amount = paymentRequest.Amount,
				BankErrorDescription = paymentRequest.BankErrorDescription,
				BankTransactionId = paymentRequest.BankTransactionId,
				Currency = paymentRequest.Currency,
				CustomerName = paymentRequest.CustomerName,
				ExpiryMonth = paymentRequest.ExpiryMonth,
				ExpiryYear = paymentRequest.ExpiryYear,
				MaskedCreditCardNumber = MaskCreditCardNumber(paymentRequest.CreditCardNumber),
				PaymentRequestId = paymentRequest.Id,
				Reference = paymentRequest.Reference,
				Status = paymentRequest.Status
			};
		}

		private string MaskCreditCardNumber(string creditCardNumber) =>
			new string('*', creditCardNumber.Length - 4) +
			creditCardNumber.Substring(creditCardNumber.Length - 4);
	}

	public class PaymentRequestModel
	{
		public Guid PaymentRequestId { get; set; }

		public PaymentRequestStatus Status { get; set; }

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

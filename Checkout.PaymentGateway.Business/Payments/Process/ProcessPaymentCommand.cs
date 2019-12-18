using Checkout.PaymentGateway.Business.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class ProcessPaymentCommand
	{
		private readonly ICreditCardNumberValidator _creditCardNumberValidator;
		private readonly ICurrencyValidator _currencyValidator;
		private readonly IDateTimeProvider _dateTimeProvider;

		public ProcessPaymentCommand(
			ICreditCardNumberValidator creditCardNumberValidator,
			ICurrencyValidator currencyValidator,
			IDateTimeProvider dateTimeProvider)
		{
			_creditCardNumberValidator = creditCardNumberValidator;
			_currencyValidator = currencyValidator;
			_dateTimeProvider = dateTimeProvider;
		}

		public async Task<ProcessPaymentCommandResult> ExecuteAsync(ProcessPaymentCommandRequest request)
		{
			var errors = Validate(request);

			if (errors.Any())
			{
				return new ProcessPaymentCommandResult
				{
					Notification = new Notification(errors)
				};
			}

			return new ProcessPaymentCommandResult();
		}

		private IEnumerable<ValidationError> Validate(ProcessPaymentCommandRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException();
			}

			if (!_creditCardNumberValidator.IsCreditCardNumberValid(request.CreditCardNumber))
			{
				yield return new ValidationError(nameof(request.CreditCardNumber), "Credit card number is invalid");
			}

			if (!_currencyValidator.IsCurrencySupported(request.Currency))
			{
				yield return new ValidationError(nameof(request.Currency), "Currency not supported");
			}

			if (!Enumerable.Range(1, 12).Contains(request.ExpiryMonth))
			{
				yield return new ValidationError(nameof(request.ExpiryMonth), "Invalid expiry month");
			}

			if (request.ExpiryYear < 1 || request.ExpiryYear > 9999)
			{
				yield return new ValidationError(nameof(request.ExpiryYear), "Invalid expiry year");
			}

			if (DateTime.TryParseExact($"{request.ExpiryYear:0000}-{request.ExpiryMonth:00}-01T00:00:00.0000000Z",
									   "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
									   CultureInfo.InvariantCulture,
									   DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
				out DateTime expiryDate))
			{
				var now = _dateTimeProvider.UtcNow();
				var firstOfCurrentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

				if (expiryDate.Date <= firstOfCurrentMonth.Date)
				{
					yield return new ValidationError(null, "Card has expired");
				}
			}
		}

	}

	public interface IDateTimeProvider
	{
		DateTime UtcNow();
	}

	public interface ICreditCardNumberValidator
	{
		bool IsCreditCardNumberValid(string creditCardNumber);
	}

	public interface ICurrencyValidator
	{
		bool IsCurrencySupported(string currencyCode);
	}

	public class ProcessPaymentCommandResult : CommonResult
	{
		public Guid PaymentRequestId { get; set; }


		//  "response_code": "10000",
		//"response_summary": "Approved",
	}

	// TODO: Move this into domain?
	// It needs to be referenced by the data layer 
	public enum PaymentRequestStatus
	{
		Successful = 1,
		Unsuccessful = 2
	}

	public class ProcessPaymentCommandRequest
	{
		public string CreditCardNumber { get; set; }

		// Not required
		// 3 to 4 digits
		public string CVV { get; set; }

		// Required
		public int ExpiryMonth { get; set; }

		// Required
		public int ExpiryYear { get; set; }

		// required
		public decimal Amount { get; set; }

		// ISO code 
		public string Currency { get; set; }

		public string Reference { get; set; }
	}
}

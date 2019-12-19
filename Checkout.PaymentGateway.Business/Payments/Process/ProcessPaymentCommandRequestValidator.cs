using Checkout.PaymentGateway.Business.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public class ProcessPaymentCommandRequestValidator : IProcessPaymentCommandRequestValidator
	{
		private readonly ICreditCardNumberValidator _creditCardNumberValidator;
		private readonly ICurrencyValidator _currencyValidator;
		private readonly IDateTimeProvider _dateTimeProvider;

		public ProcessPaymentCommandRequestValidator(
			ICreditCardNumberValidator creditCardNumberValidator,
			ICurrencyValidator currencyValidator,
			IDateTimeProvider dateTimeProvider)
		{
			_creditCardNumberValidator = creditCardNumberValidator;
			_currencyValidator = currencyValidator;
			_dateTimeProvider = dateTimeProvider;
		}

		public IEnumerable<ValidationError> Validate(ProcessPaymentCommandRequestModel request)
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

			var expiryDateValidationError = ValidateExpiryDate(request);
			if (expiryDateValidationError != null)
			{
				yield return expiryDateValidationError;
			}

			var cvvValidationError = ValidateCvv(request);
			if (cvvValidationError != null)
			{
				yield return cvvValidationError;
			}

			if (request.Amount <= 0)
			{
				yield return new ValidationError(nameof(request.Amount), "Amount must be greater than zero");
			}

			if (string.IsNullOrWhiteSpace(request.CustomerName))
			{
				yield return new ValidationError(nameof(request.CustomerName), "Customer name must be provided");
			}
		}

		private ValidationError ValidateExpiryDate(ProcessPaymentCommandRequestModel request)
		{
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
					return new ValidationError(null, "Card has expired");
				}
			}
			else
			{
				return new ValidationError(null, "Invalid expiry date");
			}

			return null;
		}

		private ValidationError ValidateCvv(ProcessPaymentCommandRequestModel request)
		{
			if (!string.IsNullOrWhiteSpace(request.CVV))
			{
				if (!request.CVV.All(Char.IsDigit))
				{
					return new ValidationError(nameof(request.CVV), "Invalid CVV");
				}
				else if (request.CVV.Length != 3 && request.CVV.Length != 4)
				{
					return new ValidationError(nameof(request.CVV), "Invalid CVV");
				}
			}

			return null;
		}
	}
}


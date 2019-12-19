using AutoFixture;
using AutoFixture.AutoMoq;
using Checkout.PaymentGateway.Business.Common;
using Checkout.PaymentGateway.Business.Payments.Process;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkout.PaymentGateway.Business.Tests.Payments.Process
{
	public class ProcessPaymentCommandRequestValidatorTests
	{
		private IFixture _fixture;

		private ProcessPaymentCommandRequestValidator _subject;
		private ProcessPaymentCommandRequestModel _request;

		private Mock<ICreditCardNumberValidator> _creditCardNumberValidatorMock;
		private Mock<ICurrencyValidator> _currencyValidatorMock;
		private Mock<IDateTimeProvider> _dateTimeProviderMock;
		private DateTime _utcNow;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture()
				.Customize(new AutoMoqCustomization());

			_creditCardNumberValidatorMock = _fixture.Freeze<Mock<ICreditCardNumberValidator>>();
			_currencyValidatorMock = _fixture.Freeze<Mock<ICurrencyValidator>>();
			_dateTimeProviderMock = _fixture.Freeze<Mock<IDateTimeProvider>>();
		}

		[SetUp]
		public void SetUp()
		{
			_subject = _fixture.Create<ProcessPaymentCommandRequestValidator>();

			_utcNow = _fixture.Create<DateTime>();

			_request = new ProcessPaymentCommandRequestModel
			{
				Amount = 12,
				CreditCardNumber = "1234123412341234",
				Currency = "GBP",
				CVV = "123",
				ExpiryMonth = _utcNow.Month,
				ExpiryYear = _utcNow.Year + 1,
				Reference = "Foo",
				CustomerName = "Bar",
			};

			_creditCardNumberValidatorMock.Setup(x => x.IsCreditCardNumberValid(_request.CreditCardNumber))
				.Returns(true);
			_currencyValidatorMock.Setup(x => x.IsCurrencySupported(_request.Currency))
				.Returns(true);
			_dateTimeProviderMock.Setup(x => x.UtcNow())
				.Returns(() => _utcNow);
		}

		[TearDown]
		public void TearDown()
		{
			_creditCardNumberValidatorMock.Reset();
			_currencyValidatorMock.Reset();
		}

		[Test]
		public void WhenRequestIsNull_ShouldThrowArgumentException()
		{
			// Arrange, Act
			TestDelegate act = () => _subject.Validate(null).ToList();

			// Assert
			Assert.That(act, Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void ShouldValidateCreditCardNumber()
		{
			// Arrange, Act
			_subject.Validate(_request).ToList();

			// Assert
			_creditCardNumberValidatorMock.Verify(x => x.IsCreditCardNumberValid(_request.CreditCardNumber));
		}

		[Test]
		public void WhenCardNumberIsNotValid_ShouldReturnValidationError()
		{
			// Arrange 
			_creditCardNumberValidatorMock.Setup(x => x.IsCreditCardNumberValid(_request.CreditCardNumber))
				.Returns(false);

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "CreditCardNumber", "Credit card number is invalid");
		}

		[Test]
		public void WhenCurrencyCodeIsNotSupported_ShouldReturnValidationError()
		{
			// Arrange 
			_currencyValidatorMock.Setup(x => x.IsCurrencySupported(_request.Currency))
				.Returns(false);

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "Currency", "Currency not supported");
		}

		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(-5)]
		[TestCase(13)]
		[TestCase(17)]
		[TestCase(21)]
		public void WhenExpiryMonthIsNotValid_ShouldReturnValidationError(int month)
		{
			// Arrange 
			_request.ExpiryMonth = month;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "Invalid expiry date");
		}

		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(-5)]
		[TestCase(10000)]
		[TestCase(10001)]
		public void WhenExpiryYearIsNotValid_ShouldReturnValidationError(int year)
		{
			// Arrange 
			_request.ExpiryYear = year;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "Invalid expiry date");
		}

		[Test]
		[TestCase(-1)]
		[TestCase(-11)]
		public void WhenExpiryDateIsInThePast_ShouldReturnValidationError(int months)
		{
			// Arrange 
			var previousMonth = _utcNow.AddMonths(months);
			_request.ExpiryMonth = previousMonth.Month;
			_request.ExpiryYear = previousMonth.Year;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "Card has expired");
		}

		[Test]
		[TestCase(1)]
		[TestCase(11)]
		public void WhenExpiryDateIsInTheFuture_ShouldNotReturnValidationError(int months)
		{
			// Arrange 
			var previousMonth = _utcNow.AddMonths(months);
			_request.ExpiryMonth = previousMonth.Month;
			_request.ExpiryYear = previousMonth.Year;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			Assert.That(validationErrors, Is.Empty);
		}

		[Test]
		public void WhenExpiryDateIsInTheSameMonth_ShouldReturnValidationErrors()
		{
			// Arrange 
			_request.ExpiryMonth = _utcNow.Month;
			_request.ExpiryYear = _utcNow.Year;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "Card has expired");
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		public void WhenCvvIsNullOrWhitespace_ShouldNotReturnValidationErrors(string cvv)
		{
			// Arrange 
			_request.CVV = cvv;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			Assert.That(validationErrors, Is.Empty);
		}

		[Test]
		[TestCase("abc")]
		[TestCase("abcd")]
		[TestCase("123-")]
		[TestCase("a123")]
		public void WhenCvvIsNotDigits_ShouldReturnValidationErrors(string cvv)
		{
			// Arrange 
			_request.CVV = cvv;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "CVV", "Invalid CVV");
		}

		[Test]
		[TestCase("1")]
		[TestCase("12")]
		[TestCase("12345")]
		[TestCase("123456")]
		public void WhenCvvIsNotCorrectLength_ShouldReturnValidationErrors(string cvv)
		{
			// Arrange 
			_request.CVV = cvv;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "CVV", "Invalid CVV");
		}

		[Test]
		[TestCase(-1)]
		[TestCase(-100123)]
		[TestCase(0)]
		[TestCase(-0.1)]
		public void WhenAmountIsNotGreaterThanZero_ShouldReturnValidationErrors(decimal amount)
		{
			// Arrange 
			_request.Amount = amount;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "Amount", "Amount must be greater than zero");
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		public void WhenCustomerNameIsNullOrWhitespace_ShouldReturnValidationErrors(string customerName)
		{
			// Arrange 
			_request.CustomerName = customerName;

			// Act
			var validationErrors = _subject.Validate(_request);

			// Assert
			AssertSingleErrorMatches(validationErrors, "CustomerName", "Customer name must be provided");
		}

		private void AssertSingleErrorMatches(IEnumerable<ValidationError> validationErrors, string error)
		{
			AssertSingleErrorMatches(validationErrors, null, error);
		}

		private void AssertSingleErrorMatches(IEnumerable<ValidationError> validationErrors, string attribute, string error)
		{
			Assert.That(validationErrors.Count(), Is.EqualTo(1));

			var validationError = validationErrors.Single();

			Assert.That(validationError.Attribute, Is.EqualTo(attribute));
			Assert.That(validationError.Error, Is.EqualTo(error));
		}
	}
}

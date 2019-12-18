using AutoFixture;
using AutoFixture.AutoMoq;
using Checkout.PaymentGateway.Business.Payments.Process;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Business.Tests.Payments
{
	public class ProcessPaymentCommandTests
	{
		private IFixture _fixture;

		private ProcessPaymentCommand _subject;
		private ProcessPaymentCommandRequest _request;

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
			_subject = _fixture.Create<ProcessPaymentCommand>();

			_utcNow = _fixture.Create<DateTime>();

			_request = new ProcessPaymentCommandRequest
			{
				Amount = 12,
				CreditCardNumber = "1234123412341234",
				Currency = "GBP",
				CVV = "123",
				ExpiryMonth = _utcNow.Month,
				ExpiryYear = _utcNow.Year + 1,
				Reference = "Foo"
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
			AsyncTestDelegate act = () => _subject.ExecuteAsync(null);

			// Assert
			Assert.That(act, Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public async Task ShouldValidateCreditCardNumber()
		{
			// Arrange, Act
			await _subject.ExecuteAsync(_request);

			// Assert
			_creditCardNumberValidatorMock.Verify(x => x.IsCreditCardNumberValid(_request.CreditCardNumber));
		}

		[Test]
		public async Task WhenCardNumberIsNotValid_ShouldReturnValidationError()
		{
			// Arrange 
			_creditCardNumberValidatorMock.Setup(x => x.IsCreditCardNumberValid(_request.CreditCardNumber))
				.Returns(false);

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Notification.HasErrors, Is.True);

			var error = result.Notification.ValidationErrors.Single();

			Assert.That(error.Attribute, Is.EqualTo("CreditCardNumber"));
			Assert.That(error.Error, Is.EqualTo("Credit card number is invalid"));
		}

		[Test]
		public async Task WhenCurrencyCodeIsNotSupported_ShouldReturnValidationError()
		{
			// Arrange 
			_currencyValidatorMock.Setup(x => x.IsCurrencySupported(_request.Currency))
				.Returns(false);

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Notification.HasErrors, Is.True);

			var error = result.Notification.ValidationErrors.Single();

			Assert.That(error.Attribute, Is.EqualTo("Currency"));
			Assert.That(error.Error, Is.EqualTo("Currency not supported"));
		}

		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(-5)]
		[TestCase(13)]
		[TestCase(17)]
		[TestCase(21)]
		public async Task WhenExpiryMonthIsNotValid_ShouldReturnValidationError(int month)
		{
			// Arrange 
			_request.ExpiryMonth = month;

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Notification.HasErrors, Is.True);

			var error = result.Notification.ValidationErrors.Single();

			Assert.That(error.Attribute, Is.EqualTo("ExpiryMonth"));
			Assert.That(error.Error, Is.EqualTo("Invalid expiry month"));
		}

		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(-5)]
		[TestCase(10000)]
		[TestCase(10001)]
		public async Task WhenExpiryYearIsNotValid_ShouldReturnValidationError(int year)
		{
			// Arrange 
			_request.ExpiryYear = year;

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Notification.HasErrors, Is.True);

			var error = result.Notification.ValidationErrors.Single();

			Assert.That(error.Attribute, Is.EqualTo("ExpiryYear"));
			Assert.That(error.Error, Is.EqualTo("Invalid expiry year"));
		}

		[Test]
		[TestCase(-1)]
		[TestCase(-11)]
		public async Task WhenExpiryDateIsInThePast_ShouldReturnValidationError(int months)
		{
			// Arrange 
			var previousMonth = _utcNow.AddMonths(months);
			_request.ExpiryMonth = previousMonth.Month;
			_request.ExpiryYear = previousMonth.Year;

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Notification.HasErrors, Is.True);

			var error = result.Notification.ValidationErrors.Single();

			Assert.That(error.Attribute, Is.Null);
			Assert.That(error.Error, Is.EqualTo("Card has expired"));
		}

		[Test]
		[TestCase(1)]
		[TestCase(11)]
		public async Task WhenExpiryDateIsInTheFuture_ShouldNotReturnValidationError(int months)
		{
			// Arrange 
			var previousMonth = _utcNow.AddMonths(months);
			_request.ExpiryMonth = previousMonth.Month;
			_request.ExpiryYear = previousMonth.Year;

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Notification.HasErrors, Is.False);
		}

		[Test]
		public async Task WhenExpiryDateIsInTheSameMonth_ShouldReturnValidationErrors()
		{
			// Arrange 
			_request.ExpiryMonth = _utcNow.Month;
			_request.ExpiryYear = _utcNow.Year;

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Notification.HasErrors, Is.True);

			var error = result.Notification.ValidationErrors.Single();

			Assert.That(error.Attribute, Is.Null);
			Assert.That(error.Error, Is.EqualTo("Card has expired"));
		}
	}
}

using AutoFixture;
using AutoFixture.AutoMoq;
using Checkout.AcmeBank;
using Checkout.AcmeBank.Models;
using Checkout.PaymentGateway.Business.Common;
using Checkout.PaymentGateway.Business.Payments.Process;
using Checkout.PaymentGateway.Data;
using Checkout.PaymentGateway.Domain.Enums;
using Microsoft.EntityFrameworkCore;
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

		private AcmeProcessPaymentResult _acmeProcessPaymentResult;

		private Mock<IProcessPaymentCommandRequestValidator> _processPaymentCommandRequestValidatorMock;
		private Mock<IAcmeBankApi> _acmeBankApiMock;

		private PaymentGatewayDatabaseContext _paymentGatewayDatabaseContext;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture()
				.Customize(new AutoMoqCustomization());

			_processPaymentCommandRequestValidatorMock = _fixture.Freeze<Mock<IProcessPaymentCommandRequestValidator>>();
			_acmeBankApiMock = _fixture.Freeze<Mock<IAcmeBankApi>>();
		}

		[SetUp]
		public void SetUp()
		{
			_subject = _fixture.Create<ProcessPaymentCommand>();

			_request = _fixture.Create<ProcessPaymentCommandRequest>();

			_processPaymentCommandRequestValidatorMock.Setup(x => x.Validate(_request))
				.Returns(Enumerable.Empty<ValidationError>());

			_acmeProcessPaymentResult = new AcmeProcessPaymentResult
			{
				Id = Guid.NewGuid(),
				WasSuccessful = true,
				Error = null
			};
			_acmeBankApiMock.Setup(x => x.ProcessPayment(_request.CreditCardNumber, _request.CVV, _request.ExpiryMonth, _request.ExpiryYear, _request.Amount, _request.Currency, _request.CustomerName))
				.ReturnsAsync(() => _acmeProcessPaymentResult);

			var options = new DbContextOptionsBuilder<PaymentGatewayDatabaseContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			_paymentGatewayDatabaseContext = new PaymentGatewayDatabaseContext(options);
			_fixture.Inject(_paymentGatewayDatabaseContext);
		}

		[TearDown]
		public void TearDown()
		{
			_processPaymentCommandRequestValidatorMock.Reset();
		}

		[Test]
		public async Task ShouldValidateRequest()
		{
			// Arrange, Act
			await _subject.ExecuteAsync(_request);

			// Assert
			_processPaymentCommandRequestValidatorMock.Verify(x => x.Validate(_request));
		}

		[Test]
		public async Task WhenRequestIsInvalid_ShouldReturnErrors()
		{
			// Arrange
			var expectedError = new ValidationError("foo", "bar");
			_processPaymentCommandRequestValidatorMock.Setup(x => x.Validate(_request))
				.Returns(new[] { expectedError });

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Notification.HasErrors, Is.True);
			Assert.That(result.Notification.ValidationErrors.Single().Attribute, Is.EqualTo("foo"));
			Assert.That(result.Notification.ValidationErrors.Single().Error, Is.EqualTo("bar"));
		}

		[Test]
		public async Task ShouldMakePaymentRequestToBank()
		{
			// Arrange, Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			_acmeBankApiMock.Verify(x => x.ProcessPayment(
				_request.CreditCardNumber,
				_request.CVV,
				_request.ExpiryMonth,
				_request.ExpiryYear,
				_request.Amount,
				_request.Currency,
				_request.CustomerName));
		}

		[Test]
		public async Task ShouldPersistPaymentRequestDetails()
		{
			// Arrange, Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.SingleOrDefault();

			Assert.That(paymentRequest, Is.Not.Null);
			Assert.That(paymentRequest.Status, Is.EqualTo(PaymentRequestStatus.Successful));
		}
	}
}

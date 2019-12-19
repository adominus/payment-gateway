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

namespace Checkout.PaymentGateway.Business.Tests.Payments.Process
{
	public class ProcessPaymentCommandTests
	{
		private IFixture _fixture;

		private ProcessPaymentCommand _subject;
		private ProcessPaymentCommandRequestModel _request;

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
			_request = _fixture.Create<ProcessPaymentCommandRequestModel>();

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

			_subject = _fixture.Create<ProcessPaymentCommand>();
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
		public async Task ShouldPersistSuccessfulPaymentRequest()
		{
			// Arrange, Act
			await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.SingleOrDefault();

			Assert.That(paymentRequest, Is.Not.Null);
			Assert.That(paymentRequest.Status, Is.EqualTo(PaymentRequestStatus.Successful));
		}

		[Test]
		public async Task ShouldPersistPaymentBankApiDetails()
		{
			// Arrange, Act
			await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.SingleOrDefault();

			Assert.That(paymentRequest, Is.Not.Null);

			Assert.That(paymentRequest.BankErrorDescription, Is.Null);
			Assert.That(paymentRequest.BankTransactionId, Is.EqualTo(_acmeProcessPaymentResult.Id));
		}

		[Test]
		public async Task ShouldPersistPaymentRequestDetails()
		{
			// Arrange, Act
			await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.SingleOrDefault();

			Assert.That(paymentRequest, Is.Not.Null);

			AssertPaymentRequestDetailsMatchEntity(paymentRequest);
		}

		[Test]
		public async Task ShouldReturnCheckoutPaymentRequestId()
		{
			// Arrange, Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.Single();

			Assert.That(result.PaymentRequestId, Is.EqualTo(paymentRequest.Id));
		}

		[Test]
		public async Task ShouldReturnStatus()
		{
			// Arrange, Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Status, Is.EqualTo(PaymentRequestStatus.Successful));
		}

		[Test]
		public async Task WhenBankFailsToProcess_ShouldReturnFailedStatus()
		{
			// Arrange
			_acmeProcessPaymentResult.WasSuccessful = false;

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Status, Is.EqualTo(PaymentRequestStatus.Unsuccessful));
		}

		[Test]
		public async Task WhenBankFailsToProcess_ShouldPersistUnsuccessfulPaymentRequest()
		{
			// Arrange
			_acmeProcessPaymentResult.WasSuccessful = false;
			_acmeProcessPaymentResult.Error = "foo";

			// Act
			await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.Single();

			Assert.That(paymentRequest.Status, Is.EqualTo(PaymentRequestStatus.Unsuccessful));

			Assert.That(paymentRequest.BankTransactionId, Is.EqualTo(_acmeProcessPaymentResult.Id));
			Assert.That(paymentRequest.BankErrorDescription, Is.EqualTo("foo"));
		}

		[Test]
		public async Task WhenBankFailsToProcess_ShouldPersistRequestDetails()
		{
			// Arrange
			_acmeProcessPaymentResult.WasSuccessful = false;

			// Act
			await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.Single();

			AssertPaymentRequestDetailsMatchEntity(paymentRequest);
		}

		[Test]
		public async Task WhenBankThrowsError_ShouldPersistRequestAsUnableToProcess()
		{
			// Arrange
			SetupBankThrowsError();

			// Act
			await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.Single();

			Assert.That(paymentRequest.Status, Is.EqualTo(PaymentRequestStatus.UnableToProcess));

			Assert.That(paymentRequest.BankTransactionId, Is.Null);
			Assert.That(paymentRequest.BankErrorDescription, Is.EqualTo("Unable to process with bank"));
		}

		[Test]
		public async Task WhenBankThrowsError_ShouldPersistRequestDetails()
		{
			// Arrange
			SetupBankThrowsError();

			// Act
			await _subject.ExecuteAsync(_request);

			// Assert
			var paymentRequest = _paymentGatewayDatabaseContext.PaymentRequests.Single();

			AssertPaymentRequestDetailsMatchEntity(paymentRequest);
		}

		[Test]
		public async Task WhenBankThrowsError_ShouldReturnUnableToProcessStatus()
		{
			// Arrange
			SetupBankThrowsError();

			// Act
			var result = await _subject.ExecuteAsync(_request);

			// Assert
			Assert.That(result.Status, Is.EqualTo(PaymentRequestStatus.UnableToProcess));
		}

		private void SetupBankThrowsError()
		{
			_acmeBankApiMock.Setup(x => x.ProcessPayment(_request.CreditCardNumber, _request.CVV, _request.ExpiryMonth, _request.ExpiryYear, _request.Amount, _request.Currency, _request.CustomerName))
				.ThrowsAsync(Refit.ApiException.Create(null,
					System.Net.Http.HttpMethod.Post,
					new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)).Result);
		}

		private void AssertPaymentRequestDetailsMatchEntity(Data.Entities.PaymentRequest paymentRequest)
		{
			Assert.That(paymentRequest.CreditCardNumber, Is.EqualTo(_request.CreditCardNumber));
			Assert.That(paymentRequest.Amount, Is.EqualTo(_request.Amount));
			Assert.That(paymentRequest.Currency, Is.EqualTo(_request.Currency));
			Assert.That(paymentRequest.CustomerName, Is.EqualTo(_request.CustomerName));
			Assert.That(paymentRequest.CVV, Is.EqualTo(_request.CVV));
			Assert.That(paymentRequest.ExpiryMonth, Is.EqualTo(_request.ExpiryMonth));
			Assert.That(paymentRequest.ExpiryYear, Is.EqualTo(_request.ExpiryYear));
			Assert.That(paymentRequest.Reference, Is.EqualTo(_request.Reference));
		}
	}
}

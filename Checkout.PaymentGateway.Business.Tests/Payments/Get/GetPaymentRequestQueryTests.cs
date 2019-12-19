using AutoFixture;
using Checkout.PaymentGateway.Business.Common.Exceptions;
using Checkout.PaymentGateway.Business.Payments.Get;
using Checkout.PaymentGateway.Data;
using Checkout.PaymentGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Business.Tests.Payments.Get
{
	public class GetPaymentRequestQueryTests
	{
		private IFixture _fixture;

		private GetPaymentRequestQuery _subject;

		private PaymentGatewayDatabaseContext _paymentGatewayDatabaseContext;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture();
		}

		[SetUp]
		public void SetUp()
		{
			var options = new DbContextOptionsBuilder<PaymentGatewayDatabaseContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			_paymentGatewayDatabaseContext = new PaymentGatewayDatabaseContext(options);
			_fixture.Inject(_paymentGatewayDatabaseContext);

			_subject = _fixture.Create<GetPaymentRequestQuery>();
		}

		[Test]
		public async Task ShouldMapPaymentRequestProperties()
		{
			// Arrange
			var entity = _fixture.Create<PaymentRequest>();
			_paymentGatewayDatabaseContext.Add(entity);
			_paymentGatewayDatabaseContext.SaveChanges();

			// Act
			var result = await _subject.ExecuteAsync(entity.Id);

			// Assert
			Assert.That(result, Is.Not.Null);

			Assert.That(result.Amount, Is.EqualTo(entity.Amount));
			Assert.That(result.BankErrorDescription, Is.EqualTo(entity.BankErrorDescription));
			Assert.That(result.BankTransactionId, Is.EqualTo(entity.BankTransactionId));
			Assert.That(result.Currency, Is.EqualTo(entity.Currency));
			Assert.That(result.CustomerName, Is.EqualTo(entity.CustomerName));
			Assert.That(result.ExpiryMonth, Is.EqualTo(entity.ExpiryMonth));
			Assert.That(result.ExpiryYear, Is.EqualTo(entity.ExpiryYear));
			Assert.That(result.PaymentRequestId, Is.EqualTo(entity.Id));
			Assert.That(result.Reference, Is.EqualTo(entity.Reference));
			Assert.That(result.Status, Is.EqualTo(entity.Status));
		}

		[Test]
		[TestCase("12345678901234", "**********1234")]
		[TestCase("11111111111111", "**********1111")]
		[TestCase("1234567890123456789", "***************6789")]
		public async Task ShouldMaskCreditCartNumber(string creditCardNumber, string expectedMaskedCardNumber)
		{
			// Arrange
			var entity = _fixture.Create<PaymentRequest>();
			entity.CreditCardNumber = creditCardNumber;
			_paymentGatewayDatabaseContext.Add(entity);
			_paymentGatewayDatabaseContext.SaveChanges();

			// Act
			var result = await _subject.ExecuteAsync(entity.Id);

			// Assert
			Assert.That(result, Is.Not.Null);

			Assert.That(result.MaskedCreditCardNumber, Is.EqualTo(expectedMaskedCardNumber));
		}

		[Test]
		public void WhenPaymentRequestIsNotFound_ShouldThrowException()
		{
			// Arrange
			var entity = _fixture.Create<PaymentRequest>();
			_paymentGatewayDatabaseContext.Add(entity);
			_paymentGatewayDatabaseContext.SaveChanges();

			// Act
			AsyncTestDelegate act = () => _subject.ExecuteAsync(Guid.NewGuid());

			// Assert
			Assert.That(act, Throws.TypeOf<NotFoundException>());
		}
	}
}

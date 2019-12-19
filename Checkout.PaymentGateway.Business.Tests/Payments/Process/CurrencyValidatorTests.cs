using Checkout.PaymentGateway.Business.Payments.Process;
using NUnit.Framework;

namespace Checkout.PaymentGateway.Business.Tests.Payments.Process
{
	public class CurrencyValidatorTests
	{
		[Test]
		[TestCase("GBP")]
		[TestCase("HKD")]
		[TestCase("gbp")]
		[TestCase("hkd")]
		public void WhenCurrencySupported_ShouldReturnTrue(string currencyCode)
		{
			// Arrange 
			var subject = new CurrencyValidator();

			// Act 
			var result = subject.IsCurrencySupported(currencyCode);

			// Assert
			Assert.That(result, Is.True);
		}

		[Test]
		[TestCase("EUR")]
		[TestCase("USD")]
		[TestCase("foo")]
		[TestCase("bar")]
		public void WhenCurrencyNotSupported_ShouldReturnFalse(string currencyCode)
		{
			// Arrange 
			var subject = new CurrencyValidator();

			// Act 
			var result = subject.IsCurrencySupported(currencyCode);

			// Assert
			Assert.That(result, Is.False);
		}
	}
}

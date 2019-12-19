using Checkout.PaymentGateway.Business.Payments.Process;
using NUnit.Framework;

namespace Checkout.PaymentGateway.Business.Tests.Payments.Process
{
	public class CreditCardNumberValidatorTests
	{
		private CreditCardNumberValidator _subject;

		[SetUp]
		public void SetUp()
		{
			_subject = new CreditCardNumberValidator();
		}

		[Test]
		[TestCase("12345")]
		[TestCase("1234567889")]
		[TestCase("12345678903")]
		public void WhenNumberIsBelowMinimumLength_ShouldReturnFalse(string cardNumber)
		{
			// Arrange, Act 
			var result = _subject.IsCreditCardNumberValid(cardNumber);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase("12345678901234567894")]
		[TestCase("123456789012345678906")]
		public void WhenNumberIsAboveMinimumLength_ShouldReturnFalse(string cardNumber)
		{
			// Arrange, Act 
			var result = _subject.IsCreditCardNumberValid(cardNumber);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase("12345678901x")]
		[TestCase("xxxxxxxxxxxx")]
		public void WhenNumberIsNotOnlyDigits_ShouldReturnFalse(string cardNumber)
		{
			// Arrange, Act 
			var result = _subject.IsCreditCardNumberValid(cardNumber);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase("179927398711")]
		[TestCase("111111111113")]
		public void WhenNumberPassesLuhnAlgorithm_ShouldReturnTrue(string cardNumber)
		{
			// Arrange, Act 
			var result = _subject.IsCreditCardNumberValid(cardNumber);

			// Assert
			Assert.That(result, Is.True);
		}

		[Test]
		[TestCase("179927398710")]
		[TestCase("179927398712")]
		[TestCase("179927398713")]
		[TestCase("179927398714")]
		[TestCase("179927398715")]
		[TestCase("179927398716")]
		[TestCase("179927398717")]
		[TestCase("179927398718")]
		[TestCase("179927398719")]
		[TestCase("111111111110")]
		[TestCase("111111111111")]
		[TestCase("111111111112")]
		[TestCase("111111111114")]
		[TestCase("111111111115")]
		[TestCase("111111111116")]
		[TestCase("111111111117")]
		[TestCase("111111111118")]
		[TestCase("111111111119")]
		public void WhenNumberFailsLuhnAlgorithm_ShouldReturnFalse(string cardNumber)
		{
			// Arrange, Act 
			var result = _subject.IsCreditCardNumberValid(cardNumber);

			// Assert
			Assert.That(result, Is.False);
		}
	}
}

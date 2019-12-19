using System;
using System.Linq;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public interface ICreditCardNumberValidator
	{
		bool IsCreditCardNumberValid(string creditCardNumber);
	}

	public class CreditCardNumberValidator : ICreditCardNumberValidator
	{
		public bool IsCreditCardNumberValid(string creditCardNumber)
		{
			if (creditCardNumber == null || creditCardNumber.Length < 12 || creditCardNumber.Length > 19)
			{
				return false;
			}

			if (!creditCardNumber.All(Char.IsDigit))
			{
				return false;
			}

			if (creditCardNumber.Length < 2)
			{
				return false;
			}

			var checksumDigit = int.Parse(creditCardNumber[creditCardNumber.Length - 1].ToString());
			int runningTotal = 0;

			for (int i = creditCardNumber.Length - 2; i >= 0; i--)
			{
				var digit = int.Parse(creditCardNumber[i].ToString());

				if (ShouldDouble(creditCardNumber, i))
				{
					var doubledDigit = digit * 2;
					if (doubledDigit >= 10)
					{
						runningTotal +=
							int.Parse(doubledDigit.ToString()[0].ToString()) +
							int.Parse(doubledDigit.ToString()[1].ToString());
					}
					else
					{
						runningTotal += doubledDigit;
					}
				}
				else
				{
					runningTotal += digit;
				}
			}

			var expectedChecksumDigit = (10 - (runningTotal % 10)) % 10;

			return expectedChecksumDigit == checksumDigit;
		}

		private bool ShouldDouble(string s, int i)
			=> (IsEven(s) && IsEven(i)) || (!IsEven(s) && !IsEven(i));

		private bool IsEven(int i)
			=> i % 2 == 0;

		private bool IsEven(string s)
			=> s.Length % 2 == 0;
	}
}

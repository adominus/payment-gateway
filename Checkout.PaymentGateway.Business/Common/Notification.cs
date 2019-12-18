using System.Collections.Generic;
using System.Linq;

namespace Checkout.PaymentGateway.Business.Common
{
	public class Notification
	{
		private List<ValidationError> _validationErrors;

		public Notification()
		{
			_validationErrors = new List<ValidationError>();
		}

		public Notification(IEnumerable<ValidationError> validationErrors)
		{
			_validationErrors = validationErrors.ToList();
		}

		public void AddError(string attribute, string error)
			=> _validationErrors.Add(new ValidationError(attribute, error));

		public bool HasErrors
			=> _validationErrors.Any();

		public IEnumerable<ValidationError> ValidationErrors
			=> _validationErrors.AsEnumerable();
	}

	public class ValidationError
	{
		public ValidationError (string attribute, string error)
		{
			this.Attribute = attribute;
			this.Error = error;
		}

		public string Attribute { get; private set; }

		public string Error { get; private set; }
	}
}

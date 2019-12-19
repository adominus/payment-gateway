namespace Checkout.PaymentGateway.Business.Common
{
	public abstract class CommonResultModel
	{
		public Notification Notification { get; set; }

		public CommonResultModel()
		{
			Notification = new Notification();
		}
	}
}

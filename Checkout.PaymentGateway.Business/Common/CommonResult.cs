namespace Checkout.PaymentGateway.Business.Common
{
	public abstract class CommonResult
	{
		public Notification Notification { get; set; }

		public CommonResult()
		{
			Notification = new Notification();
		}
	}
}

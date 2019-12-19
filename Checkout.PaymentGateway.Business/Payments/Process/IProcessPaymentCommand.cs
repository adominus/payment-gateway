using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Business.Payments.Process
{
	public interface IProcessPaymentCommand
	{
		Task<ProcessPaymentCommandResultModel> ExecuteAsync(ProcessPaymentCommandRequestModel request);
	}
}
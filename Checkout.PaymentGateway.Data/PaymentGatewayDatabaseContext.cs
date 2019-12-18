using Checkout.PaymentGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Checkout.PaymentGateway.Data
{
	public class PaymentGatewayDatabaseContext : DbContext
	{
		public DbSet<PaymentRequest> PaymentRequests { get; set; }
	}
}

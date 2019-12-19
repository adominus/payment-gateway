using Checkout.PaymentGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Checkout.PaymentGateway.Data
{
	public class PaymentGatewayDatabaseContext : DbContext
	{
		public PaymentGatewayDatabaseContext(DbContextOptions options) : base(options)
		{ }

		public PaymentGatewayDatabaseContext()
		{ }

		public DbSet<PaymentRequest> PaymentRequests { get; set; }
	}
}

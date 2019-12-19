using Checkout.AcmeBank;
using Checkout.PaymentGateway.Business.Payments.Get;
using Checkout.PaymentGateway.Business.Payments.Process;
using Checkout.PaymentGateway.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using System;

namespace Checkout.PaymentGateway.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			services.AddDbContext<PaymentGatewayDatabaseContext>(options =>
			{
				options.UseInMemoryDatabase("CheckoutPaymentGateway");
			});

			services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

			services.AddScoped<IGetPaymentRequestQuery, GetPaymentRequestQuery>();

			services.AddScoped<ICreditCardNumberValidator, CreditCardNumberValidator>();
			services.AddScoped<ICurrencyValidator, CurrencyValidator>();
			services.AddScoped<IProcessPaymentCommand, ProcessPaymentCommand>();
			services.AddScoped<IProcessPaymentCommandRequestValidator, ProcessPaymentCommandRequestValidator>();

			services.AddRefitClient<IAcmeBankApi>()
				.ConfigureHttpClient(x => x.BaseAddress = new Uri("http://localhost:8000"));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

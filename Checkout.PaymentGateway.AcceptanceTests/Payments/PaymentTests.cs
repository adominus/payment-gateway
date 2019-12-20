using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace Checkout.PaymentGateway.AcceptanceTests.Payments
{
	public class PaymentTests
	{
		private HttpClient _paymentGatewayApiClient;
		private FluentMockServer _fakeBankServer;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var paymentGatewayApiFactory = new WebApplicationFactory<Api.Startup>();
			_paymentGatewayApiClient = paymentGatewayApiFactory.CreateClient();

			_fakeBankServer = FluentMockServer.Start(new FluentMockServerSettings
			{
				Port = 8000
			});
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_fakeBankServer.Stop();
		}

		[Test]
		public async Task ShouldBeAbleToMakePaymentRequest()
		{
			// Arrange
			SetupBankResponse(Guid.NewGuid(), true, null);

			var request = new
			{
				Amount = 100,
				CreditCardNumber = "111111111113",
				Currency = "GBP",
				CustomerName = "John Smith",
				CVV = "123",
				ExpiryMonth = 12,
				ExpiryYear = DateTime.Now.AddYears(1).Year,
				Reference = "Abc"
			};

			var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

			// Act
			var response = await _paymentGatewayApiClient.PostAsync("payments", content);

			// Assert
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();

			var createPaymentRequestResponseModel = JsonConvert.DeserializeObject<CreatePaymentRequestResponseModel>(responseContent);

			Assert.That(createPaymentRequestResponseModel.Status, Is.EqualTo(1));
		}

		[Test]
		public async Task ShouldBeAbleToRetrievePaymentRequest()
		{
			// Arrange
			var bankTransactionId = Guid.NewGuid();
			SetupBankResponse(bankTransactionId, true, null);

			var request = new
			{
				Amount = 100,
				CreditCardNumber = "111111111113",
				Currency = "GBP",
				CustomerName = "John Smith",
				CVV = "123",
				ExpiryMonth = 12,
				ExpiryYear = DateTime.Now.AddYears(1).Year,
				Reference = "Abc"
			};

			var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

			// Act
			var createPaymentRequestResponse = await _paymentGatewayApiClient.PostAsync("payments", content);
			var createPaymentResponseContent = await createPaymentRequestResponse.Content.ReadAsStringAsync();
			var createPaymentRequestResponseModel = JsonConvert.DeserializeObject<CreatePaymentRequestResponseModel>(createPaymentResponseContent);

			var getPaymentRequestResponse = await _paymentGatewayApiClient.GetAsync($"payments/{createPaymentRequestResponseModel.PaymentRequestId}");

			// Assert
			var getPaymentRequestResponseContent = await getPaymentRequestResponse.Content.ReadAsStringAsync();
			var paymentRequestModel = JsonConvert.DeserializeObject<GetPaymentRequestResponseModel>(getPaymentRequestResponseContent);

			Assert.That(paymentRequestModel.Amount, Is.EqualTo(100));
			Assert.That(paymentRequestModel.BankErrorDescription, Is.Null);
			Assert.That(paymentRequestModel.BankTransactionId, Is.EqualTo(bankTransactionId));
			Assert.That(paymentRequestModel.Currency, Is.EqualTo("GBP"));
			Assert.That(paymentRequestModel.CustomerName, Is.EqualTo("John Smith"));
			Assert.That(paymentRequestModel.ExpiryMonth, Is.EqualTo(12));
			Assert.That(paymentRequestModel.ExpiryYear, Is.EqualTo(request.ExpiryYear));
			Assert.That(paymentRequestModel.MaskedCreditCardNumber, Is.EqualTo("********1113"));
			Assert.That(paymentRequestModel.PaymentRequestId, Is.EqualTo(createPaymentRequestResponseModel.PaymentRequestId));
			Assert.That(paymentRequestModel.Reference, Is.EqualTo("Abc"));
			Assert.That(paymentRequestModel.Status, Is.EqualTo(1));
		}

		[Test]
		public async Task WhenBankFailsProcessingPayment_ShouldBeAbleToRetrievePaymentRequest()
		{
			// Arrange
			var bankTransactionId = Guid.NewGuid();
			SetupBankResponse(bankTransactionId, false, "Bank error not in your favour");

			var request = new
			{
				Amount = 100,
				CreditCardNumber = "111111111113",
				Currency = "GBP",
				CustomerName = "John Smith",
				CVV = "123",
				ExpiryMonth = 12,
				ExpiryYear = DateTime.Now.AddYears(1).Year,
				Reference = "Abc"
			};

			var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

			// Act
			var createPaymentRequestResponse = await _paymentGatewayApiClient.PostAsync("payments", content);
			var createPaymentResponseContent = await createPaymentRequestResponse.Content.ReadAsStringAsync();
			var createPaymentRequestResponseModel = JsonConvert.DeserializeObject<CreatePaymentRequestResponseModel>(createPaymentResponseContent);

			var getPaymentRequestResponse = await _paymentGatewayApiClient.GetAsync($"payments/{createPaymentRequestResponseModel.PaymentRequestId}");

			// Assert
			var getPaymentRequestResponseContent = await getPaymentRequestResponse.Content.ReadAsStringAsync();
			var paymentRequestModel = JsonConvert.DeserializeObject<GetPaymentRequestResponseModel>(getPaymentRequestResponseContent);

			Assert.That(paymentRequestModel.BankErrorDescription, Is.EqualTo("Bank error not in your favour"));
			Assert.That(paymentRequestModel.BankTransactionId, Is.EqualTo(bankTransactionId));
			Assert.That(paymentRequestModel.Status, Is.EqualTo(2));
		}

		private void SetupBankResponse(Guid id, bool wasSuccessful, string error)
		{
			_fakeBankServer.ResetMappings();

			var response = new
			{
				id,
				wasSuccessful,
				error
			};

			_fakeBankServer
				.Given(Request
					.Create()
					.WithPath("/payments/process")
					.UsingPost())
				.RespondWith(Response
					.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json")
					.WithBody(JsonConvert.SerializeObject(response)));
		}
	}
}

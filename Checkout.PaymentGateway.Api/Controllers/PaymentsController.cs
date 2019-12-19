using Checkout.PaymentGateway.Business.Common.Exceptions;
using Checkout.PaymentGateway.Business.Payments.Get;
using Checkout.PaymentGateway.Business.Payments.Process;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IProcessPaymentCommand _processPaymentCommand;
        private readonly IGetPaymentRequestQuery _getPaymentRequestQuery;

        public PaymentsController(
            IProcessPaymentCommand processPaymentCommand,
            IGetPaymentRequestQuery getPaymentRequestQuery)
        {
            _processPaymentCommand = processPaymentCommand;
            _getPaymentRequestQuery = getPaymentRequestQuery;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPaymentRequest()
        {
            // TODO: This is just for testing
            var result = await _processPaymentCommand.ExecuteAsync(new ProcessPaymentCommandRequestModel
            {
                Amount = 123,
                CreditCardNumber = "111111111113",
                Currency = "GBP",
                CustomerName = "John Smith",
                CVV = "123",
                ExpiryMonth = 12,
                ExpiryYear = 2020,
                Reference = "Abc"
            });

            if (result.Notification.HasErrors)
            {
                return BadRequest(result.Notification.ValidationErrors);
            }
            else
            {
                return Created($"payments/{result.PaymentRequestId}", result);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentRequest(Guid id)
        {
            try
            {
                return Ok(await _getPaymentRequestQuery.ExecuteAsync(id));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
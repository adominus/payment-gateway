using Checkout.PaymentGateway.Business.Common.Exceptions;
using Checkout.PaymentGateway.Business.Payments.Get;
using Checkout.PaymentGateway.Business.Payments.Process;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Checkout.PaymentGateway.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<IActionResult> ProcessPaymentRequest(ProcessPaymentCommandRequestModel request)
        {
            var result = await _processPaymentCommand.ExecuteAsync(request);

            if (result.Notification.HasErrors)
            {
                return BadRequest(result.Notification.ValidationErrors);
            }
            else
            {
                return Created($"payments/{result.PaymentRequestId}", new
                {
                    result.PaymentRequestId,
                    result.Status
                });
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
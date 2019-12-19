using Microsoft.AspNetCore.Mvc;
using System;

namespace Checkout.AcmeBank.Simulator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class PaymentsController : ControllerBase
	{
		[HttpPost("process")]
		public IActionResult ProcessPayment(decimal amount)
		{
			if (amount > 100m)
			{
				return Ok(new
				{
					Id = Guid.NewGuid(),
					WasSuccessful = false,
					Error = "Insufficient Funds"
				});
			}
			else
			{
				return Ok(new
				{
					Id = Guid.NewGuid(),
					WasSuccessful = true,
				});
			}
		}
	}
}

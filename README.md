# Description 
A payment gateway interacting with a simulated bank for Checkout.com. 

# Running 
Make sure you have [dotnet core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1) installed. 

Then to run the payment gateway you can run: 

`dotnet run --project Checkout.PaymentGateway.Api/Checkout.PaymentGateway.Api.csproj`

To run the simulated bank run: 

`dotnet run --project Checkout.AcmeBank.Simulator/Checkout.AcmeBank.Simulator.csproj`

To run both simultaneously (if you don't wish to use 2 terminals) you can run them both in the background with: 

`dotnet run --project Checkout.PaymentGateway.Api/Checkout.PaymentGateway.Api.csproj & dotnet run --project Checkout.AcmeBank.Simulator/Checkout.AcmeBank.Simulator.csproj &`

Then to end them both you can run: 

`kill %2 && kill %1`

The above assumes that the result of `jobs` returns just these 2 processes in the background.  

## Tests 
From the root of the project you can run the following: 

`dotnet test Checkout.PaymentGateway.sln -v n`

# Considerations & Assumptions 
## Payment Requests 
Much more information could be processed for a payment request. For example, it does not address any of the following: 
- **Multi-Tenancy** - There is a baked in assumption that this entire gateway will be used by a single merchant as there is no reference to merchants or tenants. 
- **Idempotency** - If the request is re-submitted then the gateway will attempt to make them some request to the bank again. To overcome this behaviour the request made to the gateway could contain a unique request Id. Using this the gateway could check if it's already attempted to process this request - and if so respond accordingly. 
- **Authentication** - These endpoints are open to use by all. 
- **Extend Payment Request Information** - The payment request requires only minimal information to proceed. Most likely in actual use it would require more information eg: 
    - Customer billing address
    - Customer shipping addresses
    - Customer contact information
    - It should be able to determine what merchant is making the request



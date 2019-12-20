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

## Testing 
By default the gateway runs on port `5000` for HTTP and `5001` for HTTPs. 

By default the simulated bank runs on port `8000` for HTTP and `8001` for HTTPs.

The simulated bank is setup to allow any request where the `amount <= 100`. Greater than that and it will process as `unsuccessful` with an `Insufficient Funds` error message.     

There is a postman collection at `testing/Payment Gateway.postman_collection.json`. This includes the 2 requests documented below with a default body for `Make Payment Request`. The resulting response ID from the `Make Payment Request` will get populated into a variable `{{PaymentRequestId}}` which can then be used to call `Get Payment Request`. 

## Running Unit and Acceptance Tests 
From the root of the project you can run the following: 

`dotnet test Checkout.PaymentGateway.sln -v n`

# Endpoints 
## Creating a payment request 
### Request
`POST /payments` 

Body: 
- `string CreditCardNumber` **required**
- `string CVV` *3 or 4 characters if provided*

- `int ExpiryMonth` **required**
- `int ExpiryYear` **required**

- `decimal Amount` **required**
- `string Currency` **required** *3 character ISO currency code*

- `string CustomerName` **required**
- `string Reference`

### Responses
#### 201 Created

- `Guid PaymentRequestId`
- `int Status` where int is one of: 
```
    Successful = 1,
    Unsuccessful = 2,
    UnableToProcess = 3
```

Example response: 
```
{
    "paymentRequestId": "1ac23ee1-8a10-452b-a0e1-179f6c81870e",
    "status": 1
}
```

#### 400 Bad Request
Returns an array of objects with the schema: 
- `string Attribute`
- `string Error`

Example response: 
```
[
    {
        "attribute": "CreditCardNumber",
        "error": "Credit card number is invalid"
    },
    {
        "attribute": "Currency",
        "error": "Currency not supported"
    }
]
```

## Retrieving a payment request 
### Request
`GET /payments/{paymentRequestId}` 
### Response

- `Guid PaymentRequestId`
- `int Status` where int is one of: 
```
    Successful = 1,
    Unsuccessful = 2,
    UnableToProcess = 3
```
- `Guid? BankTransactionId`
- `string BankErrorDescription`
- `string MaskedCreditCardNumber`
- `int ExpiryMonth`
- `int ExpiryYear`
- `decimal Amount`
- `string Currency`
- `string CustomerName`
- `string Reference`

Example response: 
```
{
    "paymentRequestId": "1ac23ee1-8a10-452b-a0e1-179f6c81870e",
    "status": 1,
    "bankTransactionId": "ffa036a2-cbdc-4668-962f-7f4059204cd3",
    "bankErrorDescription": null,
    "maskedCreditCardNumber": "********1113",
    "expiryMonth": 12,
    "expiryYear": 2020,
    "amount": 100,
    "currency": "GBP",
    "customerName": "John Smith",
    "reference": "Abc"
}
```

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

## Documentation 
- We could use something like swagger to document the endpoints

## General 
- The bank API URL should be loaded via configuration along with any credentials/API keys 


using GoCardless;
using GoCardless.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoCardlessHook.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoCardlessWebHookController : ControllerBase
{
    /*Handle failures
     * 
     * bank_authorisation_denied
     * payer	    billing_request_bank_authorisation_denied	A bank authorisation for this billing request has been denied by the payer.
     * gocardless	billing_request_bank_authorisation_denied	A bank authorisation for this billing request has been denied by the payer.
     * 
     * bank_authorisation_expired
     * 
     * payer	    billing_request_bank_authorisation_expired	A bank authorisation for this billing request has expired.
     * 
     * bank_authorisation_failed
     * 
     * payer	    billing_request_bank_authorisation_failed	A bank authorisation for this billing request has failed.
     * gocardless	billing_request_bank_authorisation_failed	A bank authorisation for this billing request has failed.
     * gocardless	insufficient_funds	                        A bank authorisation for this billing request has failed due to insufficient funds.
     * 
     * failed
     * 
     * gocardless	billing_request_failed	This billing request has failed.
     * api	        billing_request_failed	This billing request has failed.
     * 
     */

    private readonly ILogger<GoCardlessWebHookController> _logger;
    private readonly GoCardlessClient _goCardlessClient;

    public GoCardlessWebHookController(ILogger<GoCardlessWebHookController> logger, GoCardlessClient goCardlessClient)
    {
        _logger = logger;
        _goCardlessClient = goCardlessClient;
    }

    [HttpPost(Name = "GoCardlessWebHook")]
    public async Task Post()
    { 
        _logger.LogInformation("GoCardlessWebHook called");
        var requestBody = Request.Body;
        var reader = new StreamReader(requestBody);
        var requestJson = await reader.ReadToEndAsync();
        var signature = Request.Headers["Webhook-Signature"];
        var SECRET = Environment.GetEnvironmentVariable("GO_CARDLESS_WEBHOOKSECRET");

        var doc = WebhookParser.Parse(requestJson, SECRET, signature);



        foreach (var ev in doc)
        {
            var billingRequest = await GetBillingRequest(ev);
            if (billingRequest != null)
            {
                var customer = await GetCustomer(ev);
                if (customer != null)
                {
                    _logger.Log(LogLevel.Information, $"Billing Request {ev?.Id} : {ev?.CreatedAt} : {billingRequest?.Id} : {billingRequest?.Status} : {customer?.Id} : {customer?.Email} : CAUSE = {ev?.Details?.Cause}, DESCRIPTION = {ev?.Details?.Description}");
                    return;
                }
                _logger.Log(LogLevel.Error, $"Unknown Customer {ev?.Id} : {ev?.CreatedAt} : CAUSE = {ev?.Details?.Cause}, DESCRIPTION = {ev?.Details?.Description}");
            }
            _logger.Log(LogLevel.Error, $"Unknown Billing Request {ev?.Id} : {ev?.CreatedAt} : CAUSE = {ev?.Details?.Cause}, DESCRIPTION = {ev?.Details?.Description}");
        }
    }

    private async Task<BillingRequest?> GetBillingRequest(Event? ev)
    {
        var billingRequestID = ev?.Links.BillingRequest;
        var billingRequest = await _goCardlessClient.BillingRequests.GetAsync(billingRequestID);
        return billingRequest?.BillingRequest;
    }

    private async Task<Customer?> GetCustomer(Event? ev)
    {
        var customerID = ev?.Links.Customer;
        var customer = await _goCardlessClient.Customers.GetAsync(customerID);
        return customer?.Customer;
    }
}
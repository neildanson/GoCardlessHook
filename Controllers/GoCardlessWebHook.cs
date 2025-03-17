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
            try
            {
                if (ev.ResourceType == EventResourceType.BillingRequests)
                {
                    await HandleBillingRequest(ev);
                }
                else if (ev.ResourceType == EventResourceType.Subscriptions) //Direct Debit
                {
                    await HandleSubscription(ev);
                }
                else
                {
                    _logger.Log(LogLevel.Information, $"{ev.ResourceType} : Unknown Event {ev?.Id} : {ev?.CreatedAt} : {ev?.ResourceType} : {ev?.Action} : {ev?.Details?.Cause}, DESCRIPTION = {ev?.Details?.Description}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling event {ev}");
            }
        }
    }

    private async Task HandleSubscription(Event? ev)
    {
        var subscription = await GetSubscription(ev);
        if (subscription != null)
        {
            var customer = await GetCustomer(ev);
            if (customer != null)
            {
                _logger.Log(LogLevel.Information, $"Subscription {ev?.Id} : {ev?.CreatedAt} : {subscription?.Id} : {subscription?.Status} : {customer?.Id} : {customer?.Email} : CAUSE = {ev?.Details?.Cause}, DESCRIPTION = {ev?.Details?.Description}");
                return;
            }
            _logger.Log(LogLevel.Error, $"Unknown Customer {ev?.Id} : {ev?.CreatedAt} : CAUSE = {ev?.Details?.Cause}, DESCRIPTION = {ev?.Details?.Description}");
        }
        _logger.Log(LogLevel.Error, $"Unknown Subscription {ev?.Id} : {ev?.CreatedAt} : CAUSE = {ev?.Details?.Cause}, DESCRIPTION = {ev?.Details?.Description}");
    }

    private async Task HandleBillingRequest(Event? ev)
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

    private async Task<BillingRequest?> GetBillingRequest(Event? ev)
    {
        var billingRequestID = ev?.Links.BillingRequest;
        billingRequestID = billingRequestID?.Split(':')?[0];
        try
        {
            var billingRequest = await _goCardlessClient.BillingRequests.GetAsync(billingRequestID);
            return billingRequest?.BillingRequest;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"Error getting Billing Request {billingRequestID}");
            return null;
        }
        catch (GoCardlessException ex)
        {
            _logger.LogError(ex, $"Error getting Billing Request {billingRequestID}");
            return null;
        }
    }

    private async Task<Subscription?> GetSubscription(Event? ev)
    {
        var subscriptionID = ev?.Links.Subscription;
        var subscription = await _goCardlessClient.Subscriptions.GetAsync(subscriptionID);
        return subscription?.Subscription;
    }

    private async Task<Customer?> GetCustomer(Event? ev)
    {
        try
        {
            var customerID = ev?.Links.Customer;
            var customer = await _goCardlessClient.Customers.GetAsync(customerID);
            return customer?.Customer;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"Error getting Customer {ev?.Links.Customer}");
            return null;
        }
        catch (GoCardlessException ex)
        {
            _logger.LogError(ex, $"Error getting Customer {ev?.Links.Customer}");
            return null;
        }
    }
}
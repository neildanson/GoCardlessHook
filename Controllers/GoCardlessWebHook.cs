using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace GoCardlessHook.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoCardlessWebHookController : ControllerBase
{
    

    private readonly ILogger<GoCardlessWebHookController> _logger;

    public GoCardlessWebHookController(ILogger<GoCardlessWebHookController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "GoCardlessWebHook")]
    public void Post()
    { 
       _logger.LogInformation("GoCardlessWebHook called");
        var requestBody = Request.Body;
        var requestJson = new StreamReader(requestBody).ReadToEnd();

         var doc = JsonSerializer.Deserialize<GoCardlessWebHookDTO>(requestJson);

        //Currenlty 400 error code. 
        foreach (var ev in doc.events)
        {
            _logger.Log(LogLevel.Information, $"{ev?.id} : {ev?.created_at} : CAUSE = {ev?.details?.cause}, DESCRIPTION = {ev?.details?.description}");
        }
    }
}


/*
{
  "events": {
    "id": "EV123",
    "created_at": "2021-04-08T17:01:06.000Z",
    "resource_type": "billing_requests",
    "action": "created",
    "details": {
      "origin": "api",
      "cause": "billing_request_created",
      "description": "This billing request has been created."
    },
    "metadata": {},
    "resource_metadata": {
      "order_dispatch_date": "2014-05-22"
    },
    "links": {
      "billing_request": "BRQ123"
    }
  }
}
*/
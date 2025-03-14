using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GoCardlessHook.Controllers;

[ApiController]
[Route("[controller]")]
public class GoCardlessWebHook : ControllerBase
{
    

    private readonly ILogger<GoCardlessWebHook> _logger;

    public GoCardlessWebHook(ILogger<GoCardlessWebHook> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "GoCardlessWebHook")]
    public void Post([FromBody] GoCardlessWebHookDTO hook)
    {
        var ev = hook.events;
        _logger.Log(LogLevel.Information, $"{ev?.id} : {ev?.created_at} : CAUSE = {ev?.details?.cause}, DESCRIPTION = {ev?.details?.description}");
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
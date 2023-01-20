﻿using DragaliaAPI.Models;
using DragaliaAPI.Models.Generated;
using Microsoft.AspNetCore.Mvc;

namespace DragaliaAPI.Controllers.Dragalia;

[Route("login")]
[Consumes("application/octet-stream")]
[Produces("application/octet-stream")]
[ApiController]
public class LoginController : DragaliaControllerBase
{
    [HttpPost]
    [Route("index")]
    public DragaliaResult Index()
    {
        // TODO: Implement daily login bonuses/notifications
        return Code(ResultCode.Success);
    }

    [HttpPost]
    [Route("verify_jws")]
    public DragaliaResult VerifyJws()
    {
        return Code(ResultCode.Success);
    }
}

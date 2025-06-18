using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtherApp.API.Controllers.Base
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected int? GetUserId()
        {
            var loggedInUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(loggedInUser))
                return null;
            return int.Parse(loggedInUser);
        }

        protected string GetUserFullName()
        {
            var loggedInUser = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(loggedInUser))
                return string.Empty;
            return loggedInUser;
        }
    }
}
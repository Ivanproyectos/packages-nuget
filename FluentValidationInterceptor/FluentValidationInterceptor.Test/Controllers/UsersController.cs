using FluentValidationInterceptor.Test.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FluentValidationInterceptor.Test.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        public IActionResult Create([FromBody] CreateUserDto command)
        {
            return Ok(command);
        }
    }
}

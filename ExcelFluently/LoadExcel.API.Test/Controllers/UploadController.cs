using ExcelFluently.Extensions;
using LoadExcel.API.Test.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoadExcel.API.Test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        [HttpPost()]
        public IActionResult Upload(IFormFile file)
        {
            List<UserModel> users = file.ImportExcel<UserModel>(configure =>
                    configure.SheetName = "Users"
                )
                .MapColumn(x => x.Name)
                .MapColumn(x => x.Email)
                .MapColumn(x => x.DateOfBirth, "Date Of Birth")
                .MapColumn(x => x.Age)
                .MapColumn(x => x.Salary)
                .ToList();

            return Ok($"{users.Count} users imported.");
        }
    }
}

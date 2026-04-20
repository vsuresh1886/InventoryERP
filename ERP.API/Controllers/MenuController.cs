using ERP.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class MenuController : ControllerBase
    {

        private readonly IMenuService _menuService;
        public MenuController(IMenuService menuservice)
        {
            _menuService = menuservice;
        }   

        [HttpGet]
        public async Task<IActionResult> menu()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);

            var menu = await _menuService.GetMenuForUser(userId);

            return Ok(menu);
        }
    }
}



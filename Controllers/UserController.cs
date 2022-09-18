using Microsoft.AspNetCore.Mvc;
using todoapp.Models;
using todoapp.Services;

namespace todoapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController
        (UserService userService) =>
        _userService = userService;

    [HttpGet]
    public async Task<List<User>> Get()
    {

        return await _userService.GetAsync();

    }
    
    
    [HttpPost]
    public async Task<IActionResult> Post(User newUser)
    {
        await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }
}
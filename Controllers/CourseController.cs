using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using todoapp.DTO;
using todoapp.DTO.User;
using todoapp.Models;
using todoapp.Models.Courses;
using todoapp.Services;

namespace todoapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<List<Course>> Get()
    {
        return await _courseService.GetAsync();
    }

    [Middlewares.Auth.Authorize]
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromQuery] CourseRegisterParams registerParams)
    {
        var userEmail = ((User)HttpContext.Items["User"]!).Email; 
        await _courseService.AddToCourse(registerParams.CourseName, userEmail);
        return Ok();
    }
}
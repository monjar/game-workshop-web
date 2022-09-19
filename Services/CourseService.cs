using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using todoapp.Config;
using todoapp.DTO;
using todoapp.DTO.User;
using todoapp.Models;
using todoapp.Models.Courses;

namespace todoapp.Services;

public interface ICourseService
{
    public Task<List<Course>> GetAsync();
    public Task AddToCourse(string courseName, string userEmail);

}


public class CourseService : ICourseService
{
    private readonly ILogger _logger;

    private readonly IMongoCollection<Course> _coursesCollection;
    private readonly IUserService _userService;
    public CourseService(
        IOptions<WorkshopDatabaseSettings> workshopSettings, ILogger<UserService> logger,IUserService userService)
    {

        
        var mongoClient = new MongoClient(
            workshopSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            workshopSettings.Value.DatabaseName);

        _coursesCollection = mongoDatabase.GetCollection<Course>(
            workshopSettings.Value.CoursesCollectionName);

        _userService = userService;
        _logger = logger;
    }

    public async Task<List<Course>> GetAsync()
    {
        return await _coursesCollection.Find(_ => true).ToListAsync();
    }
    
    
    public async Task AddToCourse(string courseName, string userEmail)
    {
        var allCourses = await _coursesCollection.Find(_ => true).ToListAsync();

        var course = await _coursesCollection.Find(x => x.CourseName == courseName).FirstOrDefaultAsync();
        var user = await _userService.GetByEmailAsync(userEmail);
        course.Users.Add(user);
        await _coursesCollection.ReplaceOneAsync(x => x.Id == course.Id, course);
    }

}
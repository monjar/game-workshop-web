using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using todoapp.Config;
using todoapp.DTO.User;
using todoapp.Models;

namespace todoapp.Services;

public interface IUserService
{
    public Task<string> Login(LoginInputDTO loginInput);
    public Task CreateAsync(User newUser);
    public Task<List<User>> GetAsync();
    public Task<User?> GetAsync(string id);

    public Task<User?> GetByEmailAsync(string email);
}


public class UserService : IUserService
{
    private readonly ILogger _logger;

    private readonly IMongoCollection<User> _usersCollection;
    private byte[] salt = Encoding.ASCII.GetBytes("secret"); // divide by 8 to convert bits to bytes
    private readonly JWTSettings _jwtSettings;
    public UserService(
        IOptions<WorkshopDatabaseSettings> workshopSettings, ILogger<UserService> logger,IOptions<JWTSettings> jwtSettings)
    {
        Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

        
        var mongoClient = new MongoClient(
            workshopSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            workshopSettings.Value.DatabaseName);

        _usersCollection = mongoDatabase.GetCollection<User>(
            workshopSettings.Value.UsersCollectionName);

        _logger = logger;

        _jwtSettings = jwtSettings.Value;
    }

    public async Task<List<User>> GetAsync()
    {
        _logger.LogInformation("Here");
        return await _usersCollection.Find(_ => true).ToListAsync();
    }


    public async Task<User?> GetAsync(string id) =>
        await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser)
    {
        
        newUser.Password = HashPassword(newUser.Password);

        await _usersCollection.InsertOneAsync(newUser);
    }

    public async Task<string> Login(LoginInputDTO loginInput)
    {
  
        var hashedPassword = HashPassword(loginInput.Password);
        var user = await _usersCollection.Find(x => x.Email == loginInput.Email).FirstOrDefaultAsync();
        if (user is null)
            return "User Not Found!";
        if (!user.Password.Equals(hashedPassword))
            return "Passwords Don't Match!";
        
        return generateJwtToken(user);
    }

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);

    private string HashPassword(string password)
    {
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        return hashed;
    }
    
    private string generateJwtToken(User user)
    {
        // generate token that is valid for 7 days
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("email", user.Email) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using todoapp.DTO.User;
using todoapp.Models;

namespace todoapp.Services;

public class UserService
{
    private readonly ILogger _logger;

    private readonly IMongoCollection<User> _usersCollection;
    private byte[] salt = Encoding.ASCII.GetBytes("secret"); // divide by 8 to convert bits to bytes

    public UserService(
        IOptions<WorkshopDatabaseSettings> workshopSettings, ILogger<UserService> logger)
    {
        Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");


        var mongoClient = new MongoClient(
            workshopSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            workshopSettings.Value.DatabaseName);

        _usersCollection = mongoDatabase.GetCollection<User>(
            workshopSettings.Value.UsersCollectionName);

        _logger = logger;
    }

    public async Task<List<User>> GetAsync()
    {
        _logger.LogInformation("Here");
        return await _usersCollection.Find(_ => true).ToListAsync();
    }


    public async Task<User?> GetAsync(string id) =>
        await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

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
        else if (!user.Password.Equals(hashedPassword))
            return "Passwords Don't Match!";
        
        return "hi";
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
}
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using todoapp.Models;

namespace todoapp.Services;

public class UserService
{
    private readonly ILogger _logger;

    private readonly IMongoCollection<User> _usersCollection;
    byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
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
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: newUser.Password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        newUser.Password = hashed;
            
        await _usersCollection.InsertOneAsync(newUser);
    }

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);
}
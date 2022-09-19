using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Realms;
using todoapp.Models.Courses;

namespace todoapp.Models;
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { set; get; } = null!;
    
    
    public string Email { set; get; } = null!;
    
    [BsonElement("Name")]
    public string Name { set; get; } = null!;

    public string Password { set; get; } = null!;

}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace todoapp.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { set; get; }
    
    
    public string Email { set; get; }
    
    [BsonElement("Name")]
    public string Name { set; get; }
    
    public string Password { set; get; }

    
    
 
}
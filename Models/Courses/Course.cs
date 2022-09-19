using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Realms;

namespace todoapp.Models.Courses;

public class Course
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { set; get; } = null!;
    
    public string CourseName { set; get; } = null!;


    public IList<User> Users { get; set; } = new List<User>();

}
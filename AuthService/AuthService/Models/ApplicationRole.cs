using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace AuthService.Models;

[CollectionName("Roles")]
public class ApplicationRole : MongoIdentityRole<ObjectId>
{
    public ApplicationRole(string name) : base(name)
    {
    }

    public ApplicationRole() : base()
    {
    }
}

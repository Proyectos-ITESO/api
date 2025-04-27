// Services/Interfaces/IMongoService.cs
using MongoDB.Driver;

namespace MicroJack.API.Services.Interfaces
{
    public interface IMongoService
    {
        IMongoDatabase Database { get; }
    }
}
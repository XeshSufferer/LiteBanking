using LiteBanking.Models.Domain;

namespace LiteBanking.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetUserByPhoneNumber(string number, CancellationToken ct = default);
    public Task<User?> GetUserByUsername(string username, CancellationToken ct = default);
    public Task<User?> GetUserById(long id, CancellationToken ct = default);
    Task<User?> GetUserByKeywords(string keywords, CancellationToken ct = default);
    Task<User?> GetUserByKeywordsAndName(string login, string keywords, CancellationToken ct = default);
    
    public Task<bool> CreateUser(User user, CancellationToken ct = default);
    public Task<bool> UpdateUser(User user, CancellationToken ct = default);
    public Task<bool> DeleteUser(long id, CancellationToken ct = default);
}
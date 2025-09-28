using LiteBanking.Models.Domain;

namespace LiteBanking.Services.Interfaces;

public interface IAccountManagementService
{

    Task<User?> CreateAccount(string username, string keywords, CancellationToken ct = default);
    Task<User?> CreateAccount(string username, List<string> keywords, CancellationToken ct = default);
    Task<User?> Login(string username, string keywords, CancellationToken ct = default);
    Task<bool> DeleteAccount(string username, string keywords, CancellationToken ct = default);
    Task<bool> DeleteAccount(string username, List<string> keywords, CancellationToken ct = default);
    Task<bool> DeleteAccount(string userid, CancellationToken ct = default);
    Task<User?> Login(string username, List<string> keywords, CancellationToken ct = default);
}
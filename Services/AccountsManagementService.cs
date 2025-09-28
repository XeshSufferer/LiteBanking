using System.Speech.Recognition;
using LiteBanking.Helpers.Interfaces;
using LiteBanking.Models.Domain;
using LiteBanking.Repositories.Interfaces;
using LiteBanking.Services.Interfaces;
using LiteBanking.Сache;

namespace LiteBanking.Services;

public class AccountsManagementService(ICacheService cache, IUserRepository userRepository, IHashingHelper hashing) : IAccountManagementService
{
    
    public async Task<User?> CreateAccount(string username,string keywords, CancellationToken ct = default)
    {
        var user = new User()
        {
            Name = username,
            HashKeyRecoveryWord = hashing.Hash(keywords)
        };

        if (await userRepository.CreateUser(user, ct))
        {
            return user;
        }
        
        
        return null;
    }
    
    public async Task<User?> CreateAccount(string username, List<string> keywords, CancellationToken ct = default)
    {
        var user = new User()
        {
            Name = username,
            HashKeyRecoveryWord = hashing.Hash(keywords.ToString())
        };

        if (await userRepository.CreateUser(user, ct))
        {
            return user;
        }
        
        
        return null;
    }

    public async Task<User?> Login(string username, string keywords, CancellationToken ct = default)
    {
        return await userRepository.GetUserByKeywordsAndName(username, hashing.Hash(keywords), ct);
    }
    
    public async Task<User?> Login(string username, List<string> keywords, CancellationToken ct = default)
    {
        return await userRepository.GetUserByKeywordsAndName(username, hashing.Hash(keywords.ToString()), ct);
    }

    public async Task<bool> DeleteAccount(string username, string keywords, CancellationToken ct = default)
    {
        var userForDelete = await userRepository.GetUserByKeywordsAndName(username, hashing.Hash(keywords), ct);
        
        return await userRepository.DeleteUser(userForDelete.Id, ct);
    }
    
    public async Task<bool> DeleteAccount(string username, List<string> keywords, CancellationToken ct = default)
    {
        var userForDelete = await userRepository.GetUserByKeywordsAndName(username, hashing.Hash(keywords.ToString()), ct);
        
        return await userRepository.DeleteUser(userForDelete.Id, ct);
    }

    public async Task<bool> DeleteAccount(string userid, CancellationToken ct = default)
    {
        return await userRepository.DeleteUser(long.Parse(userid), ct);
    }
    
    
     
}
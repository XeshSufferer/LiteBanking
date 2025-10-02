using System.Linq;
using LiteBanking.EFCoreFiles;
using LiteBanking.Helpers.Interfaces;
using LiteBanking.Models.Domain;
using LiteBanking.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LiteBanking.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;
    private readonly IHashingHelper _hashing;

    public UserRepository(AppDbContext dbContext,
                          ILogger<UserRepository> logger,
                          IHashingHelper hashing)
    {
        _context = dbContext;
        _logger = logger;
        _hashing = hashing;
    }

    

    public async Task<bool> CreateUser(User user, CancellationToken ct = default)
    {
        try
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Unable to create user {Id}", user.Id);
            return false;
        }
    }

    public async Task<bool> DeleteUser(long id, CancellationToken ct = default)
    {
        try
        {
            await _context.Users.Where(u => u.Id == id)
                                .ExecuteDeleteAsync(ct);
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Unable to delete user {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateUser(User user, CancellationToken ct = default)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Unable to update user {Id}", user.Id);
            return false;
        }
    }

    

    public async Task<User?> GetUserById(long id, CancellationToken ct = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetUserByPhoneNumber(string number, CancellationToken ct = default) =>
        null; 

    public async Task<User?> GetUserByUsername(string username, CancellationToken ct = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Name == username, ct);

    public async Task<User?> GetUserByKeywords(string keywords, CancellationToken ct = default)
    {
        var candidates = await _context.Users
                                       .Where(u => u.HashKeyRecoveryWord != null)
                                       .Select(u => new { u, u.HashKeyRecoveryWord })
                                       .ToListAsync(ct);

        
        var matched = candidates
                     .FirstOrDefault(x => _hashing.Verify(keywords, x.HashKeyRecoveryWord));

        return matched?.u;
    }
    
    public async Task<User?> GetUserByKeywordsAndName(string login,
                                                    string keywords,
                                                    CancellationToken ct = default)
    {
        var user = await _context.Users
                                 .FirstOrDefaultAsync(u => u.Name == login, ct);
        
        if (user == null) return null;
        
        return _hashing.Verify(keywords, user.HashKeyRecoveryWord)
               ? user
               : null;
    }
}
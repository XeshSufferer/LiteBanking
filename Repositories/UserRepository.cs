using System.Data.Common;
using LiteBanking.EFCoreFiles;
using LiteBanking.Models.Domain;
using LiteBanking.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LiteBanking.Repositories;

public class UserRepository : IUserRepository
{

    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext dbContext, ILogger<UserRepository> logger)
    {
        _context = dbContext;
        _logger = logger;
    }

    public async Task<bool> CreateUser(User user, CancellationToken ct = default)
    {
        try
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Unable to create user {number}", user.PhoneNumber);
            return false;
        }
    }

    public async Task<bool> DeleteUser(long id, CancellationToken ct = default)
    {
        try
        {
            await _context.Users.Where(u => u.Id == id).ExecuteDeleteAsync(ct);
            return true;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Unable to delete user {id}", id);
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
        catch (DbUpdateException e)
        {
            _logger.LogError("Unable to update user {user}", user.PhoneNumber);
            return false;
        }
    }

    public async Task<User?> GetUserById(long id, CancellationToken ct = default) => 
        await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync(ct);

    public async Task<User?> GetUserByPhoneNumber(string number, CancellationToken ct = default) =>
        await _context.Users.Where(u => u.PhoneNumber == number).FirstOrDefaultAsync(ct);

    public async Task<User?> GetUserByUsername(string username, CancellationToken ct = default) =>
        await _context.Users.Where(u => u.Name == username).FirstOrDefaultAsync(ct);
}
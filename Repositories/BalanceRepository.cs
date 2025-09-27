using LiteBanking.EFCoreFiles;
using LiteBanking.Models.Domain;
using LiteBanking.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LiteBanking.Repositories;

public class BalanceRepository : IBalanceRepository
{
    
    private readonly AppDbContext _context;
    
    public BalanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateBalance(Balance balance, CancellationToken ct = default)
    {
        try
        {
            await _context.Balances.AddAsync(balance, ct);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException e)
        {
            return false;
        }
    }

    public async Task<Balance?> GetBalanceById(long id, CancellationToken ct = default) => 
        await _context.Balances.Where(b => b.Id == id).FirstOrDefaultAsync(ct);

    public async Task<bool> UpdateBalance(Balance balance, CancellationToken ct = default)
    {
        try
        {
            _context.Balances.Update(balance);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException e)
        {
            return false;
        }
    }

    public async Task<List<Balance>?> GetAllUserBalances(long userId, CancellationToken ct = default) =>
        await _context.Balances.Where(b => b.OwnerId == userId).ToListAsync(ct);
    
    
    
}
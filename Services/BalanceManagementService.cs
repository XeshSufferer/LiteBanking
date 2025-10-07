using LiteBanking.Models.Domain;
using LiteBanking.Repositories.Interfaces;
using LiteBanking.Services.Interfaces;
using LiteBanking.Сache;

namespace LiteBanking.Services;

public class BalanceManagementService(ICacheService cache, IBalanceRepository balanceRepository) : IBalanceManagementService
{
    public async Task<bool> Send(long from, long to, decimal amount, long userid, CancellationToken ct = default)
    {
        Balance? fromBal = await balanceRepository.GetBalanceById(from, ct);
        Balance? toBal   = await balanceRepository.GetBalanceById(to, ct);

        if (fromBal == null || toBal == null || fromBal.Amount < amount || fromBal.OwnerId != userid)
            return false;

        bool ok = await balanceRepository.Send(fromBal, toBal, ct);
        if (!ok) return false;
        
        await cache.RemoveAsync($"balance:{from}");
        await cache.RemoveAsync($"balance:{to}");
        return true;
    }

    public async Task<(bool, Balance)> CreateBalance(long userid ,CancellationToken ct = default)
    {
        Balance injectedBalance = new Balance()
        {
            OwnerId = userid,
            CreatedAt = DateTime.Now,
            Amount = 0
        };
        var result = await balanceRepository.CreateBalance(injectedBalance, ct);
        return (result, injectedBalance);
    }

    public async Task<decimal> GetBalanceAmount(long id, long userid, CancellationToken ct = default)
    {
        Balance? cachedBalance = await cache.GetAsync<Balance?>($"balance:{id}");
        if (cachedBalance != null)
        {
            return cachedBalance.Amount;
        }
        
        Balance? balance = await balanceRepository.GetBalanceById(id, ct);
        if(balance == null) return 0;
        if (balance.OwnerId != userid) return 0;
        await cache.SetAsync($"balance:{id}", balance, TimeSpan.FromHours(1));
        return balance.Amount;
    }
}
using LiteBanking.Models.Domain;

namespace LiteBanking.Repositories.Interfaces;

public interface IBalanceRepository
{
    public Task<Balance?> GetBalanceById(long id, CancellationToken ct = default);
    public Task<List<Balance>?> GetAllUserBalances(long userId, CancellationToken ct = default);
    public Task<bool> UpdateBalance(Balance balance, CancellationToken ct = default);
    
}
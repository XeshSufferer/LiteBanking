using LiteBanking.Models.Domain;

namespace LiteBanking.Services.Interfaces;

public interface IBalanceManagementService
{
    Task<(bool, Balance)> CreateBalance(long userid, CancellationToken ct = default);
    Task<bool> Send(long from, long to, decimal amount, CancellationToken ct = default);
    Task<decimal> GetBalanceAmount(long id, CancellationToken ct = default);
}
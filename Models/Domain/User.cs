using LiteBanking.EFCoreFiles;
using LiteBanking.Сache;
using Microsoft.EntityFrameworkCore;

namespace LiteBanking.Models.Domain;

public class User
{
    public long Id { get; set; }
    public string HashKeyRecoveryWord { get; set; } = default!;
    public ICollection<Balance> Balances { get; set; } = new List<Balance>();
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
using System;
using Microsoft.EntityFrameworkCore;

namespace LiteBanking.Models.Domain;


public class Balance
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}
namespace LiteBanking.Models.DTO;

public class SendMoneyRequestDTO
{
    public long From { get; set; }
    public long To { get; set; }
    public decimal Amount { get; set; }
}
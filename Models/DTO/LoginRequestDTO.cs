namespace LiteBanking.Models.DTO;

public class LoginRequestDTO
{
    public string Name { get; set; } = "";
    public List<string> Keywords { get; set; } = new List<string>();
}
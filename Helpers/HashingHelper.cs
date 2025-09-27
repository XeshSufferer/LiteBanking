using LiteBanking.Helpers.Interfaces;

namespace LiteBanking.Helpers;

public class HashingHelper : IHashingHelper
{
    public string Hash(string data)
    {
        return BCrypt.Net.BCrypt.HashPassword(data);
    }

    public bool Verify(string data, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(data, hash);
    }
}
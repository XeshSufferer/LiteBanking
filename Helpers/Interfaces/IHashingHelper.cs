namespace LiteBanking.Helpers.Interfaces;

public interface IHashingHelper
{
    string Hash(string data);
    bool Verify(string data, string hash);
}
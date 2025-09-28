namespace LiteBanking.Helpers.Interfaces;

public interface IRandomWordGeneratorHelper
{
    Task<string> GetRandomWord(CancellationToken token = default);
    Task<List<string>> GetRandomWords(int count, CancellationToken token = default);
}
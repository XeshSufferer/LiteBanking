using System.Text.Json;
using gnuciDictionary;
using LiteBanking.Helpers.Interfaces;

namespace LiteBanking.Helpers;

public class RandomWordGeneratorHelper : IRandomWordGeneratorHelper
{
    
    private readonly Random _random = new Random();
    
    public async Task<string> GetRandomWord(CancellationToken token = default)
    {
        var words = EnglishDictionary.GetAllWords().ToArray();
        return words[_random.Next(0, words.Length)].Value;
    }

    public async Task<List<string>> GetRandomWords(int count, CancellationToken token = default)
    {
        List<string> words = new(); 
        for (int i = 0; i != count; i++)
        {
            words.Add(await GetRandomWord(token));
        }
        return words;
    }
}
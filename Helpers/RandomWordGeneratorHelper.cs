using System.Text.Json;
using LiteBanking.Helpers.Interfaces;

namespace LiteBanking.Helpers;

public class RandomWordGeneratorHelper : IRandomWordGeneratorHelper
{
    
    private readonly string _apiUrl = "https://random-word-api.herokuapp.com/word?number=";
    
    public async Task<string> GetRandomWord(CancellationToken token = default)
    {
        HttpResponseMessage content;
        using (HttpClient client = new HttpClient())
        {
            content = await client.GetAsync(_apiUrl + 1.ToString(), token);
        }
        return JsonSerializer.Deserialize<List<string>>(await content.Content.ReadAsStringAsync(token))[0];
    }

    public async Task<List<string>> GetRandomWords(int count, CancellationToken token = default)
    {
        HttpResponseMessage content;
        using (HttpClient client = new HttpClient())
        {
            content = await client.GetAsync(_apiUrl + count.ToString(), token);
        }
        return JsonSerializer.Deserialize<List<string>>(await content.Content.ReadAsStringAsync(token));
    }
}
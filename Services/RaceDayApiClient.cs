using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.MVC.Services;

public interface IRaceDayApiClient
{
    Task<T?> GetAsync<T>(string endpoint, string? token = null);
    Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, string? token = null);
    Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, string? token = null);
    Task<HttpResponseMessage> DeleteAsync(string endpoint, string? token = null);
}

public class RaceDayApiClient : IRaceDayApiClient
{
    private readonly HttpClient _httpClient;

    public RaceDayApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private void AddAuthHeader(string? token)
    {
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        else
            _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
    {
        AddAuthHeader(token);
        var response = await _httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode) return default;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, string? token = null)
    {
        AddAuthHeader(token);
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _httpClient.PostAsync(endpoint, content);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, string? token = null)
    {
        AddAuthHeader(token);
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _httpClient.PutAsync(endpoint, content);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint, string? token = null)
    {
        AddAuthHeader(token);
        return await _httpClient.DeleteAsync(endpoint);
    }
}
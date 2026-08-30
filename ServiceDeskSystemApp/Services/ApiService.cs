using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceDeskSystemApp.Models.Common;

namespace ServiceDeskSystemApp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // Setup JSON options to correctly parse enums as strings
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        
        // Base address for Android Emulator pointing to local IIS Express / Kestrel
        // For physical device or Windows, change this to your actual IP or localhost
        var isAndroid = DeviceInfo.Platform == DevicePlatform.Android;
        var baseUrl = isAndroid ? "http://10.0.2.2:5182" : "http://localhost:5182";
        
        _httpClient.BaseAddress = new Uri(baseUrl);
        
        if (isAndroid)
        {
            // IIS Express and some Kestrel configs reject Host: 10.0.2.2
            _httpClient.DefaultRequestHeaders.Host = "localhost";
        }
    }

    public virtual async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }
    
    public virtual async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }
    
    public virtual async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
        return response.IsSuccessStatusCode;
    }

    public virtual Task SetAuthTokenAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return Task.CompletedTask;
    }
    
    public virtual Task ClearAuthTokenAsync()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        return Task.CompletedTask;
    }
}

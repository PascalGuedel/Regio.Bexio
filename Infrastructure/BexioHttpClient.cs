using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Regio.Bexio.Infrastructure;

internal interface IBexioHttpClient
{
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request);
    
    Task PostAsync(string url);
    
    Task<TResponse?> GetAsync<TResponse>(string url);

    Task DeleteAsync(string url);
}

internal class BexioHttpClient : IBexioHttpClient
{
    private readonly HttpClient _client;

    public BexioHttpClient(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        var apiKey = configuration.GetValue<string>("Bexio:ApiToken");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        client.BaseAddress = new Uri(configuration.GetValue<string>("Bexio:ApiBaseAddress")!);
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var httpContent = new StringContent(JsonSerializer.Serialize(request));
        var httpResponseMsg = await _client.PostAsync(url, httpContent);

        return await HandleResponseAsync<TResponse>(httpResponseMsg);
    }

    public async Task PostAsync(string url)
    {
        var httpResponseMsg = await _client.PostAsync(url, null);

        await HandleResponseAsync(httpResponseMsg);
    }

    public async Task<TResponse?> GetAsync<TResponse>(string url)
    {
        var response = await _client.GetAsync(url);

        return await HandleResponseAsync<TResponse>(response);
    }

    public async Task DeleteAsync(string url)
    {
        await _client.DeleteAsync(url);
    }

    private static async Task<TResponse?> HandleResponseAsync<TResponse>(HttpResponseMessage httpResponse)
    {
        var responseContentString = await httpResponse.Content.ReadAsStringAsync();

        if (httpResponse.IsSuccessStatusCode)
        {
            var responseObject = JsonSerializer.Deserialize<TResponse>(responseContentString);

            return responseObject;
        }

        throw new HttpRequestException($"{httpResponse.StatusCode}: {responseContentString}");
    }
    
    private static async Task HandleResponseAsync(HttpResponseMessage httpResponse)
    {
        var responseContentString = await httpResponse.Content.ReadAsStringAsync();

        if (httpResponse.IsSuccessStatusCode)
        {
            return;
        }

        throw new HttpRequestException($"{httpResponse.StatusCode}: {responseContentString}");
    }
}

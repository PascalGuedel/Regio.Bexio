using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Regio.Bexio.Infrastructure;

internal interface IBexioHttpClient
{
    Task<TResponse?> PostAsync<TResponse>(string url, HttpContent content);

    Task<TResponse?> PostAsync<TResponse>(string url);

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

    public async Task<TResponse?> PostAsync<TResponse>(string url, HttpContent content)
    {
        var httpResponseMsg = await _client.PostAsync(url, content);

        return await HandleResponseAsync<TResponse>(httpResponseMsg);
    }

    public async Task<TResponse?> PostAsync<TResponse>(string url)
    {
        var httpResponseMsg = await _client.PostAsync(url, null);

        return await HandleResponseAsync<TResponse>(httpResponseMsg);
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
}

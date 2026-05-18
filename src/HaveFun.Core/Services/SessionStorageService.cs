using System.Text.Json;
using Microsoft.JSInterop;

namespace HaveFun.Core;

public sealed class SessionStorageService : ISessionStorageService
{
    private const string SessionStorageKey = "havefun.sessionStorage";
    private readonly IJSRuntime jsRuntime;

    public SessionStorageService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public event Action? CurrentUserChanged;

    public async ValueTask<SessionStorageModel?> GetCurrentUserAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", SessionStorageKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SessionStorageModel>(json);
        }
        catch (JsonException)
        {
            await ClearCurrentUserAsync();
            return null;
        }
    }

    public async ValueTask SaveCurrentUserAsync(SessionStorageModel sessionStorage)
    {
        var json = JsonSerializer.Serialize(sessionStorage);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", SessionStorageKey, json);
        CurrentUserChanged?.Invoke();
    }

    public async ValueTask ClearCurrentUserAsync()
    {
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", SessionStorageKey);
        CurrentUserChanged?.Invoke();
    }
}

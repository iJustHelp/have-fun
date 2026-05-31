using System.Text.Json;
using Microsoft.JSInterop;

namespace HaveFun.Core;

public sealed class SessionStorageService : ISessionStorageService
{
    private const string _sessionStorageKey = "havefun.sessionStorage";
    private readonly IJSRuntime _jsRuntime;

    public SessionStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public event Action? CurrentUserChanged;

    public async ValueTask<SessionStorageModel?> GetCurrentUserAsync()
    {
        var json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", _sessionStorageKey);

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
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", _sessionStorageKey, json);
        CurrentUserChanged?.Invoke();
    }

    public async ValueTask ClearCurrentUserAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", _sessionStorageKey);
        CurrentUserChanged?.Invoke();
    }
}

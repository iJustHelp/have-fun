using System.Text.Json;
using Microsoft.JSInterop;

namespace HaveFun.Web;

public sealed class BrowserUserSessionStorage : IUserSessionStorage
{
    private const string StorageKey = "havefun.currentUser";
    private readonly IJSRuntime jsRuntime;

    public BrowserUserSessionStorage(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public async ValueTask<StoredUserSession?> GetCurrentUserAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<StoredUserSession>(json);
        }
        catch (JsonException)
        {
            await ClearCurrentUserAsync();
            return null;
        }
    }

    public async ValueTask SaveCurrentUserAsync(StoredUserSession userSession)
    {
        var json = JsonSerializer.Serialize(userSession);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, json);
    }

    public ValueTask ClearCurrentUserAsync()
    {
        return jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
    }
}

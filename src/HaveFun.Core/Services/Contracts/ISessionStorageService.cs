namespace HaveFun.Core;

public interface ISessionStorageService
{
    event Action? CurrentUserChanged;

    ValueTask<SessionStorageModel?> GetCurrentUserAsync();

    ValueTask SaveCurrentUserAsync(SessionStorageModel userSession);

    ValueTask ClearCurrentUserAsync();
}

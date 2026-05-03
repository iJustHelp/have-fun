namespace HaveFun.Web;

public interface IUserSessionStorageService
{
    ValueTask<StoredUserSession?> GetCurrentUserAsync();

    ValueTask SaveCurrentUserAsync(StoredUserSession userSession);

    ValueTask ClearCurrentUserAsync();
}

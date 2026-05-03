namespace HaveFun.Web;

public interface IUserSessionStorage
{
    ValueTask<StoredUserSession?> GetCurrentUserAsync();

    ValueTask SaveCurrentUserAsync(StoredUserSession userSession);

    ValueTask ClearCurrentUserAsync();
}

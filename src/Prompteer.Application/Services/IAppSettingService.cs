namespace Prompteer.Application.Services;

public interface IAppSettingService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value);
    Task<Dictionary<string, string>> GetAllAsync();
    Task SaveManyAsync(Dictionary<string, string> settings);
}

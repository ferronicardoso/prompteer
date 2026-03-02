using Microsoft.EntityFrameworkCore;
using Prompteer.Application.Services;
using Prompteer.Domain.Entities;
using Prompteer.Infrastructure.Data;

namespace Prompteer.Infrastructure.Services;

public class AppSettingService(AppDbContext db) : IAppSettingService
{
    public async Task<string?> GetAsync(string key)
    {
        var s = await db.AppSettings.FindAsync(key);
        return s?.Value;
    }

    public async Task SetAsync(string key, string value)
    {
        var existing = await db.AppSettings.FindAsync(key);
        if (existing is null)
        {
            db.AppSettings.Add(new AppSetting { Key = key, Value = value, UpdatedAt = DateTime.UtcNow });
        }
        else
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.UtcNow;
            db.AppSettings.Update(existing);
        }
        await db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        return await db.AppSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    public async Task SaveManyAsync(Dictionary<string, string> settings)
    {
        var existing = await db.AppSettings.ToDictionaryAsync(s => s.Key);
        foreach (var (key, value) in settings)
        {
            if (existing.TryGetValue(key, out var entity))
            {
                entity.Value = value;
                entity.UpdatedAt = DateTime.UtcNow;
                db.AppSettings.Update(entity);
            }
            else
            {
                db.AppSettings.Add(new AppSetting { Key = key, Value = value, UpdatedAt = DateTime.UtcNow });
            }
        }
        await db.SaveChangesAsync();
    }
}

using Clip.Models;

namespace Clip.Repositories;

public interface IConfigurationRepository
{
    ClipConfigurationModel? Get();
    void Save(ClipConfigurationModel model);
    Dictionary<string, int> GetItemCounts();
}

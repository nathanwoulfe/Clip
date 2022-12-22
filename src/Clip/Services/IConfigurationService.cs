using Clip.Models;

namespace Clip.Services;

/// <summary>
/// Defines the interface for the configuration service.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Get the configuration model for determining permitted child types.
    /// </summary>
    /// <returns><see cref="ClipConfigurationModel"/>.</returns>
    ClipConfigurationModel GetConfigurationModel();

    /// <summary>
    /// Saves the configuration model.
    /// </summary>
    /// <param name="model"><see cref="ClipConfigurationModel"/>.</param>
    void Save(ClipConfigurationModel model);

    /// <summary>
    /// Get the configuration model for the settings view.
    /// </summary>
    /// <returns><see cref="ClipConfigurationModel"/>.</returns>
    ClipConfigurationModel Get();
}

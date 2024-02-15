using System.Reflection;
using Umbraco.Cms.Core.Manifest;

namespace Clip;

/// <summary>
/// Adds the backoffice files.
/// </summary>
internal sealed class ClipManifestFilter : IManifestFilter
{
    /// <inheritdoc/>
    public void Filter(List<PackageManifest> manifests) => manifests.Add(new()
    {
        PackageName = Constants.Name,
        AllowPackageTelemetry = true,
        Version = GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty,
        Scripts =
            [
                "/App_Plugins/Clip/Backoffice/js/clip.min.js",
            ],
        BundleOptions = BundleOptions.None,
    });
}

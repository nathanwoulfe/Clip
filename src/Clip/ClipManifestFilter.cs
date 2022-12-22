using Umbraco.Cms.Core.Manifest;

namespace Clip;

/// <summary>
/// Adds the backoffice files.
/// </summary>
internal sealed class ClipManifestFilter : IManifestFilter
{
    /// <inheritdoc/>
    public void Filter(List<PackageManifest> manifests) => manifests.Add(new PackageManifest
    {
        PackageName = Constants.Name,
        Scripts = new[]
            {
                "/App_Plugins/Clip/Backoffice/js/clip.min.js",
            },
        BundleOptions = BundleOptions.None,
    });
}

using Umbraco.Cms.Core.Manifest;

namespace Clip;

internal sealed class ClipManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new PackageManifest
        {
            PackageName = Constants.Name,
            Scripts = new[]
            {
                "/App_Plugins/Clip/Backoffice/js/clip.js"
            },
            //Stylesheets = new[]
            //{
            //    "/App_Plugins/Clip/Backoffice/css/styles.css"
            //},
            BundleOptions = BundleOptions.None,
        });
    }
}
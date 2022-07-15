export class ClipInterceptor implements IClipInterceptor {

    public static serviceName = 'clipInterceptor';

    constructor(private $q) {

    }

    request(req) {
        if (!req.url
            .toLowerCase()
            .startsWith(Umbraco.Sys.ServerVariables.umbracoUrls.contentTypeApiBaseUrl + 'getallowedchildren'))
            return (req || this.$q.when(req));

        const url = new URL('https://foo.com' + req.url);
        const qs = url.search;

        req.url = Umbraco.Sys.ServerVariables.Clip.configurationApiBaseUrl + 'getallowedchildren' + qs;

        return (req || this.$q.when(req));
    }

    //async response(result) {
    //    if (!result.config.url
    //        .toLowerCase()
    //        .startsWith(Umbraco.Sys.ServerVariables.umbracoUrls.contentTypeApiBaseUrl + 'getallowedchildren'))
    //        return result;

    //    const response = await fetch(Umbraco.Sys.ServerVariables.Clip.configurationApiBaseUrl + 'getconfigmodel');
    //    const data = await response.json();

    //    const allowedChildren:Array<any> = data.allowedChildren; 

    //    if (!allowedChildren.length)
    //        return result;

    //    result.data = result.data.filter(d => allowedChildren.includes(d.key));

    //    return result;
    //}
}

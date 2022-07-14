export class ClipInterceptor implements IClipInterceptor {

    public static serviceName = 'clipInterceptor';

    private _getAllowedChildrenPath = Umbraco.Sys.ServerVariables.umbracoUrls.contentTypeApiBaseUrl + 'getAllowedChildren';

    response = result => {
        if (!result.config.url.toLowerCase().startsWith(this._getAllowedChildrenPath))
            return result;

        const allowedChildren = Umbraco.Sys.ServerVariables.Clip.allowedChildren; 

        if (!allowedChildren.length)
            return result;

        result.data = result.data.filter(d => allowedChildren.includes(d.key));

        return result;
    }
}

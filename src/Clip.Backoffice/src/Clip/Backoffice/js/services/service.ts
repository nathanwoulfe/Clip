export class ClipService implements IClipService {

    public static serviceName = 'clipService';

    constructor(private $http, private umbRequestHelper) {
    }

    private _request = (method: 'POST' | 'GET', url: string, data?: object) => {
        url = `${Umbraco.Sys.ServerVariables.Clip.configurationApiBaseUrl}${url}`;

        return this.umbRequestHelper.resourcePromise(
            method === 'POST' ? this.$http.post(url, data)
                : this.$http.get(url),
            'Something broke'
        );
    }

    save = (config: IClipConfigModel) => this._request('POST', 'Save', config);

    get = () => this._request('GET', 'Get');
}
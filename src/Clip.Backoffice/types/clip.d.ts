interface IClipConfigModel {
    groupId: number;
    contentTypeKeys: Array<string>;
}

interface IClipConfigViewModel extends IClipConfigModel {
    groupName: string;
    icon: string;
}

interface IClipService {
    get: () => Promise<Array<IClipConfigViewModel>>;
    set: (config: Array<IClipConfigModel>) => Promise<any>;
}

interface IClipStore {
    get: () => Array<IClipConfigModel>;
    set: (config: Array<IClipConfigModel>) => void;
}

interface IClipInterceptor {
    response: (result: any) => any;
}
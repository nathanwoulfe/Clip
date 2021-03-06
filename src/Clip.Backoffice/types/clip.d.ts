interface IClipConfigModel {
    groups: Array<IClipGroupConfigModel>;
    existingItemCounts?: { [key: string]: number };
    contentTypeCounts: Array<IClipContentTypeCountModel>;
}

interface IClipContentTypeCountModel {
    count: number | string;
    alias: string;
    key: string;
    id: number;
    name?: string;
    icon?: string;
}

interface IClipGroupConfigModel {
    groupId: number;
    contentTypeKeys: Array<string>;
    groupName?: string;
    icon?: string;
}

interface IClipService {
    get: () => Promise<IClipConfigModel>;
    save: (config: IClipConfigModel) => Promise<any>;
}

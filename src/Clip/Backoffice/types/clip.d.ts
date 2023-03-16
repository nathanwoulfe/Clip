interface IClipConfigModel {
    groups: Array<IClipGroupConfigModel>;
    existingItemCounts?: { [key: string]: number };
    contentTypeCounts: Array<IClipContentTypeCountModel>;
}

interface IClipContentTypeCountModel {
    count: number | string;
    alias: string;
    udi: IUdiModel;
    id: number;
    name?: string;
    icon?: string;
    uniqueId: string;
}

interface IClipGroupConfigModel {
    groupId: number;
    udis: Array<any>; // this is dodgy - fetch/save settings uses a different model
    groupName?: string;
    icon?: string;
}

interface IUdiModel {
    uriValue: string;
    entityType: string;
}

interface IClipService {
    get: () => Promise<IClipConfigModel>;
    save: (config: IClipConfigModel) => Promise<any>;
}

interface IClipPickerOptions {
    multiPicker: boolean;
    filterCssClass?: string;
    filter?: (item: any) => boolean | undefined;
    submit: (model: any) => void;
    close: () => void;
}

interface IClipOverview {
    getIcon: (obj: { udi: string, [key: string]: any }) => void;
    editorService: any;
    filterCssClass: string;
    documentTypeKey: string;
    mediaTypeKey: string;
    config: any;
    openPicker: (type: 'document-type' | 'media-type', options: IClipPickerOptions) => void;
}

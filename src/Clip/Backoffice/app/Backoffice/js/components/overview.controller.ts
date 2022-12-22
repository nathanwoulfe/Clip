export class OverviewController {

    public static controllerName = 'Clip.Overview.Controller';

    name!: string;
    config!: IClipConfigModel;
    syncModel: { [key: string]: Array<UmbContentType> } = {};

    constructor(
        private clipService: IClipService,
        private localizationService) {
    }

    $onInit = async () => {
        this.name = await this.localizationService.localize('treeHeaders_contentCreationRules');        
    }

    async save() {
        let config: IClipConfigModel = {
            groups: [],
            contentTypeCounts: this.config.contentTypeCounts,
        };

        Object.keys(this.syncModel).forEach(k => {
            config.groups.push({
                groupId: +k,
                udis: this.syncModel[+k].map(x => x.udi )
            });
        });

        await this.clipService.save(config);
    }
}

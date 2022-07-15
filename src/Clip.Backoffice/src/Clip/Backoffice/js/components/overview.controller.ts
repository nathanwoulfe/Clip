export class OverviewController {

    public static controllerName = 'Clip.Overview.Controller';

    config!: IClipConfigModel;
    groups: Array<UmbUserGroup> = [];
    allContentTypes: Array<UmbContentType> = [];

    name!: string;

    contentTypeSyncModel: { [key: string]: Array<UmbContentType> } = {};

    constructor(
        private $q,
        private clipService: IClipService,
        private editorService,
        private localizationService,
        private notificationsService,
        private contentTypeResource,
        private userGroupsResource) {
    }

    private _getContentTypeIcon(type) {
        type.icon = this.allContentTypes.find(t => t.key === type.key)?.icon;
    }

    $onInit() {
        const promises = [
            this.contentTypeResource.getAll(),            
            this.clipService.get(),
            this.userGroupsResource.getUserGroups({ onlyCurrentUserGroups: false }),
            this.localizationService.localize('treeHeaders_contentCreationRules'),
        ]

        this.$q.all(promises)
            .then(resp => {
                this.allContentTypes = resp[0];
                this.config = resp[1];
                this.groups = resp[2];
                this.name = resp[3];

                this.config.groups.forEach(c => {
                    if (!c.groupId) return;

                    const group = this.groups.find(g => g.id == c.groupId);
                    if (!group) return;

                    c.icon = group?.icon;
                    c.groupName = group?.name;

                    let contentTypeSyncModel: Array<UmbContentType> = [];

                    c.contentTypeKeys.forEach(key => {
                        const contentType = this.allContentTypes.find(x => x.key === key);
                        if (!contentType) return;

                        contentTypeSyncModel.push(contentType);
                    });

                    this.contentTypeSyncModel[c.groupId] = contentTypeSyncModel;
                });

                this.config.contentTypeCounts.forEach(c => {
                    const type = this.allContentTypes.find(t => t.key === c.key);
                    if (!type) return;

                    c.icon = type.icon;
                    c.name = type.name;
                })
            });
    }

    removeGroup = index => {
        const g = this.config.groups[index];
        delete this.contentTypeResource[g.groupId];
        this.config.groups.splice(index, 1);
    }

    addGroup = () => {
        const groupPickerOptions = {
            submit: model => {
                model.selection.forEach(s => {
                    const idx = this.config.groups.findIndex(x => x.groupId == s.id);
                    if (idx !== -1) return;

                    this.config.groups.push({
                        icon: s.icon,
                        groupId: s.id,
                        groupName: s.name,
                        contentTypeKeys: [],
                    });
                });

                this.editorService.close();
            },
            close: () => this.editorService.close()
        };

        this.editorService.userGroupPicker(groupPickerOptions);
    }

    addContentType(groupId) {
        const typePickerOptions = {
            multiPicker: false,
            filterCssClass: 'not-allowed not-published',
            filter: item => (this.contentTypeSyncModel[groupId] || [])
                .some(x => x.id == item.id), // coerce string to int                
            submit: model => {
                const valueArray = this.contentTypeSyncModel[groupId] || [];
                const value = model.selection[0];

                this._getContentTypeIcon(value);
                valueArray.push(value);
                
                this.contentTypeSyncModel[groupId] = valueArray;
                this.editorService.close();
            },
            close: () => this.editorService.close()
        };

        this.editorService.contentTypePicker(typePickerOptions);
    }

    removeContentType(type, groupId) {
        const idx = this.contentTypeSyncModel[groupId].findIndex(x => x.key === type.key);
        this.contentTypeSyncModel[groupId].splice(idx, 1);
    }

    addContentTypeLimit() {
        const typePickerOptions = {
            multiPicker: false,
            filterCssClass: 'not-allowed not-published',
            filter: item => this.config.contentTypeCounts.some(x => x.id == item.id), // coerce string to int                
            submit: model => {
                const value: IClipContentTypeCountModel = model.selection[0];
                value.count = this.config.existingItemCounts ? this.config.existingItemCounts[value.key] : '';
                this._getContentTypeIcon(value);

                this.config.contentTypeCounts.push(value);
                this.editorService.close();
            },
            close: () => this.editorService.close()
        };

        this.editorService.contentTypePicker(typePickerOptions);
    }

    removeContentTypeCount(type) {
        const idx = this.config.contentTypeCounts.findIndex(x => x.id === type.id);
        this.config.contentTypeCounts.splice(idx, 1);
    }

    save() {
        let config: IClipConfigModel = {
            groups: [],
            contentTypeCounts: this.config.contentTypeCounts,
        };

        Object.keys(this.contentTypeSyncModel).forEach(k => {
            config.groups.push({
                groupId: +k,
                contentTypeKeys: this.contentTypeSyncModel[+k].map(x => x.key)
            });
        });

        this.clipService.set(config)
            .then(
                resp => this.notificationsService.success('Success', resp),
                err => this.notificationsService.error('Error', err),
            );
    }
}

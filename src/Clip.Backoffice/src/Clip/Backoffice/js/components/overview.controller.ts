export class OverviewController {

    public static controllerName = 'Clip.Overview.Controller';

    config: Array<IClipConfigViewModel> = [];
    groups: Array<UmbUserGroup> = [];
    allContentTypes: Array<UmbContentType> = [];

    contentTypeSyncModel: {[key: string]: Array<UmbContentType>} = {};

    constructor(
        private $q,
        private clipService: IClipService,
        private editorService,
        private notificationsService,
        private contentTypeResource,
        private userGroupsResource) {
    }

    $onInit() {
        const promises = [
            this.contentTypeResource.getAll(),
            this.clipService.get(),
            this.userGroupsResource.getUserGroups({ onlyCurrentUserGroups: false })
        ]

        this.$q.all(promises)
            .then(resp => {
                this.allContentTypes = resp[0];
                this.config = resp[1] || [];
                this.groups = resp[2];

                this.config.forEach(c => {
                    if (!c.groupId) return;

                    const group = this.groups.find(g => g.id == c.groupId);
                    if (!group) return;

                    c.icon = group?.icon;
                    c.groupName = group?.name;

                    let contentTypeSyncModel: Array<UmbContentType> = [];

                    c.contentTypeKeys.forEach(key => {
                        const contentType = this.allContentTypes.find(x => x.key === key);
                        if (!contentType) return;

                        contentTypeSyncModel.push();
                    });

                    this.contentTypeSyncModel[c.groupId] = contentTypeSyncModel;
                });
            });
    }

    remove(index) {
        this.config.splice(index, 1);
    }

    add = () => {
        console.log('yo!')
        const groupPickerOptions = {
            submit: model => {
                model.selection.forEach(s => {
                    const idx = this.config.findIndex(x => x.groupId == s.id);
                    if (idx === -1) {
                        this.config.push({
                            icon: s.icon,
                            groupId: s.id,
                            groupName: s.name,
                            contentTypeKeys: [],
                        });
                    }
                });

                this.editorService.close();
            },
            close: () => this.editorService.close()
        };

        this.editorService.userGroupPicker(groupPickerOptions);
    }

    addContentType(groupId) {
        const typePickerOptions = {
            multiPicker: true,
            submit: model => {
                const valueArray = this.contentTypeSyncModel[groupId] || [];

                valueArray.push(...model.selection.filter(x => {
                    // discard if already in the sync model
                    if (valueArray.find(y => y.key === x.key)) return false;

                    // items from the treepicker don't have icons, but we have all content types
                    // in a local variable, so get from that set
                    x.icon = this.allContentTypes.find(y => y.key === x.key)?.icon;

                    return true;
                }));

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

    save() {
        let config: Array<IClipConfigModel> = [];

        Object.keys(this.contentTypeSyncModel).forEach(k => {
            config.push({
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

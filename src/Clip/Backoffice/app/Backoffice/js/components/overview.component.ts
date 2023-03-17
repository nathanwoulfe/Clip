import { EntityType } from "../clip";

class Overview implements IClipOverview {

  config!: IClipConfigModel;
  groups: Array<UmbUserGroup> = [];
  contentTypes: Array<UmbContentType> = [];
  mediaTypes: Array<UmbContentType> = [];
  syncModel!: { [key: string]: Array<UmbContentType> };

  editorService;

  documentTypeKey = 'A2CB7800-F571-4787-9638-BC48539A0EFB';
  mediaTypeKey = '4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E';
  filterCssClass = 'not-allowed not-published';

  constructor(
    private $q,
    private $scope,
    private clipService: IClipService,
    private mediaTypeResource,
    private userGroupsResource,
    private contentTypeResource,
    editorService,
  ) {
    this.editorService = editorService;
  }

  $onInit = async () => {
    const promises = [
      this.contentTypeResource.getAll(),
      this.mediaTypeResource.getAll(),
      this.clipService.get(),
      this.userGroupsResource.getUserGroups({ onlyCurrentUserGroups: false }), ,
    ];

    [this.contentTypes, this.mediaTypes, this.config, this.groups] = await this.$q.all(promises);

    this.config.groups.forEach(g => this.populateSyncModel(g));

    this.config.contentTypeCounts.forEach(c => this.populateCountModel(c));

    this.$scope.$apply();
  }

  private getTypeByUdi = (udi: IUdiModel) => {
    const type = this.contentTypes.find(x => x.udi === udi.uriValue) || this.mediaTypes.find(x => x.udi === udi.uriValue);
    return type;
  }

  populateCountModel = (c: IClipContentTypeCountModel) => {
    const type = this.getTypeByUdi(c.udi);
    if (!type) return;

    c.icon = type.icon;
    c.name = type.name;
  }

  populateSyncModel = (g: IClipGroupConfigModel) => {
    if (!g.groupId) return;

    const group = this.groups.find(x => x.id == g.groupId);
    if (!group) return;

    g.icon = group?.icon;
    g.groupName = group?.name;

    let contentTypeSyncModel: Array<UmbContentType> = [];

    g.udis.forEach(udi => {
      const type = this.getTypeByUdi(udi);
      if (!type) return;

      contentTypeSyncModel.push(type);
    });

    this.syncModel[g.groupId] = contentTypeSyncModel;
  }

  getIcon(type: { udi: string, [key: string]: any }) {
    type.icon = (type.udi.includes(EntityType.DocumentType) ? this.contentTypes : this.mediaTypes)
      .find(t => t.udi === type.udi)?.icon;
  }

  removeGroup = id => {
    const g = this.config.groups.find(g => g.groupId === id);
    if (g === undefined) return;
    delete this.syncModel[g.groupId];

    const idx = this.config.groups.findIndex(g => g.groupId === id);
    this.config.groups.splice(idx, 1);
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
            udis: [],
          });
        });

        this.editorService.close();
      },
      close: () => this.editorService.close()
    };

    this.editorService.userGroupPicker(groupPickerOptions);
  }

  isItemFilteredInPicker(id, item, isElement: boolean) {
    if (item.metaData.tree.includes('mediaTypes')) {
      return (this.syncModel[id] || []).some(x => x.id == item.id);
    }

    return item.nodeType === 'container' ||
      item.metaData.isElement !== isElement ||
      (this.syncModel[id] || []).some(x => x.id == item.id);
  }

  addType(id, type: EntityType, isElement: boolean) {
    const typePickerOptions = {
      multiPicker: true,
      filterCssClass: this.filterCssClass,
      filter: item => this.isItemFilteredInPicker(id, item, isElement),
      submit: model => {
        const valueArray = this.syncModel[id] || [];
        model.selection.forEach(value => {
          this.getIcon(value);
          value.isElement = isElement;
          valueArray.push(value);
        });

        this.syncModel[id] = valueArray;
        this.editorService.close();
      },
      close: () => this.editorService.close()
    };

    this.openPicker(type, typePickerOptions);
  }

  removeType(type, id) {
    const idx = this.syncModel[id].findIndex(x => x.udi === type.udi);
    this.syncModel[id].splice(idx, 1);
  }

  openPicker(type: 'document-type' | 'media-type', options: IClipPickerOptions) {
    if (type === EntityType.DocumentType) {
      this.editorService.contentTypePicker(options);
    } else {
      this.editorService.mediaTypePicker(options);
    }
  }
}

const template = `
<div class="umb-editor-sub-header justify-start items-center mb0">
  <h5>
    <localize key="clip_userGroupRules">User group rules</localize>
  </h5>
  <umb-button type="button"
              button-style="outline"
              class="ml2"
              state="init"
              action="$ctrl.addGroup()"
              label-key="general_add">
  </umb-button>
</div>



<div class="umb-table" ng-if="$ctrl.config.groups.length">
  <div class="umb-table-head">
    <div class="umb-table-row">
      <div class="umb-table-cell">
      </div>
      <div class="umb-table-cell">
        <localize key="user_userGroup">User group</localize>
      </div>
      <div class="umb-table-cell">
        <localize key="clip_allowedDocumentTypes">Allowed document types</localize>
      </div>
      <div class="umb-table-cell">
        <localize key="clip_allowedElementTypes">Allowed element types</localize>
      </div>
      <div class="umb-table-cell">
        <localize key="clip_allowedMediaTypes">Allowed media types</localize>
      </div>
      <div class="umb-table-cell umb-table-cell--small" style="max-width:95px">
      </div>
    </div>
  </div>
  <div class="umb-table-body">
    <div ng-repeat="group in $ctrl.config.groups track by $index" class="umb-table-row">
      <div class="umb-table-cell">
        <umb-icon icon="{{ group.icon }}" class="umb-table-body__icon umb-table-body__fileicon umb-icon"></umb-icon>
      </div>
      <div class="umb-table-cell">
        <a class="umb-table-body__link" href="#/users/users/group/{{ group.groupId }}">{{ group.groupName }}</a>
      </div>
      <div class="umb-table-cell flex-column mt0">
        <div class="mb2">
          <umb-node-preview ng-repeat="type in $ctrl.syncModel[group.groupId] | filter: {udi: 'document-type', isElement: false}"
                            name="type.name"
                            icon="type.icon"
                            sortable="false"
                            allow-edit="false"
                            allow-remove="true"
                            on-remove="$ctrl.removeType(type, group.groupId)">
          </umb-node-preview>
        </div>
        <div class="umb-block-list__actions" style="overflow:visible">
          <button type="button"
                  class="btn-reset umb-block-list__create-button umb-outline"
                  class="umb-node-preview__action ml-auto mt3"
                  ng-click="$ctrl.addType(group.groupId, 'document-type', false)">
            <localize key="general_add">Add</localize>
          </button>
        </div>
      </div>
      <div class="umb-table-cell flex-column mt0">
        <div class="mb2">
          <umb-node-preview ng-repeat="type in $ctrl.syncModel[group.groupId] | filter: {udi: 'document-type', isElement: true}"
                            name="type.name"
                            icon="type.icon"
                            sortable="false"
                            allow-edit="false"
                            allow-remove="true"
                            on-remove="$ctrl.removeType(type, group.groupId)">
          </umb-node-preview>
        </div>
        <div class="umb-block-list__actions" style="overflow:visible">
          <button type="button"
                  class="btn-reset umb-block-list__create-button umb-outline"
                  ng-click="$ctrl.addType(group.groupId, 'document-type', true)">
            <localize key="general_add">Add</localize>
          </button>
        </div>
      </div>
      <div class="umb-table-cell flex-column mt0">
        <div class="mb2">
          <umb-node-preview ng-repeat="type in $ctrl.syncModel[group.groupId] | filter: {udi: 'media-type'}"
                            name="type.name"
                            icon="type.icon"
                            sortable="false"
                            allow-edit="false"
                            allow-remove="true"
                            on-remove="$ctrl.removeType(type, group.groupId)">
          </umb-node-preview>
        </div>
        <div class="umb-block-list__actions" style="overflow:visible">
          <button type="button"
                  class="btn-reset umb-block-list__create-button umb-outline"
                  ng-click="$ctrl.addType(group.groupId, 'media-type', false)">
            <localize key="general_add">Add</localize>
          </button>
        </div>
      </div>
      <div class="umb-table-cell umb-table-cell--small" style="max-width:95px">
        <div class="umb-node-preview__actions">
          <button type="button"
                  class="umb-node-preview__action umb-node-preview__action--red"
                  ng-click="$ctrl.removeGroup(group.groupId)">
            <localize key="general_remove">Remove</localize>
          </button>
        </div>
      </div>
    </div>
  </div>
</div>

<umb-empty-state ng-if="!$ctrl.config.groups.length">
  <localize key="content_listViewNoItems">There are no items show in the list.</localize>
</umb-empty-state>

<type-limits-table type="document-type" header-key="clip_contentTypeLimits" type-key="clip_contentType" config="$ctrl.config"></type-limits-table>
<type-limits-table type="media-type" header-key="clip_mediaTypeLimits" type-key="clip_mediaType" config="$ctrl.config"></type-limits-table>`;

export const OverviewComponent = {
  name: 'clipOverview',
  transclude: true,
  template,
  controller: Overview,
  bindings: {
    config: '=',
    syncModel: '=',
  },
}

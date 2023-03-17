import { EntityType } from "../clip";

class TypeLimitsTable {

  config!: IClipConfigModel;
  items!: Array<IClipContentTypeCountModel>;
  type!: EntityType;
  parent!: IClipOverview;

  private _filtered: boolean = false;

  $doCheck = () => {
    if (this._filtered || !this.config) return;

    this.items = this.config.contentTypeCounts.filter(x => x.udi.entityType === this.type);
    this._filtered = true;
  }

  addType = () => {
    const typePickerOptions: IClipPickerOptions = {
      multiPicker: true,
      filterCssClass: this.parent.filterCssClass,
      filter: item => item.nodeType === 'container' || item.metaData.isElement
        || this.items.some(x => x.udi.uriValue == item.udi),
      submit: model => {
        model.selection.forEach(value => {
          value.count = (this.config.existingItemCounts ? this.config.existingItemCounts[value.udi] : 0) ?? 0;
          value.uniqueId = value.key;
          value.nodeObjectType = this.type === EntityType.DocumentType ? this.parent.documentTypeKey : this.parent.mediaTypeKey;

          this.parent.getIcon(value);
          this.items.push(value);
          this.config.contentTypeCounts.push(value);
        });

        this.parent.editorService.close();
      },
      close: () => this.parent.editorService.close()
    };

    this.parent.openPicker(this.type, typePickerOptions);
  }

  removeType = (uniqueId: string) => {
    let idx = this.items.findIndex(x => x.uniqueId === uniqueId);
    this.items.splice(idx, 1);

    idx = this.config.contentTypeCounts.findIndex(x => x.uniqueId === uniqueId);
    this.config.contentTypeCounts.splice(idx, 1);
  }
}

const template = `
    <div class="content-type-counts">
        <div class="umb-editor-sub-header justify-start items-center mb0">
            <h5>
                <localize key="{{ $ctrl.headerKey }}">Type limits</localize>
            </h5>
            <umb-button type="button"
                        button-style="outline"
                        class="ml2"
                        state="init"
                        action="$ctrl.addType()"
                        label-key="general_add">
            </umb-button>
        </div>

        <div class="umb-table" ng-if="$ctrl.items.length">
            <div class="umb-table-head">
                <div class="umb-table-row">
                    <div class="umb-table-cell">

                    </div>
                    <div class="umb-table-cell">
                        <localize key="{{ $ctrl.typeKey }}">Type</localize>
                    </div>
                    <div class="umb-table-cell">
                        <localize key="clip_maxItemsOfType">Max items of type</localize>
                    </div>
                    <div class="umb-table-cell">
                        <localize key="clip_currentItemsOfType">Current items of type</localize>
                    </div>
                    <div class="umb-table-cell umb-table-cell--small">

                    </div>
                </div>
            </div>
            <div class="umb-table-body">
                <div ng-repeat="type in $ctrl.items track by $index" class="umb-table-row">
                    <div class="umb-table-cell">
                        <umb-icon icon="{{ type.icon }}" class="umb-table-body__icon umb-table-body__fileicon umb-icon"></umb-icon>
                    </div>
                    <div class="umb-table-cell">
                        <div class="umb-table-body__link">{{ type.name }}</div>
                    </div>
                    <div class="umb-table-cell">
                        <input type="number" ng-model="type.max" style="margin-bottom:0" />
                    </div>
                    <div class="umb-table-cell">
                        {{ type.count }}
                    </div>
                    <div class="umb-table-cell umb-table-cell--small">
                        <div class="umb-node-preview__actions">
                            <button type="button"
                                    class="umb-node-preview__action umb-node-preview__action--red"
                                    ng-click="$ctrl.removeType(type.uniqueId)">
                                <localize key="general_remove">Remove</localize>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <umb-empty-state ng-if="!$ctrl.items.length">
            <localize key="content_listViewNoItems">There are no items show in the list.</localize>
        </umb-empty-state>
    </div>`;

export const TypeLimitsTableComponent = {
  name: 'typeLimitsTable',
  transclude: true,
  require: {
    parent: '^clipOverview',
  },
  bindings: {
    headerKey: '@',
    typeKey: '@',
    type: '@',
    config: '=',
  },
  template,
  controller: TypeLimitsTable
};

(function(){function r(e,n,t){function o(i,f){if(!n[i]){if(!e[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var p=n[i]={exports:{}};e[i][0].call(p.exports,function(r){var n=e[i][1][r];return o(n||r)},p,p.exports,r,e,n,t)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<t.length;i++)o(t[i]);return o}return r})()({1:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.EntityType = void 0;
const _module_1 = require("./components/_module");
const _module_2 = require("./services/_module");
const name = 'clip';
angular.module(name, [
    _module_2.ServicesModule,
    _module_1.ComponentsModule,
]);
angular.module('umbraco').requires.push(name);
var EntityType;
(function (EntityType) {
    EntityType["DocumentType"] = "document-type";
    EntityType["MediaType"] = "media-type";
})(EntityType = exports.EntityType || (exports.EntityType = {}));

},{"./components/_module":2,"./services/_module":6}],2:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ComponentsModule = void 0;
const overview_controller_1 = require("./overview.controller");
const overview_component_1 = require("./overview.component");
const type_limits_table_component_1 = require("./type-limits-table.component");
exports.ComponentsModule = angular
    .module('clip.components', [])
    .controller(overview_controller_1.OverviewController.controllerName, overview_controller_1.OverviewController)
    .component(overview_component_1.OverviewComponent.name, overview_component_1.OverviewComponent)
    .component(type_limits_table_component_1.TypeLimitsTableComponent.name, type_limits_table_component_1.TypeLimitsTableComponent)
    .name;

},{"./overview.component":3,"./overview.controller":4,"./type-limits-table.component":5}],3:[function(require,module,exports){
"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.OverviewComponent = void 0;
const clip_1 = require("../clip");
class Overview {
    constructor($q, clipService, mediaTypeResource, userGroupsResource, contentTypeResource, editorService) {
        this.$q = $q;
        this.clipService = clipService;
        this.mediaTypeResource = mediaTypeResource;
        this.userGroupsResource = userGroupsResource;
        this.contentTypeResource = contentTypeResource;
        this.groups = [];
        this.contentTypes = [];
        this.mediaTypes = [];
        this.documentTypeKey = 'A2CB7800-F571-4787-9638-BC48539A0EFB';
        this.mediaTypeKey = '4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E';
        this.filterCssClass = 'not-allowed not-published';
        this.$onInit = () => __awaiter(this, void 0, void 0, function* () {
            const promises = [
                this.contentTypeResource.getAll(),
                this.mediaTypeResource.getAll(),
                this.clipService.get(),
                this.userGroupsResource.getUserGroups({ onlyCurrentUserGroups: false }), ,
            ];
            [this.contentTypes, this.mediaTypes, this.config, this.groups] = yield this.$q.all(promises);
            this.config.groups.forEach(g => this.populateSyncModel(g));
            this.config.contentTypeCounts.forEach(c => this.populateCountModel(c));
        });
        this.getTypeByUdi = (udi) => {
            const type = this.contentTypes.find(x => x.udi === udi.uriValue) || this.mediaTypes.find(x => x.udi === udi.uriValue);
            return type;
        };
        this.populateCountModel = (c) => {
            const type = this.getTypeByUdi(c.udi);
            if (!type)
                return;
            c.icon = type.icon;
            c.name = type.name;
        };
        this.populateSyncModel = (g) => {
            if (!g.groupId)
                return;
            const group = this.groups.find(x => x.id == g.groupId);
            if (!group)
                return;
            g.icon = group === null || group === void 0 ? void 0 : group.icon;
            g.groupName = group === null || group === void 0 ? void 0 : group.name;
            let contentTypeSyncModel = [];
            g.udis.forEach(udi => {
                const type = this.getTypeByUdi(udi);
                if (!type)
                    return;
                contentTypeSyncModel.push(type);
            });
            this.syncModel[g.groupId] = contentTypeSyncModel;
        };
        this.removeGroup = index => {
            const g = this.config.groups[index];
            delete this.contentTypeResource[g.groupId];
            this.config.groups.splice(index, 1);
        };
        this.addGroup = () => {
            const groupPickerOptions = {
                submit: model => {
                    model.selection.forEach(s => {
                        const idx = this.config.groups.findIndex(x => x.groupId == s.id);
                        if (idx !== -1)
                            return;
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
        };
        this.editorService = editorService;
    }
    getIcon(type) {
        var _a;
        type.icon = (_a = (type.udi.includes(clip_1.EntityType.DocumentType) ? this.contentTypes : this.mediaTypes)
            .find(t => t.udi === type.udi)) === null || _a === void 0 ? void 0 : _a.icon;
    }
    addType(groupId, type) {
        const typePickerOptions = {
            multiPicker: true,
            filterCssClass: this.filterCssClass,
            filter: item => item.nodeType === 'container' || item.metaData.isElement || (this.syncModel[groupId] || [])
                .some(x => x.id == item.id),
            submit: model => {
                const valueArray = this.syncModel[groupId] || [];
                model.selection.forEach(value => {
                    this.getIcon(value);
                    valueArray.push(value);
                });
                this.syncModel[groupId] = valueArray;
                this.editorService.close();
            },
            close: () => this.editorService.close()
        };
        this.openPicker(type, typePickerOptions);
    }
    removeType(type, groupId) {
        const idx = this.syncModel[groupId].findIndex(x => x.udi === type.udi.udiValue);
        this.syncModel[groupId].splice(idx, 1);
    }
    openPicker(type, options) {
        if (type === clip_1.EntityType.DocumentType) {
            this.editorService.contentTypePicker(options);
        }
        else {
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
                <localize key="clip_allowedMediaTypes">Allowed media types</localize>
            </div>
            <div class="umb-table-cell umb-table-cell--small">

            </div>
        </div>
    </div>
    <div class="umb-table-body">
        <div ng-repeat="group in $ctrl.config.groups track by $index" class="umb-table-row">
            <div class="umb-table-cell">
                <umb-icon icon="{{ group.icon }}" class="umb-table-body__icon umb-table-body__fileicon umb-icon"></umb-icon>
            </div>
            <div class="umb-table-cell">
                <div class="umb-table-body__link">{{ group.groupName }}</div>
            </div>
            <div class="umb-table-cell flex-column mt0">
                <div class="mb3">
                    <umb-node-preview ng-repeat="type in $ctrl.syncModel[group.groupId] | filter: {udi: 'document-type'}"
                                        name="type.name"
                                        icon="type.icon"
                                        sortable="false"
                                        allow-edit="false"
                                        allow-remove="true"
                                        on-remove="$ctrl.removeType(type, group.groupId)">
                    </umb-node-preview>
                </div>
                <button type="button"
                        class="umb-node-preview__action ml0 mr0"
                        ng-click="$ctrl.addType(group.groupId, 'document-type')">
                    <localize key="clip_addContentType">Add document type</localize>
                </button>
            </div>
            <div class="umb-table-cell flex-column mt0">
                <div class="mb3">
                    <umb-node-preview ng-repeat="type in $ctrl.syncModel[group.groupId] | filter: {udi: 'media-type'}"
                                        name="type.name"
                                        icon="type.icon"
                                        sortable="false"
                                        allow-edit="false"
                                        allow-remove="true"
                                        on-remove="$ctrl.removeType(type, group.groupId)">
                    </umb-node-preview>
                </div>
                <button type="button"
                        class="umb-node-preview__action ml0 mr0"
                        ng-click="$ctrl.addType(group.groupId, 'media-type')">
                    <localize key="clip_addMediaType">Add media type</localize>
                </button>
            </div>
            <div class="umb-table-cell umb-table-cell--small">
                <div class="umb-node-preview__actions">
                    <button type="button"
                            class="umb-node-preview__action umb-node-preview__action--red"
                            ng-click="$ctrl.removeGroup($index)">
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
exports.OverviewComponent = {
    name: 'clipOverview',
    transclude: true,
    template,
    controller: Overview,
    bindings: {
        config: '=',
        syncModel: '=',
    },
};

},{"../clip":1}],4:[function(require,module,exports){
"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.OverviewController = void 0;
class OverviewController {
    constructor(clipService, localizationService) {
        this.clipService = clipService;
        this.localizationService = localizationService;
        this.syncModel = {};
        this.$onInit = () => __awaiter(this, void 0, void 0, function* () {
            this.name = yield this.localizationService.localize('treeHeaders_contentCreationRules');
        });
    }
    save() {
        return __awaiter(this, void 0, void 0, function* () {
            let config = {
                groups: [],
                contentTypeCounts: this.config.contentTypeCounts,
            };
            Object.keys(this.syncModel).forEach(k => {
                config.groups.push({
                    groupId: +k,
                    udis: this.syncModel[+k].map(x => x.udi)
                });
            });
            yield this.clipService.save(config);
        });
    }
}
exports.OverviewController = OverviewController;
OverviewController.controllerName = 'Clip.Overview.Controller';

},{}],5:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TypeLimitsTableComponent = void 0;
const clip_1 = require("../clip");
class TypeLimitsTable {
    constructor() {
        this._filtered = false;
        this.$doCheck = () => {
            if (this._filtered || !this.config)
                return;
            this.items = this.config.contentTypeCounts.filter(x => x.udi.entityType === this.type);
            this._filtered = true;
        };
        this.addType = () => {
            const typePickerOptions = {
                multiPicker: true,
                filterCssClass: this.parent.filterCssClass,
                filter: item => item.nodeType === 'container' || item.metaData.isElement
                    || this.items.some(x => x.udi.uriValue == item.udi),
                submit: model => {
                    model.selection.forEach(value => {
                        var _a;
                        value.count = (_a = (this.config.existingItemCounts ? this.config.existingItemCounts[value.udi] : 0)) !== null && _a !== void 0 ? _a : 0;
                        value.uniqueId = value.key;
                        value.nodeObjectType = this.type === clip_1.EntityType.DocumentType ? this.parent.documentTypeKey : this.parent.mediaTypeKey;
                        this.parent.getIcon(value);
                        this.items.push(value);
                        this.config.contentTypeCounts.push(value);
                    });
                    this.parent.editorService.close();
                },
                close: () => this.parent.editorService.close()
            };
            this.parent.openPicker(this.type, typePickerOptions);
        };
        this.removeType = (uniqueId) => {
            let idx = this.items.findIndex(x => x.uniqueId === uniqueId);
            this.items.splice(idx, 1);
            idx = this.config.contentTypeCounts.findIndex(x => x.uniqueId === uniqueId);
            this.config.contentTypeCounts.splice(idx, 1);
        };
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
exports.TypeLimitsTableComponent = {
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

},{"../clip":1}],6:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ServicesModule = void 0;
const service_1 = require("./service");
exports.ServicesModule = angular
    .module('clip.services', [])
    .factory(service_1.ClipService.serviceName, service_1.ClipService)
    .name;

},{"./service":7}],7:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ClipService = void 0;
class ClipService {
    constructor($http, umbRequestHelper) {
        this.$http = $http;
        this.umbRequestHelper = umbRequestHelper;
        this._request = (method, url, data) => {
            url = `${Umbraco.Sys.ServerVariables.Clip.configurationApiBaseUrl}${url}`;
            return this.umbRequestHelper.resourcePromise(method === 'POST' ? this.$http.post(url, data)
                : this.$http.get(url), 'Something broke');
        };
        this.save = (config) => this._request('POST', 'Save', config);
        this.get = () => this._request('GET', 'Get');
    }
}
exports.ClipService = ClipService;
ClipService.serviceName = 'clipService';

},{}]},{},[1,3,4,5,2,7,6]);

//# sourceMappingURL=clip.js.map

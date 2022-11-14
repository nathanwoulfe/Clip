(function(){function r(e,n,t){function o(i,f){if(!n[i]){if(!e[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var p=n[i]={exports:{}};e[i][0].call(p.exports,function(r){var n=e[i][1][r];return o(n||r)},p,p.exports,r,e,n,t)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<t.length;i++)o(t[i]);return o}return r})()({1:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const _module_1 = require("./components/_module");
const _module_2 = require("./services/_module");
const name = 'clip';
angular.module(name, [
    _module_2.ServicesModule,
    _module_1.ComponentsModule,
]);
angular.module('umbraco').requires.push(name);

},{"./components/_module":2,"./services/_module":4}],2:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ComponentsModule = void 0;
const overview_controller_1 = require("./overview.controller");
exports.ComponentsModule = angular
    .module('clip.components', [])
    .controller(overview_controller_1.OverviewController.controllerName, overview_controller_1.OverviewController)
    .name;

},{"./overview.controller":3}],3:[function(require,module,exports){
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
    constructor($q, clipService, editorService, localizationService, contentTypeResource, userGroupsResource) {
        this.$q = $q;
        this.clipService = clipService;
        this.editorService = editorService;
        this.localizationService = localizationService;
        this.contentTypeResource = contentTypeResource;
        this.userGroupsResource = userGroupsResource;
        this.groups = [];
        this.allContentTypes = [];
        this.contentTypeSyncModel = {};
        this._filterCssClass = 'not-allowed not-published';
        this.$onInit = () => __awaiter(this, void 0, void 0, function* () {
            const promises = [
                this.contentTypeResource.getAll(),
                this.clipService.get(),
                this.userGroupsResource.getUserGroups({ onlyCurrentUserGroups: false }),
                this.localizationService.localize('treeHeaders_contentCreationRules'),
            ];
            [this.allContentTypes, this.config, this.groups, this.name] = yield this.$q.all(promises);
            this.config.groups.forEach(c => {
                if (!c.groupId)
                    return;
                const group = this.groups.find(g => g.id == c.groupId);
                if (!group)
                    return;
                c.icon = group === null || group === void 0 ? void 0 : group.icon;
                c.groupName = group === null || group === void 0 ? void 0 : group.name;
                let contentTypeSyncModel = [];
                c.contentTypeKeys.forEach(key => {
                    const contentType = this.allContentTypes.find(x => x.key === key);
                    if (!contentType)
                        return;
                    contentTypeSyncModel.push(contentType);
                });
                this.contentTypeSyncModel[c.groupId] = contentTypeSyncModel;
            });
            this.config.contentTypeCounts.forEach(c => {
                const type = this.allContentTypes.find(t => t.key === c.key);
                if (!type)
                    return;
                c.icon = type.icon;
                c.name = type.name;
            });
        });
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
                            contentTypeKeys: [],
                        });
                    });
                    this.editorService.close();
                },
                close: () => this.editorService.close()
            };
            this.editorService.userGroupPicker(groupPickerOptions);
        };
    }
    _getContentTypeIcon(type) {
        var _a;
        type.icon = (_a = this.allContentTypes.find(t => t.key === type.key)) === null || _a === void 0 ? void 0 : _a.icon;
    }
    addContentType(groupId) {
        const typePickerOptions = {
            multiPicker: false,
            filterCssClass: this._filterCssClass,
            filter: item => (this.contentTypeSyncModel[groupId] || [])
                .some(x => x.id == item.id),
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
            filterCssClass: this._filterCssClass,
            filter: item => this.config.contentTypeCounts.some(x => x.id == item.id),
            submit: model => {
                const value = model.selection[0];
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
        return __awaiter(this, void 0, void 0, function* () {
            let config = {
                groups: [],
                contentTypeCounts: this.config.contentTypeCounts,
            };
            Object.keys(this.contentTypeSyncModel).forEach(k => {
                config.groups.push({
                    groupId: +k,
                    contentTypeKeys: this.contentTypeSyncModel[+k].map(x => x.key)
                });
            });
            yield this.clipService.save(config);
        });
    }
}
exports.OverviewController = OverviewController;
OverviewController.controllerName = 'Clip.Overview.Controller';

},{}],4:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ServicesModule = void 0;
const service_1 = require("./service");
exports.ServicesModule = angular
    .module('clip.services', [])
    .factory(service_1.ClipService.serviceName, service_1.ClipService)
    .name;

},{"./service":5}],5:[function(require,module,exports){
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

},{}]},{},[1,3,2,5,4]);

//# sourceMappingURL=clip.js.map

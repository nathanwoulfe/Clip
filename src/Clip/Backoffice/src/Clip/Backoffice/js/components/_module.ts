import { OverviewController } from './overview.controller';

export const ComponentsModule = angular
    .module('clip.components', [])
    .controller(OverviewController.controllerName, OverviewController)
    .name;
import { OverviewController } from './overview.controller';
import { OverviewComponent } from './overview.component';
import { TypeLimitsTableComponent } from './type-limits-table.component';

export const ComponentsModule = angular
    .module('clip.components', [])
    .controller(OverviewController.controllerName, OverviewController)
    .component(OverviewComponent.name, OverviewComponent)
    .component(TypeLimitsTableComponent.name, TypeLimitsTableComponent)
    .name;
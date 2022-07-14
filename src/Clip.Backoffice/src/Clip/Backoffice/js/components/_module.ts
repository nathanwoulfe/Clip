import { PackageViewController } from './package-view.controller';

export const ComponentsModule = angular
    .module('clip.components', [])
    .controller(PackageViewController.controllerName, PackageViewController)
    .name;
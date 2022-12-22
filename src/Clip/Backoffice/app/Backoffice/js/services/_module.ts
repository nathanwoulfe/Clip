import { ClipService } from './service';

export const ServicesModule = angular
    .module('clip.services', [])
    .factory(ClipService.serviceName, ClipService)
    .name;
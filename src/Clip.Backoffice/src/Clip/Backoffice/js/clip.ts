import { ComponentsModule } from './components/_module';
import { ServicesModule } from './services/_module';

const name = 'clip';

angular.module(name, [
    ServicesModule,
    ComponentsModule,
]);

angular.module('umbraco').requires.push(name);

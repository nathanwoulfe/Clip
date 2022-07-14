import { ComponentsModule } from './components/_module';
import { ServicesModule } from './services/_module';
import { ClipInterceptor } from './services/interceptor';

const name = 'clip';

angular.module(name, [
    ServicesModule,
    ComponentsModule,
])
    .config(['$httpProvider', $httpProvider => {
        $httpProvider.interceptors.push(ClipInterceptor);
    }]);

angular.module('umbraco').requires.push(name);

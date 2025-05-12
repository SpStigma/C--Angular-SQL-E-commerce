import { enableProdMode, importProvidersFrom } from '@angular/core';
import { bootstrapApplication }                from '@angular/platform-browser';
import { provideRouter }                       from '@angular/router';
import {
  provideHttpClient,
  withInterceptorsFromDi
} from '@angular/common/http';
import { HTTP_INTERCEPTORS }                   from '@angular/common/http';

import { AppComponent }                        from './app/app.component';
import { routes }                           from './app/app.routes';
import { AuthInterceptor }                     from './app/interceptors/auth.interceptor';
import { environment }                         from './environments/environment';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),

    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ]
})
.catch(err => console.error(err));

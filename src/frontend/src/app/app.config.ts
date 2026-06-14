import { ApplicationConfig, APP_INITIALIZER, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { AuthService } from './services/auth.service';
import { authInterceptor } from './interceptors/auth.interceptor';
import { errorInterceptor } from './interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    // APP_INITIALIZER: restore session from HttpOnly cookie BEFORE any route guard runs.
    // Without this, AuthorizeGuard fires before JWT is restored → false redirect to /login.
    {
      provide: APP_INITIALIZER,
      useFactory: (auth: AuthService) => () => auth.initializeAuth(),
      deps: [AuthService],
      multi: true
    }
  ]
};

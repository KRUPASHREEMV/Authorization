import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { toObservable } from '@angular/core/rxjs-interop';
import { filter, take, map } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authorizeGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Suspend via Observable until APP_INITIALIZER sets isInitialized = true.
  // Eliminates the race condition where the guard fires before session is restored.
  return toObservable(authService.isInitialized).pipe(
    filter(initialized => initialized === true),
    take(1),
    map(() => {
      if (authService.isLoggedIn()) {
        return true;
      }
      return router.createUrlTree(['/login']);
    })
  );
};

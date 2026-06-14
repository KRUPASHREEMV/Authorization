import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Don't retry refresh endpoint itself (would cause infinite loop)
      if (error.status === 401 && !req.url.includes('/refresh')) {
        return authService.refreshAccessToken().pipe(
          switchMap(response => {
            // Retry original request with new token
            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${response.token}` }
            });
            return next(retryReq);
          }),
          catchError(refreshError => {
            // Refresh also failed — log out
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};

import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.getAccessToken();
  const apiUrl = environment.apiUrl;

  if (!req.url.startsWith(apiUrl)) return next(req);

  let reqWithAuth = req;
  if (token) {
    reqWithAuth = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
        'X-Tenant-Code': environment.defaultTenantCode
      }
    });
  }

  return next(reqWithAuth).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        return auth.refresh().pipe(
          switchMap((res) => {
            if (res?.success && res.data) {
              const newReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${res.data!.accessToken}`,
                  'X-Tenant-Code': environment.defaultTenantCode
                }
              });
              return next(newReq);
            }
            router.navigate(['/login']);
            return throwError(() => err);
          }),
          catchError(() => {
            router.navigate(['/login']);
            return throwError(() => err);
          })
        );
      }
      return throwError(() => err);
    })
  );
};

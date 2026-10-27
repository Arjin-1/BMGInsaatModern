import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

export const adminAuthGuard: CanActivateFn = () => {
  const http = inject(HttpClient);
  const router = inject(Router);

  return http
    .get<{ username: string; authenticated: boolean }>(
      '/api/auth/me',
      { withCredentials: true }
    )
    .pipe(
      map((response) =>
        response.authenticated
          ? true
          : router.createUrlTree(['/admin/login'])
      ),
      catchError(() =>
        of(router.createUrlTree(['/admin/login']))
      )
    );
};

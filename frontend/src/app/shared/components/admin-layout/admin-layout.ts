import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  loggingOut = false;
  logoutError = '';

  logout(): void {
    if (this.loggingOut) {
      return;
    }

    this.loggingOut = true;
    this.logoutError = '';

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post(
            '/api/auth/logout',
            {},
            {
              withCredentials: true,
              headers: {
                'X-CSRF-TOKEN': csrfToken,
              },
            }
          )
        )
      )
      .subscribe({
        next: () => {
          this.loggingOut = false;
          void this.router.navigateByUrl('/admin/login');
          this.cdr.markForCheck();
        },
        error: () => {
          this.loggingOut = false;
          this.logoutError =
            'Çıkış yapılamadı. Lütfen tekrar deneyin.';
          this.cdr.markForCheck();
        },
      });
  }
}
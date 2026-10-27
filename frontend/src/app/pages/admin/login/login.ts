import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { switchMap } from 'rxjs';

@Component({
  imports: [FormsModule],
  selector: 'app-login',
  styleUrl: './login.scss',
  templateUrl: './login.html',
})
export class Login {
   private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  username = '';
  password = '';
  loading = false;
  errorMessage = '';

  onSubmit(): void {
    if (this.loading || !this.username.trim() || !this.password) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post(
            '/api/auth/login',
            {
              username: this.username.trim(),
              password: this.password,
            },
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
          this.password = '';
          this.loading = false;
          void this.router.navigateByUrl('/admin/projects');
        },
        error: () => {
          this.errorMessage =
            'Giriş başarısız. Kullanıcı bilgilerinizi veya sunucu bağlantısını kontrol edin.';
          this.loading = false;
        },
      });
  }
}

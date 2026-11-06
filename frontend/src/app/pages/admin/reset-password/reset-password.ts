import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [FormsModule],
  template: `
    <main class="admin-login">
      <section class="login-card">
        <div class="login-heading">
          <span class="login-eyebrow">BMG İNŞAAT</span>
          <h1>Yeni Şifre Belirle</h1>
          <p>Yönetici hesabınız için yeni bir şifre oluşturun.</p>
        </div>

        @if (success) {
          <p role="status">
            Şifreniz başarıyla değiştirildi. Artık yeni şifrenizle giriş yapabilirsiniz.
          </p>
        } @else if (!email || !token) {
          <p class="login-error" role="alert">
            Şifre sıfırlama bağlantısı geçersiz. Lütfen yeni bir bağlantı isteyin.
          </p>
        } @else {
          <form class="login-form" (ngSubmit)="onSubmit()">
            <div class="form-field">
              <label for="newPassword">Yeni Şifre</label>
              <input
                id="newPassword"
                name="newPassword"
                type="password"
                [(ngModel)]="newPassword"
                placeholder="Yeni şifrenizi girin"
                autocomplete="new-password"
                minlength="12"
                maxlength="128"
                required
              />
            </div>

            <p>
              Şifreniz en az 12 karakter, büyük harf, küçük harf, rakam ve özel karakter
              içermelidir.
            </p>

            <div class="form-field">
              <label for="confirmPassword">Yeni Şifre Tekrar</label>
              <input
                id="confirmPassword"
                name="confirmPassword"
                type="password"
                [(ngModel)]="confirmPassword"
                placeholder="Yeni şifrenizi tekrar girin"
                autocomplete="new-password"
                minlength="12"
                maxlength="128"
                required
              />
            </div>

            <button
              type="submit"
              class="login-button"
              [disabled]="loading || newPassword.length < 12 || newPassword !== confirmPassword"
            >
              {{ loading ? 'Kaydediliyor...' : 'Şifremi Değiştir' }}
            </button>

            @if (errorMessage) {
              <p class="login-error" role="alert">
                {{ errorMessage }}
              </p>
            }
          </form>
        }

        <p class="login-footer">
          <a href="/admin/login">Giriş Ekranına Dön</a>
        </p>
      </section>
    </main>
  `,
  styleUrl: '../login/login.scss',
})
export class ResetPassword {
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  email = '';
  token = '';

  newPassword = '';
  confirmPassword = '';
  loading = false;
  success = false;
  errorMessage = '';

  constructor() {
    if (typeof window !== 'undefined') {
      // URL bilgilerini silmeden once oku
      const params = new URLSearchParams(window.location.search);

      this.email = params.get('email') ?? '';
      this.token = params.get('token') ?? '';

      // Tokeni adres cubugundan kaldir
      window.history.replaceState(window.history.state, '', window.location.pathname);
    }
  }

  onSubmit(): void {
    if (
      this.loading ||
      !this.email ||
      !this.token ||
      this.newPassword.length < 12 ||
      this.newPassword !== this.confirmPassword
    ) {
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
            '/api/auth/reset-password',
            {
              email: this.email,
              token: this.token,
              newPassword: this.newPassword,
              confirmPassword: this.confirmPassword,
            },
            {
              withCredentials: true,
              headers: {
                'X-CSRF-TOKEN': csrfToken,
              },
            },
          ),
        ),
      )

      .subscribe({
        next: () => {
          this.loading = false;
          this.success = true;

          this.newPassword = '';
          this.confirmPassword = '';
          this.token = '';

          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          this.loading = false;

          this.errorMessage =
            error.status === 429
              ? 'Çok fazla deneme yapıldı. 15 dakika sonra tekrar deneyin.'
              : 'Şifre değiştirilemedi. Bağlantı geçersiz veya süresi dolmuş olabilir. Yeni bir bağlantı isteyin.';

          this.cdr.markForCheck();
        },
      });
  }
}

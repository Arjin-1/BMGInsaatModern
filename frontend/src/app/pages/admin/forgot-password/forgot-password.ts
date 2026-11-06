import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [FormsModule],
  template: `
    <main class="admin-login">
      <section class="login-card">
        <div class="login-heading">
          <span class="login-eyebrow">BMG İNŞAAT</span>
          <h1>Şifremi Unuttum</h1>
          <p>
            Yönetici hesabınıza ait e-posta adresini girin. Şifre sıfırlama bağlantısı e-postanıza
            gönderilecektir.
          </p>
        </div>

        @if (!submitted) {
          <form class="login-form" (ngSubmit)="onSubmit()">
            <div class="form-field">
              <label for="email">E-posta Adresi</label>
              <input
                id="email"
                name="email"
                type="email"
                [(ngModel)]="email"
                placeholder="E-posta adresinizi girin"
                autocomplete="email"
                maxlength="256"
                required
              />
            </div>

            <button type="submit" class="login-button" [disabled]="loading || !email.trim()">
              {{ loading ? 'Gönderiliyor...' : 'Sıfırlama Bağlantısı Gönder' }}
            </button>

            @if (errorMessage) {
              <p class="login-error" role="alert">
                {{ errorMessage }}
              </p>
            }
          </form>
        } @else {
          <p role="status">
            E-posta adresinize ait uygun bir hesap varsa, şifre sıfırlama bağlantısı
            gönderilecektir. Gelen kutunuzu kontrol edin.
          </p>
        }

        <p class="login-footer">
          <a href="/admin/login">Giriş Ekranına Dön</a>
        </p>
      </section>
    </main>
  `,
  styleUrl: '../login/login.scss',
})
export class ForgotPassword {
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  email = '';
  loading = false;
  submitted = false;
  errorMessage = '';

  onSubmit(): void {
    if (this.loading || !this.email.trim()) {
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
            '/api/auth/forgot-password',
            { email: this.email.trim() },
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
          this.submitted = true;
          this.email = '';

          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          this.loading = false;

          this.errorMessage =
            error.status === 429
              ? 'Çok fazla istek gönderildi. 15 dakika sonra tekrar deneyin.'
              : 'İşlem gerçekleştirilemedi. Lütfen daha sonra tekrar deneyin.';

          this.cdr.markForCheck();
        },
      });
  }
}

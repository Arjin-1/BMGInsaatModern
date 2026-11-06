import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-confirm-email',
  imports: [],
  template: `
    <main class="confirm-page">
      <section class="confirm-card">
        <span class="eyebrow">BMG İNŞAAT</span>

        <h1>E-posta Doğrulama</h1>

        @if (!validLink) {
          <p class="error" role="alert">
            Doğrulama bağlantısı geçersiz.
            Lütfen yeni bir bağlantı isteyin.
          </p>
        } @else if (confirmed()) {
          <p class="success" role="status">
            E-posta adresiniz başarıyla doğrulandı.
          </p>

          <p>
            Artık yönetim paneline e-posta adresiniz
            ve şifrenizle giriş yapabilirsiniz.
          </p>
        } @else {
          <p>
            Yönetici hesabınızın e-posta adresini
            doğrulamak için aşağıdaki butona basın.
          </p>

          @if (errorMessage()) {
            <p class="error" role="alert">
              {{ errorMessage() }}
            </p>
          }

          <button
            type="button"
            [disabled]="submitting()"
            (click)="confirmEmail()"
          >
            {{
              submitting()
                ? 'Doğrulanıyor...'
                : 'E-postamı Doğrula'
            }}
          </button>
        }
      </section>
    </main>
  `,
  styles: [`
    :host {
      display: block;
    }

    .confirm-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 32px 16px;
      background: #f5f6f7;
    }

    .confirm-card {
      box-sizing: border-box;
      width: 100%;
      max-width: 480px;
      padding: 42px 32px;
      border: 1px solid #e7e9eb;
      border-top: 4px solid #c4a052;
      border-radius: 12px;
      background: white;
      text-align: center;
      box-shadow: 0 15px 40px rgba(0, 0, 0, 0.06);
    }

    .eyebrow {
      color: #ab8540;
      font-size: 12px;
      font-weight: 700;
      letter-spacing: 2px;
    }

    h1 {
      margin: 20px 0;
      color: #243746;
      font-size: 28px;
    }

    p {
      color: #65717b;
      line-height: 1.8;
    }

    button {
      width: 100%;
      margin-top: 20px;
      padding: 15px;
      border: 0;
      border-radius: 8px;
      background: #243746;
      color: white;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    button:focus-visible {
      outline: 3px solid #c4a052;
      outline-offset: 3px;
    }

    .success {
      color: #217346;
      font-weight: 700;
    }

    .error {
      color: #b42318;
      font-weight: 600;
    }

    @media (max-width: 480px) {
      .confirm-card {
        padding: 32px 20px;
      }
    }
  `]
})
export class ConfirmEmail {
  private readonly http = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);

  private readonly userId =
    this.route.snapshot.queryParamMap.get('userId') ?? '';

  private readonly token =
    this.route.snapshot.queryParamMap.get('token') ?? '';

  readonly validLink = Boolean(this.userId && this.token);

  readonly submitting = signal(false);
  readonly confirmed = signal(false);
  readonly errorMessage = signal('');

  constructor() {
    // Tokenin adres cubugunda kalmasini engelle.
    if (this.validLink) {
      window.history.replaceState(
        window.history.state,
        '',
        window.location.pathname
      );
    }
  }

  confirmEmail(): void {
    if (!this.validLink || this.submitting() || this.confirmed()) {
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set('');

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post(
            '/api/auth/confirm-email',
            {
              userId: this.userId,
              token: this.token,
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
          this.submitting.set(false);
          this.confirmed.set(true);
        },
        error: () => {
          this.submitting.set(false);
          this.errorMessage.set(
            'E-posta doğrulanamadı. Bağlantı geçersiz veya süresi dolmuş olabilir.'
          );
        },
      });
  }
}


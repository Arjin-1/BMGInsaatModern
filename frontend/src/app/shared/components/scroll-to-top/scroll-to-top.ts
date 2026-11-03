import {
  Component,
  HostListener,
  inject,
  signal
} from '@angular/core';

import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-scroll-to-top',
  standalone: true,
  template: `
    @if (isVisible()) {
      <button
        type="button"
        class="scroll-to-top"
        [attr.aria-label]="
          language() === 'tr'
            ? 'Sayfanın en üstüne çık'
            : 'Back to top'
        "
        [attr.title]="
          language() === 'tr'
            ? 'Yukarı Çık'
            : 'Back to Top'
        "
        (click)="scrollToTop()">
        <span aria-hidden="true">↑</span>
      </button>
    }
  `,
  styles: [`
    .scroll-to-top {
      position: fixed;
      right: 25px;
      bottom: 25px;
      z-index: 1000;

      display: flex;
      align-items: center;
      justify-content: center;

      width: 48px;
      height: 48px;

      background: #202a35;
      color: #c4a052;

      border: 2px solid #c4a052;
      border-radius: 50%;

      box-shadow: 0 5px 20px rgba(0, 0, 0, 0.2);

      font-size: 26px;
      font-weight: 700;

      cursor: pointer;

      transition:
        background 0.2s ease,
        color 0.2s ease,
        transform 0.2s ease;
    }

    .scroll-to-top:hover {
      background: #c4a052;
      color: #202a35;
      transform: translateY(-4px);
    }

    .scroll-to-top:focus-visible {
      outline: 3px solid #c4a052;
      outline-offset: 3px;
    }

    @media (max-width: 600px) {
      .scroll-to-top {
        right: 16px;
        bottom: 16px;
        width: 44px;
        height: 44px;
      }
    }

    @media (prefers-reduced-motion: reduce) {
      .scroll-to-top {
        transition: none;
      }
    }
  `]
})
export class ScrollToTop {
  private readonly languageService = inject(LanguageService);

  readonly language = this.languageService.language;

  readonly isVisible = signal(false);

  // Sayfa 350 piksel kaydirilinca butonu goster
  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.isVisible.set(window.scrollY > 350);
  }

  // Sayfanin en ustune cik
  scrollToTop(): void {
    const reduceMotion = window.matchMedia(
      '(prefers-reduced-motion: reduce)'
    ).matches;

    window.scrollTo({
      top: 0,
      behavior: reduceMotion ? 'instant' : 'smooth'
    });
  }
}


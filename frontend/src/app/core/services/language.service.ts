import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type SiteLanguage = 'tr' | 'en';

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private readonly platformId = inject(PLATFORM_ID);

  private readonly currentLanguage = signal<SiteLanguage>('tr');

  readonly language = this.currentLanguage.asReadonly();

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      try {
        const savedLanguage = localStorage.getItem('bmg-language');

        if (savedLanguage === 'tr' || savedLanguage === 'en') {
          this.currentLanguage.set(savedLanguage);
        }
      } catch {
        // Tarayıcı depolaması kapalıysa varsayılan dil Türkçe kalır.
      }
    }
  }

  setLanguage(language: SiteLanguage): void {
    this.currentLanguage.set(language);

    if (isPlatformBrowser(this.platformId)) {
      try {
        localStorage.setItem('bmg-language', language);
      } catch {
        // Depolama kullanılamasa da dil değişikliği çalışır.
      }
    }
  }
}

import { Component, inject, signal } from '@angular/core';
import {
  LanguageService,
  SiteLanguage,
} from '../../../core/services/language.service';

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  private readonly languageService = inject(LanguageService);

  // Aktif site dili
  readonly language = this.languageService.language;

  // Mobil menü durumu
  readonly isMenuOpen = signal(false);

  // Dil değiştirme
  changeLanguage(language: SiteLanguage): void {
    this.languageService.setLanguage(language);
    this.closeMenu();
  }

  // Mobil menüyü aç / kapat
  toggleMenu(): void {
    this.isMenuOpen.update((isOpen) => !isOpen);
  }

  // Mobil menüyü kapat
  closeMenu(): void {
    this.isMenuOpen.set(false);
  }
}

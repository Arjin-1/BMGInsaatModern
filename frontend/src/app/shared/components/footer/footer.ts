import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.scss',
})
export class Footer {
  private readonly languageService = inject(LanguageService);

  // Header ile aynı dil sistemini kullanır.
  readonly language = this.languageService.language;

  // Telif hakkı yılını otomatik günceller.
  readonly currentYear = new Date().getFullYear();
}

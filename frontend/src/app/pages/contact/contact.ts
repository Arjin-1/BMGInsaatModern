
import { Component, inject } from '@angular/core';
import { LanguageService } from '../../core/services/language.service';

@Component({
  selector: 'app-contact',
  imports: [],
  templateUrl: './contact.html',
  styleUrl: './contact.scss',
})
export class Contact {

  private readonly languageService = inject(LanguageService);

  // Header'dan secilen aktif dil
  readonly language = this.languageService.language;

}


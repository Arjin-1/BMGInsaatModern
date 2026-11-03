import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject,
} from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { LanguageService } from '../../core/services/language.service';
import { RouterLink } from '@angular/router';

// Public tarafta goruntulenecek hizmet bilgileri
interface ConstructionService {
  id: number;

  titleTr: string;
  titleEn: string;

  descriptionTr: string;
  descriptionEn: string;

  imageUrl: string;

  displayOrder: number;
}

@Component({
  selector: 'app-services',
  imports: [RouterLink],
  templateUrl: './services.html',
  styleUrl: './services.scss',
})
export class Services implements OnInit {

  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly languageService = inject(LanguageService);

  readonly language = this.languageService.language;

  // Yayinlanmis hizmetler
  services: ConstructionService[] = [];

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadServices();
  }

  // Public API'den yayinlanmis hizmetleri getir
  loadServices(): void {
    this.loading = true;
    this.errorMessage = '';

    this.http
      .get<ConstructionService[]>('/api/services')
      .subscribe({
        next: (services) => {
          this.services = services;
          this.loading = false;

          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Hizmetler yüklenemedi:', error);

          this.loading = false;
          this.errorMessage =
            'Hizmetler şu anda görüntülenemiyor. Lütfen tekrar deneyin.';

          this.cdr.markForCheck();
        },
      });
  }

  // Backend'deki hizmet gorselinin adresini olustur
  getImageSrc(imageUrl: string): string {
    if (/^https?:\/\//i.test(imageUrl)) {
      return imageUrl;
    }

    return `http://localhost:5056/${imageUrl.replace(/^\/+/, '')}`;
  }
}


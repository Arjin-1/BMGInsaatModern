import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject,
} from '@angular/core';

import { HttpClient } from '@angular/common/http';

// Veritabanindan gelen hizmet bilgileri
interface ConstructionService {
  id: number;

  titleTr: string;
  titleEn: string;

  descriptionTr: string;
  descriptionEn: string;

  imageUrl: string;

  isPublished: boolean;
  displayOrder: number;

  createdAtUtc: string;
  updatedAtUtc: string | null;
}

@Component({
  selector: 'app-admin-services',
  imports: [],
  templateUrl: './services.html',
  styleUrl: './services.scss',
})
export class Services implements OnInit {

  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  // Hizmet listesi
  services: ConstructionService[] = [];

  // Yuklenme ve hata durumlari
  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadServices();
  }

  // Yonetici panelinde tum hizmetleri listele
  loadServices(): void {
    this.loading = true;
    this.errorMessage = '';

    this.http
      .get<ConstructionService[]>('/api/admin/services', {
        withCredentials: true,
      })
      .subscribe({
        next: (services) => {
          this.services = services;

          this.loading = false;
          this.errorMessage = '';

          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error(
            'Hizmetler yuklenemedi:',
            error
          );

          this.loading = false;

          this.errorMessage =
            'Hizmetler yüklenemedi. Lütfen tekrar deneyin.';

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

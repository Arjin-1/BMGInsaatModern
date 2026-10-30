import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { LanguageService } from '../../core/services/language.service';

interface PublicCertificate {
  id: number;
  nameTr: string;
  nameEn: string | null;
  imageUrl: string | null;
  displayOrder: number;
}

@Component({
  selector: 'app-certificates',
  imports: [RouterLink],
  templateUrl: './certificates.html',
  styleUrl: './certificates.scss',
})
export class Certificates implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly languageService = inject(LanguageService);

  readonly language = this.languageService.language;

  readonly certificates = signal<PublicCertificate[]>([]);
  readonly loading = signal(true);
  readonly error = signal(false);

  ngOnInit(): void {
    this.loadCertificates();
  }

  loadCertificates(): void {
    this.loading.set(true);
    this.error.set(false);

    this.http.get<PublicCertificate[]>('/api/certificates').subscribe({
      next: (data) => {
        this.certificates.set(
          [...data].sort((a, b) => a.displayOrder - b.displayOrder)
        );
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Sertifikalar yüklenemedi:', err);
        this.error.set(true);
        this.loading.set(false);
      },
    });
  }

  getImageSrc(imageUrl: string): string {
    if (/^https?:\/\//i.test(imageUrl)) {
      return imageUrl;
    }

    // Mevcut geliştirme ortamındaki backend adresi.
    // Production aşamasında merkezi API yapılandırmasına taşınacak.
    return `http://localhost:5056/${imageUrl.replace(/^\/+/, '')}`;
  }
}

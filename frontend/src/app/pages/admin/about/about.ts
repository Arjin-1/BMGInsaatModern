import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { switchMap } from 'rxjs';

interface AboutContent {
  aboutTr: string;
  aboutEn: string;
  missionTr: string;
  missionEn: string;
  visionTr: string;
  visionEn: string;
}

@Component({
  selector: 'app-admin-about',
  imports: [FormsModule],
  templateUrl: './about.html',
  styleUrl: './about.scss',
})
export class About implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  content: AboutContent = {
    aboutTr: '',
    aboutEn: '',
    missionTr: '',
    missionEn: '',
    visionTr: '',
    visionEn: '',
  };

  private originalContent: AboutContent | null = null;
  loading = true;
  errorMessage = '';
  saving = false;
  saveError = '';
  saveSuccess = '';

  ngOnInit(): void {
    this.http.get<AboutContent>('/api/about').subscribe({
      next: (data) => {
        this.content = { ...data };
        this.originalContent = { ...data };
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;

        if (error.status !== 404) {
          this.errorMessage = 'Hakkımızda bilgileri yüklenemedi. Sunucu bağlantısını kontrol edin.';
        }

        // 404 ise henüz kayıt yoktur; boş form kullanılabilir.
        this.cdr.markForCheck();
      },
    });
  }

  saveAbout(): void {
    if (this.saving || this.loading) {
      return;
    }

    const aboutTr = this.content.aboutTr.trim();
    const missionTr = this.content.missionTr.trim();
    const visionTr = this.content.visionTr.trim();

    this.saveError = '';
    this.saveSuccess = '';

    if (!aboutTr || !missionTr || !visionTr) {
      this.saveError = 'Şirket tanıtımı, misyon ve vizyon alanları zorunludur.';
      return;
    }

    const original = this.originalContent;

    // Türkçe metin değişmediyse mevcut İngilizceyi koru.
    // Değiştiyse İngilizceyi boş gönder:
    // Backend Azure ile yeniden çevirecek.
    const body = {
      aboutTr,
      missionTr,
      visionTr,

      aboutEn: original && aboutTr === original.aboutTr ? original.aboutEn : '',

      missionEn: original && missionTr === original.missionTr ? original.missionEn : '',

      visionEn: original && visionTr === original.visionTr ? original.visionEn : '',
    };

    this.saving = true;

    let updateCompleted = false;

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put('/api/admin/about', body, {
            withCredentials: true,
            headers: {
              'X-CSRF-TOKEN': csrfToken,
            },
          }),
        ),
        // Kaydedilen İngilizce çevirileri tekrar getir.
        switchMap(() => {
          updateCompleted = true;

          return this.http.get<AboutContent>('/api/about');
        }),
      )
      .subscribe({
        next: (updatedContent) => {
          this.content = { ...updatedContent };
          this.originalContent = { ...updatedContent };

          this.saving = false;
          this.saveSuccess = 'Hakkımızda bilgileri ve İngilizce içerikleri başarıyla kaydedildi.';

          this.cdr.markForCheck();
        },

        error: () => {
          this.saving = false;

          if (updateCompleted) {
            this.saveSuccess =
              'Bilgiler kaydedildi ancak güncel çeviriler yeniden okunamadı. Sayfayı yenileyin.';
          } else {
            this.saveError =
              'Bilgiler kaydedilemedi. Sunucu bağlantısını ve çeviri servisini kontrol edin.';
          }

          this.cdr.markForCheck();
        },
      });
  }

}

import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { switchMap } from 'rxjs';
import { toast } from 'ngx-sonner';

// PostgreSQL'den gelen proje videosu bilgileri
interface ProjectVideo {
  id: number;

  titleTr: string;
  titleEn: string;

  descriptionTr: string;
  descriptionEn: string;

  videoUrl: string;

  isPublished: boolean;
  displayOrder: number;

  createdAtUtc: string;
  updatedAtUtc: string | null;
}

@Component({
  selector: 'app-admin-project-videos',
  templateUrl: './project-videos.html',
  styleUrl: './project-videos.scss',
})
export class ProjectVideos implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  // Proje videolari
  videos: ProjectVideo[] = [];

  // Yuklenme ve hata durumlari
  loading = true;
  errorMessage = '';

  // Yeni proje videosu olusturma formu
  titleTr = '';
  descriptionTr = '';
  displayOrder = 0;

  // Kaydetme durumu
  saving = false;

  // Video yukleme durumu
  uploadingVideoId: number | null = null;

  // Video yayinlama / yayindan kaldirma durumu
  publishingVideoId: number | null = null;

  // Video duzenleme durumu
  editingVideoId: number | null = null;

  editTitleTr = '';
  editDescriptionTr = '';
  editDisplayOrder = 0;

  updatingVideo = false;

  // Video silme durumlari
  deletingVideoId: number | null = null;

  // Silinmesi icin onay beklenen video
  pendingDeleteVideo: ProjectVideo | null = null;

  ngOnInit(): void {
    this.loadVideos();
  }

  // Admin panelinde tum proje videolarini listele
  loadVideos(): void {
    this.loading = true;
    this.errorMessage = '';

    this.http
      .get<ProjectVideo[]>('/api/admin/project-videos', {
        withCredentials: true,
      })
      .subscribe({
        next: (videos) => {
          this.videos = videos;
          this.loading = false;
          this.errorMessage = '';

          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Proje videolari yuklenemedi:', error);

          this.loading = false;
          this.errorMessage = 'Proje videoları yüklenemedi. Lütfen tekrar deneyin.';

          this.cdr.markForCheck();
        },
      });
  }

  // Yeni proje videosu kaydi olustur
  createVideo(): void {
    if (this.saving) {
      return;
    }

    const titleTr = this.titleTr.trim();
    const descriptionTr = this.descriptionTr.trim();
    const displayOrder = Number(this.displayOrder);

    // Zorunlu alan kontrolu
    if (!titleTr || !descriptionTr) {
      toast.error('Video başlığı ve açıklaması zorunludur.');
      return;
    }

    // Baslik uzunlugu kontrolu
    if (titleTr.length > 200) {
      toast.error('Video başlığı en fazla 200 karakter olabilir.');
      return;
    }

    // Goruntulenme sirasi kontrolu
    if (!Number.isInteger(displayOrder) || displayOrder < 0 || displayOrder > 2147483647) {
      toast.error('Görüntülenme sırası sıfır veya pozitif bir tam sayı olmalıdır.');
      return;
    }

    this.saving = true;

    // CSRF token al ve video kaydini olustur
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post<{ id: number; message: string }>(
            '/api/admin/project-videos',
            {
              titleTr,
              descriptionTr,
              displayOrder,
            },
            {
              withCredentials: true,
              headers: {
                'X-CSRF-TOKEN': csrfToken,
              },
            },
          ),
        ),
      )
      .subscribe({
        next: () => {
          this.saving = false;

          // Formu temizle
          this.titleTr = '';
          this.descriptionTr = '';
          this.displayOrder = 0;

          toast.success('Proje videosu oluşturuldu. Şimdi MP4 dosyasını yükleyebilirsiniz.');

          // Video listesini yenile
          this.loadVideos();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Proje videosu oluşturulamadı:', error);

          this.saving = false;

          toast.error('Proje videosu oluşturulamadı. Lütfen tekrar deneyin.');

          this.cdr.markForCheck();
        },
      });
  }

  // MP4 yukle veya mevcut videoyu degistir
  uploadVideo(videoId: number, input: HTMLInputElement): void {
    if (this.uploadingVideoId !== null) {
      return;
    }

    const file = input.files?.[0];

    if (!file) {
      return;
    }

    // Dosya uzantisi kontrolu
    if (!file.name.toLowerCase().endsWith('.mp4')) {
      toast.error('Yalnızca MP4 video dosyaları yükleyebilirsiniz.');
      input.value = '';
      return;
    }

    // Maksimum 200 MB
    if (file.size < 12 || file.size > 200_000_000) {
      toast.error('Video geçerli olmalı ve 200 MB sınırını aşmamalıdır.');
      input.value = '';
      return;
    }

    this.uploadingVideoId = videoId;

    const formData = new FormData();
    formData.append('file', file);

    // CSRF token al ve videoyu backend'e gonder
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post<{
            id: number;
            videoUrl: string;
            message: string;
          }>(`/api/admin/project-videos/${videoId}/video`, formData, {
            withCredentials: true,
            headers: {
              'X-CSRF-TOKEN': csrfToken,
            },
          }),
        ),
      )
      .subscribe({
        next: () => {
          this.uploadingVideoId = null;
          input.value = '';

          toast.success('Proje videosu başarıyla yüklendi.');

          // Video listesini yenile
          this.loadVideos();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Proje videosu yüklenemedi:', error);

          this.uploadingVideoId = null;
          input.value = '';

          toast.error('Video yüklenemedi. MP4 dosyasını kontrol edip tekrar deneyin.');

          this.cdr.markForCheck();
        },
      });
  }

  // Proje videosunu yayinla veya yayindan kaldir
  toggleVideoPublication(video: ProjectVideo): void {
    if (this.publishingVideoId !== null || this.uploadingVideoId !== null) {
      return;
    }

    // MP4 dosyasi olmayan video yayinlanamaz
    if (!video.isPublished && !video.videoUrl) {
      toast.error('Videoyu yayınlamak için önce MP4 dosyası yüklemelisiniz.');
      return;
    }

    const newPublishedStatus = !video.isPublished;

    this.publishingVideoId = video.id;

    // CSRF token al ve yayin durumunu guncelle
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put<void>(
            `/api/admin/project-videos/${video.id}`,
            {
              titleTr: video.titleTr,
              descriptionTr: video.descriptionTr,
              displayOrder: video.displayOrder,
              isPublished: newPublishedStatus,
            },
            {
              withCredentials: true,
              headers: {
                'X-CSRF-TOKEN': csrfToken,
              },
            },
          ),
        ),
      )
      .subscribe({
        next: () => {
          this.publishingVideoId = null;

          toast.success(
            newPublishedStatus
              ? 'Proje videosu başarıyla yayınlandı.'
              : 'Proje videosu yayından kaldırıldı.',
          );

          // Guncel listeyi veritabanindan getir
          this.loadVideos();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Video yayın durumu değiştirilemedi:', error);

          this.publishingVideoId = null;

          toast.error('Video yayın durumu değiştirilemedi. Lütfen tekrar deneyin.');

          this.cdr.markForCheck();
        },
      });
  }

  // Secilen videonun duzenleme formunu ac
  startEditingVideo(video: ProjectVideo): void {
    if (this.updatingVideo || this.uploadingVideoId !== null || this.publishingVideoId !== null) {
      return;
    }

    this.editingVideoId = video.id;

    // Mevcut bilgileri forma aktar
    this.editTitleTr = video.titleTr;
    this.editDescriptionTr = video.descriptionTr;
    this.editDisplayOrder = video.displayOrder;

    this.cdr.markForCheck();
  }

  // Video duzenleme islemini iptal et
  cancelEditingVideo(): void {
    if (this.updatingVideo) {
      return;
    }

    this.editingVideoId = null;

    this.editTitleTr = '';
    this.editDescriptionTr = '';
    this.editDisplayOrder = 0;

    this.cdr.markForCheck();
  }

  // Proje videosu bilgilerini guncelle
  updateVideo(video: ProjectVideo): void {
    if (
      this.editingVideoId !== video.id ||
      this.updatingVideo ||
      this.uploadingVideoId !== null ||
      this.publishingVideoId !== null
    ) {
      return;
    }

    const titleTr = this.editTitleTr.trim();
    const descriptionTr = this.editDescriptionTr.trim();
    const displayOrder = Number(this.editDisplayOrder);

    // Zorunlu alan kontrolu
    if (!titleTr || !descriptionTr) {
      toast.error('Video başlığı ve açıklaması zorunludur.');
      return;
    }

    // Baslik uzunlugu kontrolu
    if (titleTr.length > 200) {
      toast.error('Video başlığı en fazla 200 karakter olabilir.');
      return;
    }

    // Goruntulenme sirasi kontrolu
    if (!Number.isInteger(displayOrder) || displayOrder < 0 || displayOrder > 2147483647) {
      toast.error('Görüntülenme sırası sıfır veya pozitif bir tam sayı olmalıdır.');
      return;
    }

    this.updatingVideo = true;

    // CSRF token al ve video bilgilerini guncelle
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put<void>(
            `/api/admin/project-videos/${video.id}`,
            {
              titleTr,
              descriptionTr,
              displayOrder,
              isPublished: video.isPublished,
            },
            {
              withCredentials: true,
              headers: {
                'X-CSRF-TOKEN': csrfToken,
              },
            },
          ),
        ),
      )
      .subscribe({
        next: () => {
          this.updatingVideo = false;

          // Duzenleme formunu kapat
          this.cancelEditingVideo();

          toast.success('Proje videosu bilgileri başarıyla güncellendi.');

          // Guncel bilgileri PostgreSQL'den getir
          this.loadVideos();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Proje videosu güncellenemedi:', error);

          this.updatingVideo = false;

          toast.error('Proje videosu güncellenemedi. Lütfen tekrar deneyin.');

          this.cdr.markForCheck();
        },
      });
  }

  // Video silme onay penceresini ac
  deleteVideo(video: ProjectVideo): void {
    if (
      this.pendingDeleteVideo !== null ||
      this.deletingVideoId !== null ||
      this.updatingVideo ||
      this.uploadingVideoId !== null ||
      this.publishingVideoId !== null
    ) {
      return;
    }

    this.pendingDeleteVideo = video;
    this.cdr.markForCheck();
  }

  // Video silme islemini iptal et
  cancelDeleteVideo(): void {
    if (this.deletingVideoId !== null) {
      return;
    }

    this.pendingDeleteVideo = null;
    this.cdr.markForCheck();
  }

  
  // Onaylanan proje videosunu sil
  confirmDeleteVideo(): void {
    const video = this.pendingDeleteVideo;

    if (
      !video ||
      this.deletingVideoId !== null ||
      this.updatingVideo ||
      this.uploadingVideoId !== null ||
      this.publishingVideoId !== null
    ) {
      return;
    }

    this.deletingVideoId = video.id;

    // CSRF token al ve videoyu sil
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.delete<{
            id: number;
            message: string;
            warning?: boolean;
          }>(
            `/api/admin/project-videos/${video.id}`,
            {
              withCredentials: true,
              headers: {
                'X-CSRF-TOKEN': csrfToken
              }
            }
          )
        )
      )
      .subscribe({
        next: (response) => {
          this.deletingVideoId = null;
          this.pendingDeleteVideo = null;

          // Silinen video duzenleniyorsa formu kapat
          if (this.editingVideoId === video.id) {
            this.cancelEditingVideo();
          }

          if (response.warning) {
            toast.warning(
              'Video kaydı silindi ancak MP4 dosyası temizlenemedi.'
            );
          } else {
            toast.success(
              'Proje videosu başarıyla silindi.'
            );
          }

          // Video listesini yenile
          this.loadVideos();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error(
            'Proje videosu silinemedi:',
            error
          );

          this.deletingVideoId = null;

          toast.error(
            'Proje videosu silinemedi. Lütfen tekrar deneyin.'
          );

          this.cdr.markForCheck();
        }
      });
  }

}

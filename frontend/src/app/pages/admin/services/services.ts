import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { switchMap } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { toast } from 'ngx-sonner';

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
  imports: [FormsModule],
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

  // Yeni hizmet olusturma formu
  titleTr = '';

  descriptionTr = '';

  displayOrder = 0;

  saving = false;
  formError = '';

  // Hizmet gorseli yukleme durumu
  uploadingServiceId: number | null = null;

  // Hizmet yayinlama / yayindan kaldirma durumu
  publishingServiceId: number | null = null;

  // Hizmet duzenleme durumu
  editingServiceId: number | null = null;

  editTitleTr = '';
  editDescriptionTr = '';
  editDisplayOrder = 0;

  updatingService = false;

  // Hizmet silme durumlari
  deletingServiceId: number | null = null;

  pendingDeleteService: ConstructionService | null = null;

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
          console.error('Hizmetler yuklenemedi:', error);

          this.loading = false;

          this.errorMessage = 'Hizmetler yüklenemedi. Lütfen tekrar deneyin.';

          this.cdr.markForCheck();
        },
      });
  }

  // Backend'deki hizmet gorselinin adresini olustur
  getImageSrc(imageUrl: string): string {
    if (/^https?:\/\//i.test(imageUrl)) {
      return imageUrl;
    }

    
    return `/${imageUrl.replace(/^\/+/, '')}`;

  }

  // Yeni hizmet olustur
  createService(): void {
    if (this.saving) {
      return;
    }

    this.formError = '';

    const titleTr = this.titleTr.trim();
    const descriptionTr = this.descriptionTr.trim();
    const displayOrder = Number(this.displayOrder);

    // Zorunlu alan kontrolu
    if (!titleTr || !descriptionTr) {
      this.formError = 'Hizmet başlığı ve açıklaması zorunludur.';
      return;
    }

    // Baslik karakter siniri
    if (titleTr.length > 200) {
      this.formError = 'Hizmet başlığı en fazla 200 karakter olabilir.';
      return;
    }

    // Goruntulenme sirasi kontrolu
    if (!Number.isInteger(displayOrder) || displayOrder < 0 || displayOrder > 2147483647) {
      this.formError = 'Görüntülenme sırası sıfır veya pozitif bir tam sayı olmalıdır.';
      return;
    }

    this.saving = true;

    // CSRF token al ve hizmet olustur
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post<{ id: number; message: string }>(
            '/api/admin/services',
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

          this.formError = '';
          toast.success('Hizmet başarıyla oluşturuldu. Şimdi görsel yükleyebilirsiniz.');

          // Hizmet listesini yenile
          this.loadServices();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Hizmet oluşturulamadı:', error);

          this.saving = false;
          this.formError = 'Hizmet oluşturulamadı. Lütfen tekrar deneyin.';

          this.cdr.markForCheck();
        },
      });
  }

  // Hizmete gorsel yukle veya mevcut gorseli degistir
  uploadServiceImage(serviceId: number, input: HTMLInputElement): void {
    if (this.uploadingServiceId !== null) {
      return;
    }

    const file = input.files?.[0];

    if (!file) {
      return;
    }

    // Desteklenen dosya uzantilari
    const allowedExtensions = ['jpg', 'jpeg', 'png', 'webp'];
    const extension = file.name.split('.').pop()?.toLowerCase();

    if (!extension || !allowedExtensions.includes(extension)) {
      toast.error('Yalnızca JPG, PNG ve WebP görselleri yükleyebilirsiniz.');

      input.value = '';
      return;
    }

    // Dosya boyutu kontrolu
    if (file.size === 0 || file.size > 15_000_000) {
      toast.error('Görsel boş olamaz ve 15 MB sınırını aşamaz.');

      input.value = '';
      return;
    }

    this.uploadingServiceId = serviceId;

    const formData = new FormData();
    formData.append('file', file);

    // CSRF token al ve gorseli backend'e gonder
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post<{
            id: number;
            imageUrl: string;
            message: string;
          }>(`/api/admin/services/${serviceId}/image`, formData, {
            withCredentials: true,
            headers: {
              'X-CSRF-TOKEN': csrfToken,
            },
          }),
        ),
      )
      .subscribe({
        next: () => {
          this.uploadingServiceId = null;

          toast.success('Hizmet görseli başarıyla kaydedildi.');

          input.value = '';

          // Hizmet listesini ve gorselleri yenile
          this.loadServices();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Hizmet görseli yüklenemedi:', error);

          this.uploadingServiceId = null;
          toast.error('Görsel yüklenemedi. Dosyayı kontrol edip tekrar deneyin.');

          input.value = '';
          this.cdr.markForCheck();
        },
      });
  }

  // Hizmeti yayinla veya yayindan kaldir
  toggleServicePublication(service: ConstructionService): void {
    if (this.publishingServiceId !== null) {
      return;
    }

    // Gorseli olmayan hizmet yayinlanamaz
    if (!service.isPublished && !service.imageUrl) {
      toast.error('Hizmeti yayınlamak için önce görsel yüklemelisiniz.');
      return;
    }

    const newPublishedStatus = !service.isPublished;

    this.publishingServiceId = service.id;

    // CSRF token al ve hizmetin yayin durumunu guncelle
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put<void>(
            `/api/admin/services/${service.id}`,
            {
              titleTr: service.titleTr,
              descriptionTr: service.descriptionTr,
              displayOrder: service.displayOrder,
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
          this.publishingServiceId = null;

          toast.success(
            newPublishedStatus ? 'Hizmet başarıyla yayınlandı.' : 'Hizmet yayından kaldırıldı.',
          );

          this.loadServices();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Hizmet yayın durumu değiştirilemedi:', error);

          this.publishingServiceId = null;

          toast.error('Hizmetin yayın durumu değiştirilemedi. Lütfen tekrar deneyin.');

          this.cdr.markForCheck();
        },
      });
  }

  // Secilen hizmetin duzenleme formunu ac
  startEditingService(service: ConstructionService): void {
    if (
      this.updatingService ||
      this.uploadingServiceId !== null ||
      this.publishingServiceId !== null
    ) {
      return;
    }

    this.editingServiceId = service.id;

    // Mevcut bilgileri forma aktar
    this.editTitleTr = service.titleTr;
    this.editDescriptionTr = service.descriptionTr;
    this.editDisplayOrder = service.displayOrder;

    this.cdr.markForCheck();
  }

  // Hizmet duzenleme islemini iptal et
  cancelEditingService(): void {
    if (this.updatingService) {
      return;
    }

    this.editingServiceId = null;

    this.editTitleTr = '';
    this.editDescriptionTr = '';
    this.editDisplayOrder = 0;

    this.cdr.markForCheck();
  }

  // Hizmet bilgilerini guncelle
  updateService(service: ConstructionService): void {
    if (
      this.editingServiceId !== service.id ||
      this.updatingService ||
      this.uploadingServiceId !== null ||
      this.publishingServiceId !== null
    ) {
      return;
    }

    const titleTr = this.editTitleTr.trim();
    const descriptionTr = this.editDescriptionTr.trim();
    const displayOrder = Number(this.editDisplayOrder);

    // Zorunlu alan kontrolu
    if (!titleTr || !descriptionTr) {
      toast.error('Hizmet başlığı ve açıklaması zorunludur.');
      return;
    }

    // Baslik uzunlugu kontrolu
    if (titleTr.length > 200) {
      toast.error('Hizmet başlığı en fazla 200 karakter olabilir.');
      return;
    }

    // Sira numarasi kontrolu
    if (!Number.isInteger(displayOrder) || displayOrder < 0 || displayOrder > 2147483647) {
      toast.error('Görüntülenme sırası geçerli bir pozitif tam sayı veya sıfır olmalıdır.');
      return;
    }

    this.updatingService = true;

    // CSRF token al ve hizmet bilgilerini guncelle
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put<void>(
            `/api/admin/services/${service.id}`,
            {
              titleTr,
              descriptionTr,
              displayOrder,
              isPublished: service.isPublished,
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
          this.updatingService = false;

          // Duzenleme formunu kapat
          this.cancelEditingService();

          toast.success('Hizmet bilgileri başarıyla güncellendi.');

          // Guncel bilgileri veritabanindan getir
          this.loadServices();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Hizmet güncellenemedi:', error);

          this.updatingService = false;

          toast.error('Hizmet güncellenemedi. Lütfen tekrar deneyin.');

          this.cdr.markForCheck();
        },
      });
  }

  
  // Hizmet silme onay penceresini ac
  deleteService(service: ConstructionService): void {
    if (
      this.pendingDeleteService !== null ||
      this.deletingServiceId !== null ||
      this.updatingService ||
      this.uploadingServiceId !== null ||
      this.publishingServiceId !== null
    ) {
      return;
    }

    this.pendingDeleteService = service;
    this.cdr.markForCheck();
  }

  // Silme islemini iptal et
  cancelDeleteService(): void {
    if (this.deletingServiceId !== null) {
      return;
    }

    this.pendingDeleteService = null;
    this.cdr.markForCheck();
  }

  // Onaylanan hizmeti veritabanindan sil
  confirmDeleteService(): void {
    const service = this.pendingDeleteService;

    if (
      !service ||
      this.deletingServiceId !== null ||
      this.updatingService ||
      this.uploadingServiceId !== null ||
      this.publishingServiceId !== null
    ) {
      return;
    }

    this.deletingServiceId = service.id;

    // CSRF token al ve hizmeti sil
    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.delete<void>(
            `/api/admin/services/${service.id}`,
            {
              withCredentials: true,
              headers: {
                'X-CSRF-TOKEN': csrfToken,
              },
            }
          )
        )
      )
      .subscribe({
        next: () => {
          this.deletingServiceId = null;
          this.pendingDeleteService = null;

          // Silinen hizmet duzenleniyorsa formu kapat
          if (this.editingServiceId === service.id) {
            this.cancelEditingService();
          }

          toast.success('Hizmet başarıyla silindi.');

          this.loadServices();
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Hizmet silinemedi:', error);

          this.deletingServiceId = null;

          toast.error(
            'Hizmet silinemedi. Lütfen tekrar deneyin.'
          );

          this.cdr.markForCheck();
        },
      });
  }

}

import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject,
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { switchMap } from 'rxjs';

interface Certificate {
  id: number;
  nameTr: string;
  nameEn: string;
  imageUrl: string;
  isPublished: boolean;
  displayOrder: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

@Component({
  selector: 'app-admin-certificates',
  imports: [],
  templateUrl: './certificates.html',
  styleUrl: './certificates.scss',
})
export class Certificates implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  certificates: Certificate[] = [];
  loading = true;
  errorMessage = '';

  // Yeni sertifika formu
  nameTr = '';
  nameEn = '';
  displayOrder = 0;

  saving = false;
  formError = '';


  // Sertifika gorseli yukleme durumu
  uploadingCertificateId: number | null = null;
  imageError = '';

  
  // Sertifika yayinlama islemleri
  publishingCertificateId: number | null = null;
  publishError = '';

  
  // Sertifika duzenleme formu
  editingCertificateId: number | null = null;

  editNameTr = '';
  editNameEn = '';
  editDisplayOrder = 0;

  editingError = '';
  updating = false;

  
  // Sertifika silme islemleri
  deletingCertificateId: number | null = null;

  
  // Ozel silme onay penceresi
  pendingDeleteCertificate: Certificate | null = null;

  // Toast bildirimleri
  toastMessage = '';
  toastType: 'success' | 'error' = 'success';

  private toastTimer: ReturnType<typeof setTimeout> | null = null;




  ngOnInit(): void {
    this.loadCertificates();
  }

  loadCertificates(): void {
    this.loading = true;
    this.errorMessage = '';

    this.http
      .get<Certificate[]>('/api/admin/certificates', {
        withCredentials: true,
      })
      .subscribe({
        next: (certificates) => {
          this.certificates = certificates;
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.loading = false;
          this.errorMessage =
            'Sertifikalar yüklenemedi. Lütfen tekrar deneyin.';
          this.cdr.markForCheck();
        },
      });
  }

  createCertificate(): void {
    if (this.saving) {
      return;
    }

    this.formError = '';

    const nameTr = this.nameTr.trim();
    const nameEn = this.nameEn.trim();
    const displayOrder = Number(this.displayOrder);

    if (!nameTr || !nameEn) {
      this.formError =
        'Türkçe ve İngilizce sertifika adlarını giriniz.';
      return;
    }

    if (nameTr.length > 200 || nameEn.length > 200) {
      this.formError =
        'Sertifika adları en fazla 200 karakter olabilir.';
      return;
    }

    if (
      !Number.isInteger(displayOrder) ||
      displayOrder < 0
    ) {
      this.formError =
        'Görüntülenme sırası sıfır veya pozitif bir tam sayı olmalıdır.';
      return;
    }

    this.saving = true;

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post<{ id: number; message: string }>(
            '/api/admin/certificates',
            {
              nameTr,
              nameEn,
              displayOrder,
            },
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
          this.saving = false;

          this.formError = '';
       

          this.nameTr = '';
          this.nameEn = '';
          this.displayOrder = 0;

          this.showToast(
            'Sertifika oluşturuldu. Şimdi görsel yükleyebilirsiniz.',
            'success'
          );

          this.loadCertificates();
          this.cdr.markForCheck();
        },
        error: () => {
          this.saving = false;

          this.formError = '';
      

          this.showToast(
            'Sertifika oluşturulamadı. Lütfen tekrar deneyin.',
            'error'
          );

          this.cdr.markForCheck();
        },
      });

  }

  
  uploadCertificateImage(
    certificateId: number,
    input: HTMLInputElement
  ): void {
    if (this.uploadingCertificateId !== null) {
      return;
    }

    this.imageError = '';
  

    const file = input.files?.[0];

    if (!file) {
      return;
    }

    const allowedExtensions = ['jpg', 'jpeg', 'png', 'webp'];
    const extension = file.name.split('.').pop()?.toLowerCase();

    if (!extension || !allowedExtensions.includes(extension)) {
      this.imageError = 'Yalnızca JPG, PNG ve WebP yükleyebilirsiniz.';
      input.value = '';
      return;
    }

    if (file.size === 0 || file.size > 15_000_000) {
      this.imageError = 'Görsel boş olamaz ve 15 MB sınırını aşamaz.';
      input.value = '';
      return;
    }

    this.uploadingCertificateId = certificateId;

    const formData = new FormData();
    formData.append('file', file);

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post(
            `/api/admin/certificates/${certificateId}/image`,
            formData,
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
          this.uploadingCertificateId = null;

          this.imageError = '';
         
          input.value = '';

          this.showToast(
            'Sertifika görseli başarıyla kaydedildi.',
            'success'
          );

          this.loadCertificates();
          this.cdr.markForCheck();
        },
        error: () => {
          this.uploadingCertificateId = null;

          this.imageError = '';
        
          input.value = '';

          this.showToast(
            'Görsel yüklenemedi. Dosyayı kontrol edip tekrar deneyin.',
            'error'
          );

          this.cdr.markForCheck();
        },
      });

  }

  
  toggleCertificatePublication(certificate: Certificate): void {
    if (this.publishingCertificateId !== null) {
      return;
    }

    this.publishError = '';
  

    const newPublishedStatus = !certificate.isPublished;

    // Gorseli olmayan sertifikalar yayinlanamaz
    if (newPublishedStatus && !certificate.imageUrl) {
      this.publishError =
        'Sertifikayi yayinlamadan once gorsel yuklemelisiniz.';
      return;
    }

    this.publishingCertificateId = certificate.id;

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put(
            `/api/admin/certificates/${certificate.id}`,
            {
              nameTr: certificate.nameTr,
              nameEn: certificate.nameEn,
              displayOrder: certificate.displayOrder,
              isPublished: newPublishedStatus,
            },
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
          this.publishingCertificateId = null;

          this.publishError = '';
         

          this.showToast(
            newPublishedStatus
              ? 'Sertifika başarıyla yayınlandı.'
              : 'Sertifika başarıyla yayından kaldırıldı.',
            'success'
          );

          this.loadCertificates();
          this.cdr.markForCheck();
        },
        error: () => {
          this.publishingCertificateId = null;

          this.publishError = '';
         

          this.showToast(
            'Yayın durumu değiştirilemedi. Lütfen tekrar deneyin.',
            'error'
          );

          this.cdr.markForCheck();
        },
      });

  }

  
  // Secilen sertifikanin bilgilerini duzenleme formuna aktar
  startEditing(certificate: Certificate): void {
    if (this.updating) {
      return;
    }

    this.editingCertificateId = certificate.id;

    this.editNameTr = certificate.nameTr;
    this.editNameEn = certificate.nameEn;
    this.editDisplayOrder = certificate.displayOrder;

    this.editingError = '';
    
  }

  // Duzenleme formunu kapat
  cancelEditing(): void {
    if (this.updating) {
      return;
    }

    this.editingCertificateId = null;

    this.editNameTr = '';
    this.editNameEn = '';
    this.editDisplayOrder = 0;

    this.editingError = '';
  }

  
  // Sertifika bilgilerini guncelle
  updateCertificate(certificate: Certificate): void {
    if (this.updating ||
        this.editingCertificateId !== certificate.id) {
      return;
    }

    this.editingError = '';
    

    const nameTr = this.editNameTr.trim();
    const nameEn = this.editNameEn.trim();
    const displayOrder = Number(this.editDisplayOrder);

    // Zorunlu alan kontrolu
    if (!nameTr || !nameEn) {
      this.editingError =
        'Türkçe ve İngilizce sertifika adları zorunludur.';
      return;
    }

    // Karakter siniri kontrolu
    if (nameTr.length > 200 || nameEn.length > 200) {
      this.editingError =
        'Sertifika adları en fazla 200 karakter olabilir.';
      return;
    }

    // Goruntulenme sirasi kontrolu
    if (
      !Number.isInteger(displayOrder) ||
      displayOrder < 0 ||
      displayOrder > 2147483647
    ) {
      this.editingError =
        'Görüntülenme sırası geçerli, negatif olmayan bir tam sayı olmalıdır.';
      return;
    }

    this.updating = true;

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put(
            `/api/admin/certificates/${certificate.id}`,
            {
              nameTr,
              nameEn,
              displayOrder,

              // Mevcut yayin durumunu koru
              isPublished: certificate.isPublished,
            },
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
          this.updating = false;
          this.editingCertificateId = null;

          this.editNameTr = '';
          this.editNameEn = '';
          this.editDisplayOrder = 0;

          this.editingError = '';
      

          this.showToast(
            'Sertifika bilgileri başarıyla güncellendi.',
            'success'
          );

          this.loadCertificates();
          this.cdr.markForCheck();
        },
        error: () => {
          this.updating = false;

         

          this.showToast(
            'Sertifika güncellenemedi. Lütfen tekrar deneyin.',
            'error'
          );

          this.cdr.markForCheck();
        },
      });

  }

  
  // Basari veya hata bildirimi goster
  showToast(
    message: string,
    type: 'success' | 'error' = 'success'
  ): void {
    if (this.toastTimer !== null) {
      clearTimeout(this.toastTimer);
    }

    this.toastMessage = message;
    this.toastType = type;
    this.cdr.markForCheck();

    this.toastTimer = setTimeout(() => {
      this.toastMessage = '';
      this.toastTimer = null;
      this.cdr.markForCheck();
    }, 4000);
  }

  // Toast bildirimini elle kapat
  closeToast(): void {
    if (this.toastTimer !== null) {
      clearTimeout(this.toastTimer);
      this.toastTimer = null;
    }

    this.toastMessage = '';
    this.cdr.markForCheck();
  }

  
  // Sil butonuna basildiginda ozel onay penceresini ac
  deleteCertificate(certificate: Certificate): void {
    if (
      this.deletingCertificateId !== null ||
      this.updating ||
      this.uploadingCertificateId !== null ||
      this.publishingCertificateId !== null
    ) {
      return;
    }

    this.pendingDeleteCertificate = certificate;
    
   
  }

  
  // Silme onay penceresini kapat
  cancelDeleteCertificate(): void {
    this.pendingDeleteCertificate = null;
    this.cdr.markForCheck();
  }

  
  // Ozel modal uzerinden sertifikayi sil
  confirmDeleteCertificate(): void {
    const certificate = this.pendingDeleteCertificate;

    if (
      !certificate ||
      this.deletingCertificateId !== null ||
      this.updating ||
      this.uploadingCertificateId !== null ||
      this.publishingCertificateId !== null
    ) {
      return;
    }

    this.deletingCertificateId = certificate.id;

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.delete<{
            id: number;
            message: string;
            warning?: boolean;
          }>(
            `/api/admin/certificates/${certificate.id}`,
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
        next: (result) => {
          this.deletingCertificateId = null;
          this.pendingDeleteCertificate = null;

          if (this.editingCertificateId === certificate.id) {
            this.cancelEditing();
          }

          if (result.warning) {
            this.showToast(
              'Sertifika silindi ancak görsel dosyası temizlenemedi.',
              'error'
            );
          } else {
            this.showToast(
              'Sertifika başarıyla silindi.',
              'success'
            );
          }

          this.loadCertificates();
          this.cdr.markForCheck();
        },
        error: () => {
          this.deletingCertificateId = null;

          this.showToast(
            'Sertifika silinemedi. Lütfen tekrar deneyin.',
            'error'
          );

          this.cdr.markForCheck();
        },
      });
  }




}

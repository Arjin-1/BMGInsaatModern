import { ChangeDetectorRef, Component, inject, input, output, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { switchMap } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';

interface AdminProject {
  id: number;
  nameTr: string;
  nameEn: string;
  descriptionTr: string;
  descriptionEn: string;
  city: string;
  employerTr: string;
  employerEn: string;
  startDate: string | null;
  endDate: string | null;
  status: number;
  isPublished: boolean;
  isFeatured: boolean;
  images: {
    id: number;
    imageUrl: string;
    isCover: boolean;
    displayOrder: number;
  }[];
}

@Component({
  imports: [FormsModule],
  selector: 'app-project-edit',
  styleUrl: './project-edit.scss',
  templateUrl: './project-edit.html',
})
export class ProjectEdit implements OnInit {
  readonly projectId = input<number | null>(null);
  readonly saved = output<void>();

  private readonly http = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);

  project: AdminProject | null = null;

  loading = true;
  errorMessage = '';

  saving = false;
  saveError = '';
  saveSuccess = '';

  uploadingImage = false;
  uploadError = '';
  uploadSuccess = '';

  // Silinmesi için onay bekleyen fotoğraf
  imagePendingDeleteId: number | null = null;
  changingCover = false;
  coverError = '';
  deletingImage = false;
  imageDeleteError = '';

  // Fotoğraf silme onayını aç
  requestImageDelete(imageId: number): void {
    this.imagePendingDeleteId = imageId;
  }

  // Fotoğraf silme işlemini iptal et
  cancelImageDelete(): void {
    this.imagePendingDeleteId = null;
  }

  setCoverImage(imageId: number): void {
    const project = this.project;

    if (!project || this.changingCover || this.deletingImage) {
      return;
    }

    const selectedImage = project.images.find((image) => image.id === imageId);

    if (!selectedImage || selectedImage.isCover) {
      return;
    }

    this.changingCover = true;
    this.coverError = '';

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put(
            `/api/admin/projects/${project.id}/images/${imageId}/cover`,
            {},
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
          project.images = project.images.map((image) => ({
            ...image,
            isCover: image.id === imageId,
          }));

          this.changingCover = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.changingCover = false;
          this.coverError = 'Kapak fotoğrafı değiştirilemedi. Lütfen tekrar deneyin.';
          this.cdr.markForCheck();
        },
      });
  }

  confirmImageDelete(): void {
    const project = this.project;
    const imageId = this.imagePendingDeleteId;

    if (!project || imageId === null || this.deletingImage) {
      return;
    }

    const image = project.images.find((i) => i.id === imageId);

    if (!image) {
      this.imagePendingDeleteId = null;
      return;
    }

    this.deletingImage = true;
    this.imageDeleteError = '';

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.delete(`/api/admin/projects/${project.id}/images/${imageId}`, {
            withCredentials: true,
            headers: {
              'X-CSRF-TOKEN': csrfToken,
            },
          }),
        ),
      )
      .subscribe({
        next: () => {
          const wasCover = image.isCover;

          project.images = project.images.filter((i) => i.id !== imageId);

          if (wasCover && project.images.length > 0) {
            const newCover = [...project.images].sort(
              (a, b) => a.displayOrder - b.displayOrder || a.id - b.id,
            )[0];

            newCover.isCover = true;
          }

          this.deletingImage = false;
          this.imagePendingDeleteId = null;
          this.cdr.markForCheck();
        },
        error: () => {
          this.deletingImage = false;
          this.imageDeleteError = 'Fotoğraf silinemedi. Sunucu bağlantısını kontrol edin.';
          this.cdr.markForCheck();
        },
      });
  }

  ngOnInit(): void {
    const id = this.projectId() ?? Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(id) || id <= 0) {
      this.errorMessage = 'Geçersiz proje ID.';
      this.loading = false;
      return;
    }

    this.http
      .get<AdminProject[]>('/api/admin/projects', {
        withCredentials: true,
      })
      .subscribe({
        next: (projects) => {
          this.project = projects.find((p) => p.id === id) ?? null;

          if (!this.project) {
            this.errorMessage = 'Proje bulunamadı.';
          }

          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.errorMessage = 'Proje bilgileri yüklenemedi.';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  saveProject(): void {
    const p = this.project;

    if (!p || this.saving) {
      return;
    }

    if (!p.nameTr.trim() || !p.city.trim()) {
      this.saveError = 'Proje adı ve şehir zorunludur.';
      this.saveSuccess = '';
      return;
    }

    if (p.startDate && p.endDate && p.endDate < p.startDate) {
      this.saveError = 'Bitiş tarihi başlangıç tarihinden önce olamaz.';
      this.saveSuccess = '';
      return;
    }

    this.saving = true;
    this.saveError = '';
    this.saveSuccess = '';

    const body = {
      nameTr: p.nameTr.trim(),
      nameEn: p.nameEn ?? '',
      descriptionTr: p.descriptionTr ?? '',
      descriptionEn: p.descriptionEn ?? '',
      city: p.city.trim(),
      employerTr: p.employerTr ?? '',
      employerEn: p.employerEn ?? '',
      startDate: p.startDate || null,
      endDate: p.endDate || null,
      status: p.status,
      isPublished: p.isPublished,
      isFeatured: p.isFeatured,
    };

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.put(`/api/admin/projects/${p.id}`, body, {
            withCredentials: true,
            headers: {
              'X-CSRF-TOKEN': csrfToken,
            },
          }),
        ),
      )
      .subscribe({
        next: () => {
          this.saving = false;
          this.saved.emit();
        },
        error: () => {
          this.saving = false;
          this.saveError = 'Proje kaydedilemedi. Bilgileri ve sunucu bağlantısını kontrol edin.';
          this.cdr.markForCheck();
        },
      });
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    const project = this.project;

    if (!file || !project || this.uploadingImage) {
      return;
    }

    this.uploadError = '';
    this.uploadSuccess = '';

    if (!/\.(jpg|jpeg|png|webp)$/i.test(file.name)) {
      this.uploadError = 'Yalnızca JPG, PNG ve WebP fotoğrafları yüklenebilir.';
      input.value = '';
      return;
    }

    if (file.size === 0 || file.size > 15_000_000) {
      this.uploadError = 'Fotoğraf boş olamaz veya 15 MB sınırını aşamaz.';
      input.value = '';
      return;
    }

    this.uploadingImage = true;

    const formData = new FormData();
    formData.append('file', file, file.name);

    this.http
      .get<{ csrfToken: string }>('/api/auth/csrf', {
        withCredentials: true,
      })
      .pipe(
        switchMap(({ csrfToken }) =>
          this.http.post<{
            id: number;
            imageUrl: string;
            isCover: boolean;
            displayOrder: number;
          }>(`/api/admin/projects/${project.id}/images`, formData, {
            withCredentials: true,
            headers: {
              'X-CSRF-TOKEN': csrfToken,
            },
          }),
        ),
      )
      .subscribe({
        next: (image) => {
          project.images = [...project.images, image];

          this.uploadingImage = false;
          this.uploadSuccess = 'Fotoğraf başarıyla yüklendi.';
          input.value = '';

          this.cdr.markForCheck();
        },
        error: () => {
          this.uploadingImage = false;
          this.uploadError = 'Fotoğraf yüklenemedi. Dosyayı ve sunucu bağlantısını kontrol edin.';
          input.value = '';

          this.cdr.markForCheck();
        },
      });
  }
}

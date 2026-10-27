import { ChangeDetectorRef, Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { switchMap } from 'rxjs';

@Component({
  imports: [FormsModule],
  selector: 'app-project-create',
  styleUrl: './project-create.scss',
  templateUrl: './project-create.html',
})
export class ProjectCreate {
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly created = output<void>();

  saving = false;
  saveError = '';
  project = {
    nameTr: '',
    nameEn: '',
    descriptionTr: '',
    descriptionEn: '',
    city: '',
    employerTr: '',
    employerEn: '',
    startDate: null as string | null,
    endDate: null as string | null,
    status: 1,
    isPublished: false,
    isFeatured: false,
  };
  
createProject(): void {
  if (this.saving) {
    return;
  }

  const p = this.project;

  if (!p.nameTr.trim() || !p.city.trim()) {
    this.saveError = 'Proje adı ve şehir zorunludur.';
    return;
  }

  if (p.startDate && p.endDate && p.endDate < p.startDate) {
    this.saveError = 'Bitiş tarihi başlangıç tarihinden önce olamaz.';
    return;
  }

  this.saving = true;
  this.saveError = '';

  const body = {
    ...p,
    nameTr: p.nameTr.trim(),
    city: p.city.trim(),
    employerTr: p.employerTr.trim(),
  };

  this.http
    .get<{ csrfToken: string }>('/api/auth/csrf', {
      withCredentials: true,
    })
    .pipe(
      switchMap(({ csrfToken }) =>
        this.http.post(
          '/api/admin/projects',
          body,
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
        this.created.emit();
      },
      error: () => {
        this.saving = false;
        this.saveError =
          'Proje oluşturulamadı. Bilgileri ve sunucu bağlantısını kontrol edin.';
        this.cdr.markForCheck();
      },
    });
}

}

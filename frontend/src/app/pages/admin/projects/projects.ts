import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { ProjectEdit } from '../project-edit/project-edit';
import { ProjectCreate } from '../project-create/project-create';
import { switchMap } from 'rxjs';

interface AdminProject {
  id: number;
  nameTr: string;
  city: string;
  status: number;
  isPublished: boolean;
  isFeatured: boolean;
  images: {
    id: number;
    imageUrl: string;
    isCover: boolean;
  }[];
}

@Component({
  imports: [RouterLink, ProjectEdit, ProjectCreate],
  selector: 'app-projects',
  styleUrl: './projects.scss',
  templateUrl: './projects.html',
})
export class Projects implements OnInit {
  selectedProjectId: number | null = null;
  projectToDelete: AdminProject | null = null;
  deleting = false;
deleteError = '';

openDeleteConfirm(project: AdminProject): void {
  this.projectToDelete = project;
}

closeDeleteConfirm(): void {
  this.projectToDelete = null;
}
  showCreateModal = false;

openCreateModal(): void {
  this.showCreateModal = true;
}

closeCreateModal(): void {
  this.showCreateModal = false;
}
onProjectCreated(): void {
  this.closeCreateModal();

  this.successMessage = 'Proje başarıyla oluşturuldu.';
  this.loadProjects();

  if (this.toastTimeout) {
    clearTimeout(this.toastTimeout);
  }

  this.toastTimeout = setTimeout(() => {
    this.successMessage = '';
    this.cdr.markForCheck();
  }, 4000);
}
  successMessage = '';
  private toastTimeout?: ReturnType<typeof setTimeout>;

openEditModal(projectId: number): void {
  this.selectedProjectId = projectId;
}

closeEditModal(): void {
  this.selectedProjectId = null;
}
onProjectSaved(): void {
  this.closeEditModal();
  this.successMessage = 'Proje başarıyla güncellendi.';
  this.loadProjects();

  if (this.toastTimeout) {
    clearTimeout(this.toastTimeout);
  }

  this.toastTimeout = setTimeout(() => {
    this.successMessage = '';
    this.cdr.markForCheck();
  }, 4000);
}
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);
  projects: AdminProject[] = [];
  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadProjects();
  }
  loadProjects(): void {
    this.loading = true;
    this.errorMessage = '';

    this.http
      .get<AdminProject[]>('/api/admin/projects', {
        withCredentials: true,
      })
      .subscribe({
        next: (projects) => {
          this.projects = projects;
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.errorMessage = 'Projeler yüklenemedi.';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }
  
deleteProject(): void {
  const project = this.projectToDelete;

  if (!project || this.deleting) {
    return;
  }

  this.deleting = true;
  this.deleteError = '';

  this.http
    .get<{ csrfToken: string }>('/api/auth/csrf', {
      withCredentials: true,
    })
    .pipe(
      switchMap(({ csrfToken }) =>
        this.http.delete(
          `/api/admin/projects/${project.id}`,
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
        this.deleting = false;
        this.closeDeleteConfirm();

        this.successMessage = 'Proje başarıyla silindi.';
        this.loadProjects();

        if (this.toastTimeout) {
          clearTimeout(this.toastTimeout);
        }

        this.toastTimeout = setTimeout(() => {
          this.successMessage = '';
          this.cdr.markForCheck();
        }, 4000);

        this.cdr.markForCheck();
      },
      error: () => {
        this.deleting = false;
        this.deleteError =
          'Silme işlemi tamamlanamadı. Proje listesini kontrol edin.';
        this.cdr.markForCheck();
      },
    });
}

}

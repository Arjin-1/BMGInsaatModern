import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LanguageService } from '../../core/services/language.service';


interface ProjectImage {
  id: number;
  imageUrl: string;
  isCover: boolean;
  displayOrder: number;
}

interface Project {
  id: number;
  nameTr: string;
  nameEn: string;
  city: string;
  status: number;
  isFeatured: boolean;
  images: ProjectImage[];
}

@Component({
  selector: 'app-projects',
  imports: [],
  templateUrl: './projects.html',
  styleUrl: './projects.scss',
})
export class Projects implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly languageService = inject(LanguageService);

  readonly language = this.languageService.language;

  readonly projects = signal<Project[]>([]);
  // Secili proje filtresi
  readonly selectedFilter = signal<'all' | 'ongoing' | 'completed'>('all');

  // Secilen filtreye gore projeleri listele
  readonly filteredProjects = computed(() => {
    const projects = this.projects();
    const filter = this.selectedFilter();

    if (filter === 'ongoing') {
      return projects.filter((project) => project.status === 1);
    }

    if (filter === 'completed') {
      return projects.filter((project) => project.status === 2);
    }

    return projects;
  });

  // Filtreyi degistir
  setFilter(filter: 'all' | 'ongoing' | 'completed'): void {
    this.selectedFilter.set(filter);
  }
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadProjects();
  }

  private loadProjects(): void {
    this.http
      .get<Project[]>('/api/projects')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (projects) => {
          this.projects.set(projects);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('Projeler yüklenirken bir hata oluştu.');
          this.isLoading.set(false);
        },
      });
  }
}

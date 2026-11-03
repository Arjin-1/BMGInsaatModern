import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { LanguageService } from '../../core/services/language.service';

// Public tarafta goruntulenecek proje videolari
interface ProjectVideo {
  id: number;

  titleTr: string;
  titleEn: string;

  descriptionTr: string;
  descriptionEn: string;

  videoUrl: string;
  displayOrder: number;
}


@Component({
  selector: 'app-project-videos',
  imports: [RouterLink],
  templateUrl: './project-videos.html',
  styleUrl: './project-videos.scss'
})

export class ProjectVideos implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly languageService = inject(LanguageService);

  readonly language = this.languageService.language;

  // Yalnizca yayinlanmis proje videolari
  videos: ProjectVideo[] = [];

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadVideos();
  }

  // Public API'den yayinlanmis videolari getir
  loadVideos(): void {
    this.loading = true;
    this.errorMessage = '';

    this.http
      .get<ProjectVideo[]>('/api/project-videos')
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
          this.errorMessage = 'Videolar yuklenemedi.';

          this.cdr.markForCheck();
        }
      });
  }

  // Guvenli video oynatma endpoint'i
  getVideoSrc(videoId: number): string {
    return `/api/project-videos/${videoId}/stream`;
  }
}

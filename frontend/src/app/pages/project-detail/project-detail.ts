import {
  Component,
  DestroyRef,
  HostListener,
  inject,
  OnInit,
  OnDestroy,
  signal,
  ViewChild,
  ElementRef,
  computed,
} from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LanguageService } from '../../core/services/language.service';
import { DOCUMENT } from '@angular/common';

interface ProjectImage {
  id: number;
  imageUrl: string;
  isCover: boolean;
  displayOrder: number;
}

interface ProjectDetailData {
  id: number;
  nameTr: string;
  nameEn: string;
  descriptionTr: string | null;
  descriptionEn: string | null;
  city: string;
  employerTr: string | null;
  employerEn: string | null;
  startDate: string | null;
  endDate: string | null;
  status: number;
  isFeatured: boolean;
  images: ProjectImage[];
}

@Component({
  selector: 'app-project-detail',
  imports: [],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss',
})
export class ProjectDetail implements OnInit, OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly languageService = inject(LanguageService);
  private readonly document = inject(DOCUMENT);
  private previousBodyOverflow: string | null = null;

  // Header'dan secilen dil
  readonly language = this.languageService.language;

  // API'den gelen proje
  readonly project = signal<ProjectDetailData | null>(null);
  // Yonetim panelinde kapak olarak secilen fotograf
  readonly coverImage = computed(() => {
    const images = this.project()?.images ?? [];

    return images.find((image) => image.isCover) ?? images[0] ?? null;
  });
  // Buyuk goruntulemede acik olan fotografin indeksi
  // null: Galeri kapali
  readonly selectedImageIndex = signal<number | null>(null);
  // Galeriyi acan fotograf butonunu sakla
  private lastFocusedGalleryButton: HTMLButtonElement | null = null;

  @ViewChild('lightboxClose')
  set lightboxCloseButton(button: ElementRef<HTMLButtonElement> | undefined) {
    if (button) {
      button.nativeElement.focus();
    }
  }

  // Yuklenme ve hata durumlari
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(id) || id <= 0) {
      this.errorMessage.set('Geçersiz proje numarası.');
      this.isLoading.set(false);
      return;
    }

    this.http
      .get<ProjectDetailData>(`/api/projects/${id}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (project) => {
          this.project.set(project);
          this.isLoading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.errorMessage.set('Proje bulunamadı veya yayında değil.');
          } else {
            this.errorMessage.set('Proje yüklenirken bir hata oluştu.');
          }

          this.isLoading.set(false);
        },
      });
  }

  openImage(index: number, trigger: HTMLButtonElement): void {
    const images = this.project()?.images ?? [];

    if (index < 0 || index >= images.length) {
      return;
    }

    // Galeriyi hangi butonun actigini sakla
    this.lastFocusedGalleryButton = trigger;

    if (this.selectedImageIndex() === null) {
      this.previousBodyOverflow = this.document.body.style.overflow;
      this.document.body.style.overflow = 'hidden';
    }

    this.selectedImageIndex.set(index);
  }

  closeImage(): void {
    this.selectedImageIndex.set(null);

    // Sayfanin kaydirma ozelligini geri getir
    if (this.previousBodyOverflow !== null) {
      this.document.body.style.overflow = this.previousBodyOverflow;
      this.previousBodyOverflow = null;
    }

    // Klavye odagini fotografi acan butona geri getir
    const trigger = this.lastFocusedGalleryButton;
    this.lastFocusedGalleryButton = null;

    if (trigger?.isConnected) {
      trigger.focus();
    }
  }

  // Sonraki fotograf
  nextImage(): void {
    const currentIndex = this.selectedImageIndex();
    const images = this.project()?.images ?? [];

    if (currentIndex === null || images.length === 0) {
      return;
    }

    this.selectedImageIndex.set((currentIndex + 1) % images.length);
  }

  // Onceki fotograf
  previousImage(): void {
    const currentIndex = this.selectedImageIndex();
    const images = this.project()?.images ?? [];

    if (currentIndex === null || images.length === 0) {
      return;
    }

    this.selectedImageIndex.set((currentIndex - 1 + images.length) % images.length);
  }

  // Galeri acikken klavye kontrollerini yonet
  @HostListener('document:keydown', ['$event'])
  handleKeyboard(event: KeyboardEvent): void {
    // Galeri kapaliysa klavye olaylarina karisma
    if (this.selectedImageIndex() === null) {
      return;
    }

    switch (event.key) {
      case 'Tab': {
        const buttons = Array.from(
          this.document.querySelectorAll<HTMLButtonElement>('.project-lightbox button'),
        );

        if (buttons.length === 0) {
          event.preventDefault();
          break;
        }

        const first = buttons[0];
        const last = buttons[buttons.length - 1];
        const active = this.document.activeElement;

        if (event.shiftKey && active === first) {
          event.preventDefault();
          last.focus();
        } else if (!event.shiftKey && active === last) {
          event.preventDefault();
          first.focus();
        } else if (!buttons.includes(active as HTMLButtonElement)) {
          event.preventDefault();
          first.focus();
        }

        break;
      }

      case 'Escape':
        event.preventDefault();
        this.closeImage();
        break;

      case 'ArrowRight':
        event.preventDefault();
        this.nextImage();
        break;

      case 'ArrowLeft':
        event.preventDefault();
        this.previousImage();
        break;
    }
  }

  ngOnDestroy(): void {
    this.closeImage();
  }
}

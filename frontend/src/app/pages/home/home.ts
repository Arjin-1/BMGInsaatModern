import { Component, computed, DestroyRef, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LanguageService } from '../../core/services/language.service';

interface FeaturedProject {
  id: number;
  nameTr: string;
  nameEn: string;
  city: string;
  isFeatured: boolean;
  images: {
    id: number;
    imageUrl: string;
    isCover: boolean;
    displayOrder: number;
  }[];
}

interface HeroSlide {
  image: string;
  imageAlt: string;
  imageAltEn: string;
  eyebrow: string;
  eyebrowEn: string;
  title: string;
  titleEn: string;
  highlightedTitle: string;
  highlightedTitleEn: string;
  description: string;
  descriptionEn: string;
}

@Component({
  imports: [],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home implements OnInit, OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly languageService = inject(LanguageService);

  // Header'dan seçilen aktif dil
  readonly language = this.languageService.language;

  readonly featuredProjects = signal<FeaturedProject[]>([]);
  readonly projectsLoading = signal(true);
  readonly projectsError = signal(false);

  // BMG Insaat'in gercek proje fotograflari

readonly slides: readonly HeroSlide[] = [
  {
    image: '/images/hero-vinc.webp',
    imageAlt: 'Vinç ile büyük çelik boru montajı',
    imageAltEn: 'Installation of a large steel pipe using a crane',
    eyebrow: 'BMG TAAHHÜT İNŞAAT',
    eyebrowEn: 'BMG CONTRACTING & CONSTRUCTION',
    title: 'Güçlü Altyapılar.',
    titleEn: 'Strong Infrastructure.',
    highlightedTitle: 'Kalıcı Eserler.',
    highlightedTitleEn: 'Lasting Achievements.',
    description:
      'Mühendislik, güven ve kalite anlayışıyla geleceğe değer katan projeler geliştiriyoruz.',
    descriptionEn:
      'We develop projects that create lasting value through engineering, trust and quality.',
  },
  {
    image: '/images/hero-boru.webp',
    imageAlt: 'Büyük çelik boru hattı altyapı uygulaması',
    imageAltEn: 'Large steel pipeline infrastructure construction',
    eyebrow: 'MÜHENDİSLİK VE TAAHHÜT',
    eyebrowEn: 'ENGINEERING & CONTRACTING',
    title: 'Mühendislik Gücü.',
    titleEn: 'Engineering Excellence.',
    highlightedTitle: 'Güvenilir Uygulamalar.',
    highlightedTitleEn: 'Reliable Solutions.',
    description:
      'Teknik bilgi birikimimizle altyapı projelerinde nitelikli çözümler üretiyoruz.',
    descriptionEn:
      'We deliver high-quality infrastructure solutions backed by our technical expertise.',
  },
  {
    image: '/images/hero-beton.webp',
    imageAlt: 'Viyadük yakınında beton pompasıyla yapılan inşaat çalışması',
    imageAltEn: 'Concrete construction work near a viaduct',
    eyebrow: 'ALTYAPI PROJELERİ',
    eyebrowEn: 'INFRASTRUCTURE PROJECTS',
    title: 'Sağlam Temeller.',
    titleEn: 'Solid Foundations.',
    highlightedTitle: 'Güçlü Bir Gelecek.',
    highlightedTitleEn: 'A Stronger Future.',
    description:
      'Her aşamada kaliteyi ve sorumluluk bilincini ön planda tutuyoruz.',
    descriptionEn:
      'We prioritize quality and responsibility at every stage of our projects.',
  },
  {
    image: '/images/hero-gece.webp',
    imageAlt: 'Akşam saatlerinde boru hattı ve beton uygulaması',
    imageAltEn: 'Pipeline and concrete construction work in the evening',
    eyebrow: 'SAHADA MÜHENDİSLİK',
    eyebrowEn: 'ENGINEERING IN ACTION',
    title: 'Her Projede Emek.',
    titleEn: 'Dedication in Every Project.',
    highlightedTitle: 'Her Eserde Güven.',
    highlightedTitleEn: 'Trust in Every Achievement.',
    description:
      'Disiplinli çalışma anlayışımızla kalıcı değerler oluşturmayı hedefliyoruz.',
    descriptionEn:
      'Through disciplined teamwork, we aim to create lasting value.',
  },
];


  readonly activeSlideIndex = signal(0);

  readonly activeSlide = computed(() => this.slides[this.activeSlideIndex()]);

  // Her fotografin ekranda kalacagi sure
  private readonly slideInterval = 6000;

  // Otomatik gecis zamanlayicisi
  private sliderTimer?: ReturnType<typeof setInterval>;

  // Sayfa acildiginda otomatik gecis baslar
  ngOnInit(): void {
    this.startAutoSlide();
    this.loadFeaturedProjects();
  }

  private loadFeaturedProjects(): void {
    this.http
      .get<FeaturedProject[]>('/api/projects')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (projects) => {
          this.featuredProjects.set(projects.filter((project) => project.isFeatured));

          this.projectsLoading.set(false);
        },
        error: () => {
          this.projectsError.set(true);
          this.projectsLoading.set(false);
        },
      });
  }

  private startAutoSlide(): void {
    this.stopAutoSlide();

    this.sliderTimer = setInterval(() => {
      this.nextSlide();
    }, this.slideInterval);
  }

  private stopAutoSlide(): void {
    if (this.sliderTimer !== undefined) {
      clearInterval(this.sliderTimer);
      this.sliderTimer = undefined;
    }
  }

  // Sonraki fotograf
  nextSlide(): void {
    this.activeSlideIndex.update((currentIndex) => (currentIndex + 1) % this.slides.length);
  }

  // Onceki fotograf
  previousSlide(): void {
    this.activeSlideIndex.update(
      (currentIndex) => (currentIndex - 1 + this.slides.length) % this.slides.length,
    );
  }

  // Sayfadan ayrilinca zamanlayiciyi temizle
  ngOnDestroy(): void {
    this.stopAutoSlide();
  }
}

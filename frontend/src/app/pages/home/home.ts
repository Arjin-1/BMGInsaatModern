
import {
  Component,
  computed,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';

interface HeroSlide {
  image: string;
  imageAlt: string;
  eyebrow: string;
  title: string;
  highlightedTitle: string;
  description: string;
}

@Component({
  imports: [],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home implements OnInit, OnDestroy {

  // BMG Insaat'in gercek proje fotograflari
  readonly slides: readonly HeroSlide[] = [
    {
      image: '/images/hero-vinc.webp',
      imageAlt: 'Vinç ile büyük çelik boru montajı',
      eyebrow: 'BMG TAAHHÜT İNŞAAT',
      title: 'Güçlü Altyapılar.',
      highlightedTitle: 'Kalıcı Eserler.',
      description:
        'Mühendislik, güven ve kalite anlayışıyla geleceğe değer katan projeler geliştiriyoruz.',
    },
    {
      image: '/images/hero-boru.webp',
      imageAlt: 'Büyük çelik boru hattı altyapı uygulaması',
      eyebrow: 'MÜHENDİSLİK VE TAAHHÜT',
      title: 'Mühendislik Gücü.',
      highlightedTitle: 'Güvenilir Uygulamalar.',
      description:
        'Teknik bilgi birikimimizle altyapı projelerinde nitelikli çözümler üretiyoruz.',
    },
    {
      image: '/images/hero-beton.webp',
      imageAlt: 'Viyadük yakınında beton pompasıyla yapılan inşaat çalışması',
      eyebrow: 'ALTYAPI PROJELERİ',
      title: 'Sağlam Temeller.',
      highlightedTitle: 'Güçlü Bir Gelecek.',
      description:
        'Her aşamada kaliteyi ve sorumluluk bilincini ön planda tutuyoruz.',
    },
    {
      image: '/images/hero-gece.webp',
      imageAlt: 'Akşam saatlerinde boru hattı ve beton uygulaması',
      eyebrow: 'SAHADA MÜHENDİSLİK',
      title: 'Her Projede Emek.',
      highlightedTitle: 'Her Eserde Güven.',
      description:
        'Disiplinli çalışma anlayışımızla kalıcı değerler oluşturmayı hedefliyoruz.',
    },
  ];

  readonly activeSlideIndex = signal(0);

  readonly activeSlide = computed(
    () => this.slides[this.activeSlideIndex()]
  );

  // Her fotografin ekranda kalacagi sure
  private readonly slideInterval = 6000;

  // Otomatik gecis zamanlayicisi
  private sliderTimer?: ReturnType<typeof setInterval>;

  // Sayfa acildiginda otomatik gecis baslar
  ngOnInit(): void {
    this.startAutoSlide();
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
    this.activeSlideIndex.update(
      (currentIndex) => (currentIndex + 1) % this.slides.length
    );
  }

  // Onceki fotograf
  previousSlide(): void {
    this.activeSlideIndex.update(
      (currentIndex) =>
        (currentIndex - 1 + this.slides.length) % this.slides.length
    );
  }

  // Sayfadan ayrilinca zamanlayiciyi temizle
  ngOnDestroy(): void {
    this.stopAutoSlide();
  }
}

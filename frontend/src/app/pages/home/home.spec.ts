import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Home } from './home';

describe('Home', () => {
  let fixture: ComponentFixture<Home>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
    }).compileComponents();

    fixture = TestBed.createComponent(Home);

    await fixture.whenStable();
  });

  it('should create the home component', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display the main heading', () => {
    const compiled = fixture.nativeElement as HTMLElement;

    const heading = compiled.querySelector('h1');

    expect(heading).not.toBeNull();
    expect(heading?.textContent).toContain(
      'Geleceği Güvenle İnşa Ediyoruz.'
    );
  });

  it('should display the infrastructure hero image', () => {
    const compiled = fixture.nativeElement as HTMLElement;

    const image = compiled.querySelector('.hero-image img');

    expect(image).not.toBeNull();
    expect(image?.getAttribute('src')).toBe(
      '/images/hero-altyapi.jpg'
    );
    expect(image?.getAttribute('alt')).toBeTruthy();
  });

  it('should link to the projects section', () => {
    const compiled = fixture.nativeElement as HTMLElement;

    const projectsLink = compiled.querySelector(
      'a.hero-button'
    );

    expect(projectsLink).not.toBeNull();
    expect(projectsLink?.getAttribute('href')).toBe(
      '#projeler'
    );
  });
});

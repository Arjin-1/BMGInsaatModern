import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
    })
      .compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });


it('should render the shared header', async () => {
  const fixture = TestBed.createComponent(App);

  await fixture.whenStable();

  const compiled = fixture.nativeElement as HTMLElement;

  const header = compiled.querySelector('app-header .site-header');

  expect(header).not.toBeNull();
});

});

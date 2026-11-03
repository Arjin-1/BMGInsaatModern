import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { Header } from './shared/components/header/header';
import { Footer } from './shared/components/footer/footer';
import { ScrollToTop } from './shared/components/scroll-to-top/scroll-to-top';


@Component({
  selector: 'app-root',
  imports: [Header, RouterOutlet, Footer, ScrollToTop],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly router = inject(Router);
}

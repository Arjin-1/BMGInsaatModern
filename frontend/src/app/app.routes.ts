import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'Ana Sayfa | BMG İnşaat',
    loadComponent: () =>
      import('./pages/home/home').then((m) => m.Home),
  },
  {
    path: 'about',
    title: 'Hakkımızda | BMG İnşaat',
    loadComponent: () =>
      import('./pages/about/about').then((m) => m.About),
  },
];


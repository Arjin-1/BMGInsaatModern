import { Routes } from '@angular/router';
import { adminAuthGuard } from './core/guards/admin-auth.guard';

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
  {
    path: 'admin/login',
    title: 'Yönetici Girişi | BMG İnşaat',
    loadComponent: () =>
      import('./pages/admin/login/login').then((m) => m.Login),
  },
  {
    path: 'admin/projects',
    title: 'Proje Yönetimi | BMG İnşaat',
    canActivate: [adminAuthGuard],
    loadComponent: () =>
      import('./pages/admin/projects/projects').then((m) => m.Projects),
  },
  {
    path: 'admin/projects/:id/edit',
    title: 'Proje Düzenle | BMG İnşaat',
    canActivate: [adminAuthGuard],
    loadComponent: () =>
      import('./pages/admin/project-edit/project-edit').then(
        (m) => m.ProjectEdit
      ),
  },
];


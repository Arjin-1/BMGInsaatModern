import { Routes } from '@angular/router';
import { adminAuthGuard } from './core/guards/admin-auth.guard';

export const routes: Routes = [
  // HERKESE AÇIK SAYFALAR
  {
    path: '',
    title: 'Ana Sayfa | BMG İnşaat',
    loadComponent: () => import('./pages/home/home').then((m) => m.Home),
  },
  {
    path: 'about',
    title: 'Hakkımızda | BMG İnşaat',
    loadComponent: () => import('./pages/about/about').then((m) => m.About),
  },
  {
    path: 'projects',
    title: 'Projelerimiz | BMG İnşaat',
    loadComponent: () => import('./pages/projects/projects').then((m) => m.Projects),
  },
  {
    path: 'projects/:id',
    title: 'Proje Detayı | BMG İnşaat',
    loadComponent: () =>
      import('./pages/project-detail/project-detail').then((m) => m.ProjectDetail),
  },

  {
    path: 'services',
    title: 'Hizmetlerimiz | BMG İnşaat',
    loadComponent: () => import('./pages/services/services').then((m) => m.Services),
  },
  {
    path: 'project-videos',
    title: 'Proje Videoları | BMG İnşaat',
    loadComponent: () =>
      import('./pages/project-videos/project-videos').then((m) => m.ProjectVideos),
  },
  {
    path: 'contact',
    title: 'İletişim | BMG İnşaat',
    loadComponent: () => import('./pages/contact/contact').then((m) => m.Contact),
  },
  {
    path: 'certificates',
    title: 'Sertifikalarımız | BMG İnşaat',
    loadComponent: () => import('./pages/certificates/certificates').then((m) => m.Certificates),
  },

  // YÖNETİCİ GİRİŞ SAYFASI
  {
    path: 'admin/login',
    title: 'Yönetici Girişi | BMG İnşaat',
    loadComponent: () => import('./pages/admin/login/login').then((m) => m.Login),
  },

  // E-POSTA DOGRULAMA SAYFASI
  // Giris yapmadan erisilebilir.
  {
    path: 'admin/confirm-email',
    title: 'E-posta Doğrulama | BMG İnşaat',
    loadComponent: () =>
      import('./pages/admin/confirm-email/confirm-email').then((m) => m.ConfirmEmail),
  },

  {
    path: 'admin/forgot-password',
    loadComponent: () =>
      import('./pages/admin/forgot-password/forgot-password').then((m) => m.ForgotPassword),
  },

  {
    path: 'admin/reset-password',
    loadComponent: () =>
      import('./pages/admin/reset-password/reset-password').then((m) => m.ResetPassword),
  },

  // ORTAK YÖNETİM PANELİ
  {
    path: 'admin',
    canActivate: [adminAuthGuard],
    loadComponent: () =>
      import('./shared/components/admin-layout/admin-layout').then((m) => m.AdminLayout),

    children: [
      {
        path: 'projects',
        title: 'Proje Yönetimi | BMG İnşaat',
        canActivate: [adminAuthGuard],
        loadComponent: () => import('./pages/admin/projects/projects').then((m) => m.Projects),
      },
      {
        path: 'about',
        title: 'Hakkımızda Yönetimi | BMG İnşaat',
        canActivate: [adminAuthGuard],
        loadComponent: () => import('./pages/admin/about/about').then((m) => m.About),
      },

      {
        path: 'certificates',
        title: 'Sertifikalar Yönetimi | BMG İnşaat',
        canActivate: [adminAuthGuard],
        loadComponent: () =>
          import('./pages/admin/certificates/certificates').then((m) => m.Certificates),
      },

      {
        path: 'services',
        title: 'Hizmetler Yönetimi | BMG İnşaat',
        canActivate: [adminAuthGuard],
        loadComponent: () => import('./pages/admin/services/services').then((m) => m.Services),
      },

      {
        path: 'project-videos',
        title: 'Proje Videoları Yönetimi | BMG İnşaat',
        canActivate: [adminAuthGuard],
        loadComponent: () =>
          import('./pages/admin/project-videos/project-videos').then((m) => m.ProjectVideos),
      },

      {
        path: 'projects/:id/edit',
        title: 'Proje Düzenle | BMG İnşaat',
        canActivate: [adminAuthGuard],
        loadComponent: () =>
          import('./pages/admin/project-edit/project-edit').then((m) => m.ProjectEdit),
      },
    ],
  },
];

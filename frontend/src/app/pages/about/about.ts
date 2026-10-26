import { Component, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AboutApiService } from '../../core/services/about-api.service';

import {
  AboutContent,
  CompanyValue,
} from '../../core/models/about.models';

@Component({
  selector: 'app-about',
  imports: [],
  templateUrl: './about.html',
  styleUrl: './about.scss',
})
export class About {
  private readonly aboutApi = inject(AboutApiService);

  readonly aboutContent = signal<AboutContent | null>(null);
  readonly companyValues = signal<CompanyValue[]>([]);

  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  constructor() {
    forkJoin({
      about: this.aboutApi.getAboutContent(),
      values: this.aboutApi.getPublishedCompanyValues(),
    })
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: ({ about, values }) => {
          this.aboutContent.set(about);
          this.companyValues.set(values);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set(
            'İçerikler yüklenirken bir hata oluştu.'
          );
          this.isLoading.set(false);
        },
      });
  }
}


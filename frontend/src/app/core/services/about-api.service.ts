import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  AboutContent,
  CompanyValue,
} from '../models/about.models';

@Injectable({
  providedIn: 'root',
})
export class AboutApiService {
  private readonly http = inject(HttpClient);

  getAboutContent(): Observable<AboutContent> {
    return this.http.get<AboutContent>('/api/about');
  }

  getPublishedCompanyValues(): Observable<CompanyValue[]> {
    return this.http.get<CompanyValue[]>('/api/company-values');
  }
}

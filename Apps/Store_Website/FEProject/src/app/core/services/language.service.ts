import { Injectable, computed, signal } from '@angular/core';

type Language = 'en' | 'ar';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly language = signal<Language>('en');
  readonly isArabic = computed(() => this.language() === 'ar');

  toggleLanguage(): void {
    const next = this.language() === 'en' ? 'ar' : 'en';
    this.language.set(next);
    document.documentElement.lang = next;
    document.documentElement.dir = next === 'ar' ? 'rtl' : 'ltr';
  }
}

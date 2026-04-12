import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';
import { Directionality } from '@angular/cdk/bidi';

@Injectable({
  providedIn: 'root'
})
export class DealerTranslationService {
  private currentLangSubject = new BehaviorSubject<string>('en');
  public currentLang$ = this.currentLangSubject.asObservable();
  private directionality = inject(Directionality);

  constructor(private translate: TranslateService) {
    // Set default language
    this.translate.setDefaultLang('en');
    this.translate.use('en');
  }

  setLanguage(lang: string) {
    this.translate.use(lang);
    this.currentLangSubject.next(lang);
    
    // Update document direction and lang attribute
    const htmlTag = document.documentElement;
    const newDirection = lang === 'ar' ? 'rtl' : 'ltr';
    
    if (lang === 'ar') {
      htmlTag.setAttribute('dir', 'rtl');
      htmlTag.setAttribute('lang', 'ar');
      document.body.classList.add('rtl');
      document.body.classList.remove('ltr');
    } else {
      htmlTag.setAttribute('dir', 'ltr');
      htmlTag.setAttribute('lang', 'en');
      document.body.classList.add('ltr');
      document.body.classList.remove('rtl');
    }
    
    // Update Material CDK Directionality to fix dialog overlay direction
    (this.directionality as any).value = newDirection;
    this.directionality.change.emit(newDirection as any);
    
    // Store in localStorage for persistence
    localStorage.setItem('selectedLanguage', lang);
  }

  getCurrentLanguage(): string {
    return this.currentLangSubject.value;
  }

  getCurrentDirection(): 'ltr' | 'rtl' {
    return this.currentLangSubject.value === 'ar' ? 'rtl' : 'ltr';
  }

  getTranslation(key: string): string {
    return this.translate.instant(key);
  }

  // Initialize language from localStorage if available
  initializeLanguage() {
    const savedLang = localStorage.getItem('selectedLanguage');
    if (savedLang && (savedLang === 'en' || savedLang === 'ar')) {
      this.setLanguage(savedLang);
    } else {
      // Set default direction on first load
      this.setLanguage('en');
    }
  }
}
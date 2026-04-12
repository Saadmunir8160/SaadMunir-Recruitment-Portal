import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DealerTranslationService } from '../../services/translation.service';

@Component({
  selector: 'app-language-switch',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="language-switch">
      <button 
        class="btn-lang"
        [class.active]="currentLang === 'en'"
        (click)="switchLanguage('en')">
        EN
      </button>
      <button 
        class="btn-lang"
        [class.active]="currentLang === 'ar'"
        (click)="switchLanguage('ar')">
        عربي
      </button>
    </div>
  `,
  styles: [`
    .language-switch {
      display: flex;
      gap: 8px;
    }
    
    .btn-lang {
      padding: 6px 12px;
      border: 1px solid #ddd;
      background: white;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
      transition: all 0.3s;
    }
    
    .btn-lang:hover {
      background: #f5f5f5;
    }
    
    .btn-lang.active {
      background: #007bff;
      color: white;
      border-color: #007bff;
    }
  `]
})
export class LanguageSwitchComponent {
  currentLang = 'en';

  constructor(private translationService: DealerTranslationService) {
    this.currentLang = this.translationService.getCurrentLanguage();
    
    this.translationService.currentLang$.subscribe(lang => {
      this.currentLang = lang;
    });
  }

  switchLanguage(lang: string) {
    this.translationService.setLanguage(lang);
  }
}
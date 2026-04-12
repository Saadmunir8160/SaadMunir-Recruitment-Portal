import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-shop-progress',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">
          <!-- <span class="material-icons">shopping_cart</span> -->
          {{ 'DEALER.SHOP.PROGRESS.TITLE' | translate }}
        </h1>
        <!-- <p class="page-subtitle">{{ 'DEALER.SHOP.PROGRESS.SUBTITLE' | translate }}</p> -->
      </div>
    </div>
  `,
  styleUrls: ['./shop-progress.component.scss']
})
export class ShopProgressComponent {
  @Input() currentStep: number = 1;
}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { ShopProgressComponent } from './components/shop-progress/shop-progress.component';

@Component({
  selector: 'app-shop-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, ShopProgressComponent],
  template: `
    <div class="shop-layout">
      <div class="shop-container">
        <!-- Main content area where routed components will load -->
        <router-outlet></router-outlet>
      </div>
    </div>
  `,
  styles: [`
    .shop-layout {
      min-height: 100vh;
      background: #f8f9fa;
      padding: 20px 0;
    }
    
    .shop-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 20px;
    }
    
    @media (max-width: 768px) {
      .shop-container {
        padding: 0 15px;
      }
    }
  `]
})
export class ShopLayoutComponent {}

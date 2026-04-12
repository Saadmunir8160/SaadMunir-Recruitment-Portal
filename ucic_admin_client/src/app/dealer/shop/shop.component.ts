import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { ShopService } from './services/shop.service';
import { CartService } from './services/cart.service';

// Import step components
import { ProductsComponent } from './components/step1-products/products.component';
import { ProductDetailsComponent } from './components/step2-product-details/product-details.component';
import { CartComponent } from './components/step3-cart/cart.component';
import { Step4CheckoutComponent } from './components/step4-checkout/step4-checkout.component';

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    ProductsComponent,
    ProductDetailsComponent,
    CartComponent,
    Step4CheckoutComponent
  ],
  templateUrl: './shop.component.html',
  styleUrls: ['./shop.component.scss']
})
export class ShopComponent implements OnInit, OnDestroy {
  currentStep = 1;
  totalSteps = 4;
  steps: any[] = [];
  cartItemCount = 0;
  private subscriptions: Subscription[] = [];

  constructor(
    private shopService: ShopService,
    private cartService: CartService
  ) {
    this.steps = this.shopService.steps;
    this.totalSteps = this.shopService.totalSteps;
  }

  ngOnInit(): void {
    // Subscribe to current step changes
    const stepSub = this.shopService.getCurrentStep().subscribe(step => {
      this.currentStep = step;
    });
    this.subscriptions.push(stepSub);

    // Subscribe to cart changes
    const cartSub = this.cartService.getCartSummary().subscribe(summary => {
      this.cartItemCount = summary.itemCount;
    });
    this.subscriptions.push(cartSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  nextStep(): void {
    if (this.canProceedToNext()) {
      this.shopService.nextStep();
    }
  }

  previousStep(): void {
    this.shopService.previousStep();
  }

  goToStep(stepNumber: number): void {
    this.shopService.goToStep(stepNumber);
  }

  canProceedToNext(): boolean {
    return this.shopService.canProceedFromStep(this.currentStep);
  }

  isStepCompleted(stepNumber: number): boolean {
    return stepNumber < this.currentStep;
  }

  isStepActive(stepNumber: number): boolean {
    return stepNumber === this.currentStep;
  }

  getStepClass(stepNumber: number): string {
    if (this.isStepCompleted(stepNumber)) {
      return 'completed';
    } else if (this.isStepActive(stepNumber)) {
      return 'active';
    }
    return 'pending';
  }

  onAddToCart(product: any): void {
    // This will be called from step1-products component
  }

  onCartUpdate(): void {
    // This will be called from step3-cart component
  }

  onOrderComplete(orderData: any): void {
    // Order completed successfully
  }
}

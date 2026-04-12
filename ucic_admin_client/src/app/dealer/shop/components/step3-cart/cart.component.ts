import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { DealerTranslationService } from '../../../services/translation.service';
import { CartService } from '../../services/cart.service';
import { UnitConfigurationService } from '../../services/unit-configuration.service';
import { CartItem, CartSummary } from '../../models/cart-item.model';
import { ShopProgressComponent } from '../shop-progress/shop-progress.component';
import { ClearCartDialogComponent } from '../clear-cart-dialog/clear-cart-dialog.component';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, ShopProgressComponent, TranslateModule],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent implements OnInit {
  @Output() nextStep = new EventEmitter<void>();
  @Output() previousStep = new EventEmitter<void>();
  @Output() cartUpdate = new EventEmitter<void>();

  cartItems: CartItem[] = [];
  Math = Math; // Make Math available in template
  cartSummary: CartSummary = {
    items: [],
    totalBags: 0,
    totalTons: 0,
    totalTrucks: 0,
    itemCount: 0
  };

  // Track bags per truck setting for each cart item (default 500)
  private bagsPerTruckSettings: Map<string, number> = new Map();

  constructor(
    private cartService: CartService,
    private unitConfigService: UnitConfigurationService,
    private router: Router,
    private translate: TranslateService,
    private dialog: MatDialog,
    private translationService: DealerTranslationService
  ) {}

  ngOnInit(): void {
    // Subscribe to cart items
    this.cartService.getCartItems().subscribe((items: CartItem[]) => {
      this.cartItems = items;
    });
    
    // Subscribe to cart summary
    this.cartService.getCartSummary().subscribe((summary: CartSummary) => {
      this.cartSummary = summary;
    });
  }

  // Truck-based calculation methods
  getItemBagsPerTruck(item: CartItem): number {
    return this.bagsPerTruckSettings.get(item.id) || 500; // Default to 500 bags per truck
  }
  get backArrowIcon(): string {
    return this.translate.currentLang === 'ar' ? 'arrow_back' : 'arrow_forward';
  }
  setItemBagsPerTruck(item: CartItem, bagsPerTruck: number): void {
    const currentTrucks = this.getItemTrucks(item);
    this.bagsPerTruckSettings.set(item.id, bagsPerTruck);
    
    // Recalculate quantity based on current trucks and new bags per truck
    const newQuantity = currentTrucks * bagsPerTruck;
    
    // Update the cart item quantity to maintain same number of trucks
    this.cartService.updateQuantity(item.id, newQuantity);
    this.cartUpdate.emit();
  }

  getItemTrucks(item: CartItem): number {
    if (item.unit !== 'BAG') return 0;
    const bagsPerTruck = this.getItemBagsPerTruck(item);
    return Math.ceil(item.quantity / bagsPerTruck);
  }

  updateItemTrucks(item: CartItem, trucks: number): void {
    if (item.unit !== 'BAG') return;
    
    const bagsPerTruck = this.getItemBagsPerTruck(item);
    const newQuantity = trucks * bagsPerTruck;
    
    // Update the cart item quantity
    this.cartService.updateQuantity(item.id, newQuantity);
    this.cartUpdate.emit();
  }

  increaseItemTrucks(item: CartItem): void {
    const currentTrucks = this.getItemTrucks(item);
    if (currentTrucks < 100) { // Max 100 trucks
      this.updateItemTrucks(item, currentTrucks + 1);
    }
  }

  decreaseItemTrucks(item: CartItem): void {
    const currentTrucks = this.getItemTrucks(item);
    if (currentTrucks > 1) { // Min 1 truck
      this.updateItemTrucks(item, currentTrucks - 1);
    }
  }

  onItemTrucksChange(item: CartItem, event: Event): void {
    const target = event.target as HTMLInputElement;
    const trucks = Math.max(1, Math.min(100, Number(target.value) || 1));
    this.updateItemTrucks(item, trucks);
  }

  updateCartSummary(): void {
    // This method is no longer needed as we subscribe to summary changes
  }

  updateQuantity(productId: string, quantity: number): void {
    // Find the item to check unit type for validation
    const item = this.cartItems.find(item => item.id === productId);
    if (item) {
      const minQty = this.getMinQuantity(item.unit);
      const maxQty = this.getMaxQuantity(item.unit);
      const validQuantity = Math.max(minQty, Math.min(maxQty, quantity));
      
      if (validQuantity > 0) {
        this.cartService.updateQuantity(productId, validQuantity);
        this.cartUpdate.emit();
      }
    }
  }

  onQuantityChange(productId: string, event: Event): void {
    const target = event.target as HTMLInputElement;
    const quantity = Number(target.value);
    
    // Find the item to check unit type for validation
    const item = this.cartItems.find(item => item.id === productId);
    if (item) {
      const minQty = this.getMinQuantity(item.unit);
      const maxQty = this.getMaxQuantity(item.unit);
      const validQuantity = Math.max(minQty, Math.min(maxQty, quantity));
      this.updateQuantity(productId, validQuantity);
    }
  }

  getMinQuantity(unit: string): number {
    const config = this.unitConfigService.getUnitConfiguration(unit);
    return config?.minValue || 1;
  }

  getMaxQuantity(unit: string): number {
    const config = this.unitConfigService.getUnitConfiguration(unit);
    return config?.maxValue || 1000;
  }

  getStepValue(unit: string): number {
    const config = this.unitConfigService.getUnitConfiguration(unit);
    return config?.stepValue || 1;
  }

  removeItem(productId: string): void {
    this.cartService.removeFromCart(productId);
    this.cartUpdate.emit();
  }

  clearCart(): void {
    const dialogRef = this.dialog.open(ClearCartDialogComponent, {
      width: '440px',
      maxWidth: '90vw',
      panelClass: 'clear-cart-dialog-container',
      direction: this.translationService.getCurrentDirection()
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        this.cartService.clearCart();
        this.cartUpdate.emit();
        // Stay on cart page - empty cart state will be shown automatically
      }
    });
  }

  proceedToCheckout(): void {
    // Validate cart is not empty and has valid items
    if (!this.canProceedToCheckout()) {
      const message = this.getCheckoutDisabledMessage();
      alert(message || 'Cannot proceed to checkout. Please check your cart.');
      return;
    }

    // Additional validation can be added here (e.g., check minimum order value, product availability, etc.)
    
    this.router.navigate(['/dealer/shop/checkout']).then(success => {
      // If first navigation fails, try once more after a short delay
      if (!success) {
        setTimeout(() => {
          this.router.navigate(['/dealer/shop/checkout']);
        }, 10);
      }
    });
  }

  continueShopping(): void {
    this.router.navigate(['/dealer/shop/products']);
  }

  canProceedToCheckout(): boolean {
    // Check if cart has items and all items have valid quantities
    if (this.cartItems.length === 0) {
      return false;
    }

    // Ensure all items have valid quantities greater than 0
    return this.cartItems.every(item => item.quantity > 0);
  }

  getCheckoutDisabledMessage(): string {
    if (this.cartItems.length === 0) {
      return 'Add products to your cart to enable checkout';
    }

    const invalidItems = this.cartItems.filter(item => item.quantity <= 0);
    if (invalidItems.length > 0) {
      return 'Some items have invalid quantities. Please update them.';
    }

    return '';
  }

  // Calculate trucks needed for bag quantities (assuming 500 bags per truck on average)
  calculateTrucksForBags(bags: number): number {
    // Use 500 bags per truck as default, this can be made configurable later
    return Math.ceil(bags / 500);
  }

  // Get display text for total quantity based on unit type
  getTotalQuantityDisplay(item: CartItem): string {
    // For all unit types: show the actual quantity stored in the cart
    // The quantity field already represents the total amount in the specified unit
    return `${item.quantity} ${item.unit}`;
  }

  // Helper methods for unit type checking
  isTruckBasedUnit(unit: string): boolean {
    return unit === 'BAG';
  }

  isDirectInputUnit(unit: string): boolean {
    return ['TON', 'EA', 'TRK', 'PCS'].includes(unit);
  }

  // Increment quantity for direct input units
  incrementQuantity(item: CartItem): void {
    const step = this.getStepValue(item.unit);
    const max = this.getMaxQuantity(item.unit);
    const newQuantity = Math.min(item.quantity + step, max);
    this.updateQuantity(item.id, newQuantity);
  }

  // Decrement quantity for direct input units
  decrementQuantity(item: CartItem): void {
    const step = this.getStepValue(item.unit);
    const min = this.getMinQuantity(item.unit);
    const newQuantity = Math.max(item.quantity - step, min);
    this.updateQuantity(item.id, newQuantity);
  }

}

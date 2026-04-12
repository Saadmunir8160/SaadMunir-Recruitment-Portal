import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { CartItem, CartSummary } from '../models/cart-item.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartItems: CartItem[] = [];
  private cartSubject = new BehaviorSubject<CartItem[]>([]);
  private cartSummarySubject = new BehaviorSubject<CartSummary>(this.calculateSummary());

  constructor() {
    // Clear any potentially corrupted localStorage data first
    if (typeof localStorage !== 'undefined') {
      const savedCart = localStorage.getItem('dealerCart');
      if (savedCart) {
        try {
          const items = JSON.parse(savedCart);
          if (Array.isArray(items)) {
            const totalQuantity = items.reduce((sum, item) => sum + (item.quantity || 0), 0);
            if (totalQuantity > 20) {
              localStorage.removeItem('dealerCart');
            }
          }
        } catch (e) {
          localStorage.removeItem('dealerCart');
        }
      }
    }
    this.loadCartFromStorage();
  }

  getCartItems(): Observable<CartItem[]> {
    return this.cartSubject.asObservable();
  }

  getCurrentCartItems(): CartItem[] {
    return this.cartItems;
  }

  getCartSummary(): Observable<CartSummary> {
    return this.cartSummarySubject.asObservable();
  }

  addToCart(item: CartItem): Observable<CartItem>;
  addToCart(product: any, quantity?: number): Observable<CartItem>;
  addToCart(productOrItem: any, quantity: number = 1): Observable<CartItem> {
    let cartItem: CartItem;
    
    // Handle both CartItem object and product with quantity
    if (productOrItem.id && productOrItem.unit && !productOrItem.dealerProductID && !productOrItem.productId) {
      // It's already a CartItem (has id and unit but no product identifiers)
      // Normalize unit to uppercase
      cartItem = {
        ...productOrItem,
        unit: productOrItem.unit.toUpperCase()
      };
    } else {
      // It's a product, create CartItem based on product unit
      const productUnit = (productOrItem.unit || 'BAG').toUpperCase(); // Default to BAG if not specified, normalize to uppercase
      const productId = productOrItem.dealerProductID || productOrItem.productId;
      
      if (!productId) {
        throw new Error('Product must have either dealerProductID or productId');
      }
      
      cartItem = {
        id: productId.toString(),
        name: productOrItem.productName,
        quantity: quantity,
        unit: productUnit, // Required: Unit determines how quantity is measured
        price: productOrItem.price,
        image: productOrItem.productImage,
        specifications: productOrItem.specifications
      };
    }

    const existingItemIndex = this.cartItems.findIndex(item => item.id === cartItem.id);
    
    if (existingItemIndex !== -1) {
      // Update existing item - recalculate bags and tons
      this.cartItems[existingItemIndex].quantity += cartItem.quantity;
      this.recalculateBagsAndTons(this.cartItems[existingItemIndex]);
      cartItem = this.cartItems[existingItemIndex];
    } else {
      // Add new item
      this.recalculateBagsAndTons(cartItem);
      this.cartItems.push(cartItem);
    }

    this.updateCart();
    return of(cartItem);
  }

  updateQuantity(productId: string, quantity: number): void {
    const itemIndex = this.cartItems.findIndex(item => item.id === productId);
    if (itemIndex !== -1) {
      if (quantity <= 0) {
        this.removeFromCart(productId);
      } else {
        this.cartItems[itemIndex].quantity = quantity;
        this.recalculateBagsAndTons(this.cartItems[itemIndex]);
        this.updateCart();
      }
    }
  }



  private recalculateBagsAndTons(item: CartItem): void {
    // Since bags and tons are independent units, we don't need complex calculations
    // The quantity represents the amount in the specified unit
    // This method is kept for compatibility but doesn't do conversions
  }

  private calculateTons(totalBags: number, bagsPerTon: number = 25): number {
    return totalBags / bagsPerTon;
  }

  private calculateTrucks(totalTons: number, tonsPerTruck: number = 10): number {
    return Math.ceil(totalTons / tonsPerTruck);
  }

  removeFromCart(productId: string): void {
    this.cartItems = this.cartItems.filter(item => item.id !== productId);
    this.updateCart();
  }

  clearCart(): void {
    this.cartItems = [];
    this.updateCart();
  }

  private updateCart(): void {
    this.cartSubject.next([...this.cartItems]);
    this.cartSummarySubject.next(this.calculateSummary());
    this.saveCartToStorage();
  }

  private calculateSummary(): CartSummary {
    // Calculate bags and tons separately based on each item's unit
    const totalBags = this.cartItems
      .filter(item => item.unit === 'BAG')
      .reduce((sum, item) => sum + (item.quantity || 0), 0);
      
    const totalTons = this.cartItems
      .filter(item => item.unit === 'TON')
      .reduce((sum, item) => sum + (item.quantity || 0), 0);
      
    const totalTrucks = this.calculateTrucks(totalTons);
    const itemCount = this.cartItems.length; // Count of unique products, not total quantity
    const totalAmount = this.cartItems.reduce((sum, item) => sum + ((item.price || 0) * (item.quantity || 0)), 0);

    return {
      items: [...this.cartItems],
      totalBags,
      totalTons,
      totalTrucks,
      itemCount,
      totalAmount
    };
  }

  private saveCartToStorage(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('dealerCart', JSON.stringify(this.cartItems));
    }
  }

  private loadCartFromStorage(): void {
    if (typeof localStorage !== 'undefined') {
      const savedCart = localStorage.getItem('dealerCart');
      if (savedCart) {
        try {
          const parsed = JSON.parse(savedCart);
          if (Array.isArray(parsed)) {
            this.cartItems = parsed;
            this.updateCart();
          }
        } catch (e) {
          console.warn('Failed to parse cart from localStorage:', e);
          this.cartItems = [];
          this.updateCart();
        }
      } else {
        // No saved cart, ensure BehaviorSubject reflects empty state
        this.updateCart();
      }
    }
  }

  getCartItemCount(): number {
    return this.cartItems.length; // Count of unique products, not total quantity
  }

  getTotalTons(): number {
    return this.calculateSummary().totalTons;
  }

  getTotalBags(): number {
    return this.calculateSummary().totalBags;
  }

  getTotalTrucks(): number {
    return this.calculateSummary().totalTrucks;
  }

  /**
   * Get cart totals formatted for daily limits checking
   */
  getCartTotalsForLimits(): { totalBags: number; totalTons: number } {
    const summary = this.calculateSummary();
    return {
      totalBags: summary.totalBags,
      totalTons: summary.totalTons
    };
  }

  /**
   * Validate if adding a product would exceed daily limits
   */
  canAddProductToCart(product: any, quantity: number, dailyLimits: any): {
    canAdd: boolean;
    wouldExceedBags: boolean;
    wouldExceedTons: boolean;
    errorMessage?: string;
  } {
    const currentTotals = this.getCartTotalsForLimits();
    let wouldExceedBags = false;
    let wouldExceedTons = false;

    if (product.unit === 'TON') {
      // Check tons limit only
      wouldExceedTons = (dailyLimits.usedTodayTons + currentTotals.totalTons + quantity) > dailyLimits.totalLimitTons;
    } else {
      // Check bags limit only (default unit)
      wouldExceedBags = (dailyLimits.usedTodayBags + currentTotals.totalBags + quantity) > dailyLimits.totalLimitBags;
    }

    let errorMessage = '';
    if (wouldExceedBags) {
      const remainingBags = Math.max(0, dailyLimits.totalLimitBags - dailyLimits.usedTodayBags - currentTotals.totalBags);
      errorMessage = `Adding this item would exceed daily bags limit. Remaining: ${remainingBags} bags`;
    } else if (wouldExceedTons) {
      const remainingTons = Math.max(0, dailyLimits.totalLimitTons - dailyLimits.usedTodayTons - currentTotals.totalTons);
      errorMessage = `Adding this item would exceed daily tons limit. Remaining: ${remainingTons.toFixed(2)} tons`;
    }

    return {
      canAdd: !wouldExceedBags && !wouldExceedTons,
      wouldExceedBags,
      wouldExceedTons,
      errorMessage: errorMessage || undefined
    };
  }
}

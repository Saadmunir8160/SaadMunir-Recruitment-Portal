import { Component, EventEmitter, Input, OnDestroy, OnInit, OnChanges, Output, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';

import { DealerProductsService } from '../../services/dealer-products.service';
import { CartService } from '../../services/cart.service';
import { UnitConfigurationService } from '../../services/unit-configuration.service';
import { NotificationService } from '../../services/notification.service';
import { Product } from '../../models/product.model';
import { CartItem } from '../../models/cart-item.model';

@Component({
  selector: 'app-product-search-popup',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './product-search-popup.component.html',
  styleUrls: ['./product-search-popup.component.scss']
})
export class ProductSearchPopupComponent implements OnInit, OnDestroy, OnChanges {
  @Input() isVisible: boolean = false;
  @Output() close = new EventEmitter<void>();
  @Output() productAdded = new EventEmitter<CartItem>();

  @ViewChild('searchInput', { static: false }) searchInput!: ElementRef<HTMLInputElement>;

  // Search and filter properties
  searchTerm: string = '';
  selectedCategory: string = '';
  categories: string[] = [];

  // Products data
  products: Product[] = [];
  availableProducts: Product[] = []; // Filtered products excluding those in cart
  isLoading: boolean = false;
  hasMoreProducts: boolean = false;

  // Pagination
  private currentPage: number = 1;
  private pageSize: number = 20;

  // Search debouncing
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  // Current cart items for checking duplicates
  private currentCartItems: CartItem[] = [];

  constructor(
    private dealerProductsService: DealerProductsService,
    private cartService: CartService,
    private unitConfigService: UnitConfigurationService,
    private notificationService: NotificationService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    // Setup search debouncing
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.performSearch();
    });

    // Load initial data
    this.loadCategories();
    this.loadProducts();

    // Subscribe to cart changes to track what's already in cart
    this.cartService.getCartItems().subscribe(items => {
      this.currentCartItems = items;
      this.filterAvailableProducts(); // Update available products when cart changes
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnChanges(): void {
    // Focus search input when popup becomes visible
    if (this.isVisible && this.searchInput) {
      setTimeout(() => {
        this.searchInput.nativeElement.focus();
      }, 100);
    }
  }

  // Public methods for template
  closePopup(): void {
    this.close.emit();
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchTerm);
  }

  onCategoryChange(): void {
    // Category changes require fresh data from server
    this.loadProducts(true);
  }

  // Filter products to show only those not in cart and match search criteria
  filterAvailableProducts(): void {
    let filteredProducts = this.products.filter(product => 
      !this.isProductInCart(product)
    );

    // Apply additional client-side search filtering
    if (this.searchTerm && this.searchTerm.trim()) {
      const searchLower = this.searchTerm.toLowerCase().trim();
      filteredProducts = filteredProducts.filter(product => {
        const productName = (product.productName || '').toLowerCase();
        const productCode = (product.erpItemCode || product.productCode || '').toLowerCase();
        const lnCode = (product.itemDescription || product.product_LnCode || '').toLowerCase();
        
        return productName.includes(searchLower) || 
               productCode.includes(searchLower) || 
               lnCode.includes(searchLower);
      });
    }

    this.availableProducts = filteredProducts;
  }

  getProductUnit(product: Product): string {
    // Use unit from API response if available
    if (product.unit) {
      return product.unit.toUpperCase();
    }
    // Determine unit based on product properties or default to 'BAG'
    if (product.packagingInfo?.bagsPerTruck) {
      return 'BAG';
    }
    // Could add more logic here based on product category or other properties
    return 'BAG'; // Default unit
  }

  getDefaultQuantity(product: Product): number {
    const unit = this.getProductUnit(product);
    
    if (unit === 'BAG') {
      // For bags products, default should be minimum bags per truck (500 or 600)
      // Use 500 as default since that's the typical default bags per truck
      return 500;
    }
    
    const config = this.unitConfigService.getUnitConfiguration(unit);
    return config?.minValue || 1;
  }

  getShortDescription(product: Product): string {
    const description = product.shortDescription || product.description || 'No description available';
    // Limit description to 100 characters
    if (description.length <= 100) {
      return description;
    }
    return description.substring(0, 97) + '...';
  }

  // Cart management
  isProductInCart(product: Product): boolean {
    return this.currentCartItems.some(item => 
      item.id === product.dealerProductID.toString()
    );
  }

  addProductToCart(product: Product): void {
    if (this.isProductInCart(product)) {
      return; // Already in cart
    }

    const quantity = this.getDefaultQuantity(product);
    const unit = this.getProductUnit(product);

    const cartItem: CartItem = {
      id: product.dealerProductID.toString(),
      name: product.productName,
      quantity: quantity,
      unit: unit,
      price: product.price,
      image: product.productImage,
      specifications: product.specifications
    };

    this.cartService.addToCart(cartItem).subscribe(
      (addedItem) => {
        this.productAdded.emit(addedItem);
        // Show success notification
        const successMsg = this.translate.instant('DEALER.SHOP.NOTIFICATIONS.PRODUCT_ADDED', { productName: product.productName });
        this.notificationService.success(successMsg);
        // Close popup after adding product
        this.closePopup();
      },
      (error) => {
        console.error('Error adding product to cart:', error);
        const errorMsg = this.translate.instant('DEALER.SHOP.NOTIFICATIONS.ADD_FAILED');
        this.notificationService.error(errorMsg);
      }
    );
  }

  // Image handling
  getProductImageUrl(imagePath?: string): string {
    return this.dealerProductsService.getProductImage(imagePath || '');
  }

  // Data loading methods
  private loadCategories(): void {
    this.dealerProductsService.getProductCategories().subscribe(
      (categories) => {
        this.categories = categories;
      },
      (error) => {
        console.error('Error loading categories:', error);
      }
    );
  }

  private loadProducts(reset: boolean = true): void {
    if (reset) {
      this.currentPage = 1;
      this.products = [];
    }

    this.isLoading = true;

    const filter = {
      search: this.searchTerm || undefined,
      category: this.selectedCategory || undefined
    };

    this.dealerProductsService.getProducts(filter, this.currentPage, this.pageSize).subscribe(
      (response) => {
        const newProducts = response.data || response.products || response;
        
        if (reset) {
          this.products = newProducts;
        } else {
          this.products = [...this.products, ...newProducts];
        }

        // Filter available products (excluding those in cart)
        this.filterAvailableProducts();

        // Check if there are more products to load
        this.hasMoreProducts = newProducts.length === this.pageSize;
        
        this.isLoading = false;
      },
      (error) => {
        console.error('Error loading products:', error);
        this.isLoading = false;
      }
    );
  }

  private performSearch(): void {
    // If we have a search term, filter existing products locally for better UX
    if (this.searchTerm && this.searchTerm.trim() && this.products.length > 0) {
      this.filterAvailableProducts();
    } else {
      // Otherwise load fresh data from server
      this.loadProducts(true);
    }
  }

  loadMoreProducts(): void {
    if (!this.hasMoreProducts || this.isLoading) {
      return;
    }

    this.currentPage++;
    this.loadProducts(false);
  }
}
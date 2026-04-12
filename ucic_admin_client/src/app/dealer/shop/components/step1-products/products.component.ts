import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DealerProductsService } from '../../services/dealer-products.service';
import { ProductFilter } from '../../models/product.model';
import { DealerProduct } from '../../models/order.model';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, TranslateModule],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  products: DealerProduct[] = [];
  filteredProducts: DealerProduct[] = [];
  paginatedProducts: DealerProduct[] = [];
  categories: string[] = [];
  loading = false;

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 12; // 12 products per page for shop
  pageSizeOptions: (number | string)[] = [8, 12, 16, 24, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;

  // Filter properties
  searchTerm = '';
  selectedCategory = '';
  sortBy = 'name';
  sortOrder = 'asc';

  constructor(
    private router: Router,
    private dealerProductsService: DealerProductsService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadProducts(): void {
    this.loading = true;
    
    // Fetch all products without filters - we'll apply filtering on frontend
    this.dealerProductsService.getProducts({}, 1, 1000).subscribe({
      next: (response: any) => {
        // Handle paginated response
        if (response && response.data && response.metadata) {
          this.products = response.data;
        } else if (response && response.data) {
          // Fallback for response with data but no metadata
          this.products = response.data;
        } else if (Array.isArray(response)) {
          // Direct array response (fallback)
          this.products = response;
        } else {
          this.products = [];
        }
        
        // Apply filters and pagination on frontend
        this.applyFilters();
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        // Fallback to empty array on error
        this.products = [];
        this.filteredProducts = [];
        this.totalCount = 0;
        this.totalPages = 0;
      }
    });
  }

  loadCategories(): void {
    this.dealerProductsService.getProductCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => {
        // Fallback to empty categories
        this.categories = [];
      }
    });
  }

  onSearchChange(): void {
    this.currentPage = 1; // Reset to first page when searching
    this.applyFilters();
  }

  onCategoryChange(): void {
    this.currentPage = 1; // Reset to first page when filtering
    this.applyFilters();
  }

  onSortChange(): void {
    this.currentPage = 1; // Reset to first page when sorting
    this.applyFilters();
  }

  onPageSizeChange(size: number | string): void {
    this.pageSize = size === 'All' ? 0 : +size;
    this.currentPage = 1;
    this.applyFilters();
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.updatePagination();
  }

  private applyFilters(): void {
    let filtered = [...this.products];

    // Apply search filter
    if (this.searchTerm && this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase().trim();
      filtered = filtered.filter(product => 
        (product.productName && product.productName.toLowerCase().includes(term)) ||
        (product.description && product.description.toLowerCase().includes(term)) ||
        (product.productCode && product.productCode.toLowerCase().includes(term)) ||
        (product.product_LnCode && product.product_LnCode.toLowerCase().includes(term))
      );
    }

    // Apply category filter - skip for now as category is not in the model
    if (this.selectedCategory && this.selectedCategory.trim()) {
      // TODO: Implement category filtering when category field is available
      // For now, categories will be loaded from API but not used for filtering
    }

    // Apply sorting
    filtered.sort((a, b) => {
      let comparison = 0;
      
      switch (this.sortBy) {
        case 'name':
          comparison = (a.productName || '').localeCompare(b.productName || '');
          break;
        case 'newest':
          // Sort by product ID as a proxy for newest (higher ID = newer)
          comparison = (a.dealerProductID || 0) - (b.dealerProductID || 0);
          break;
        case 'price':
          // Note: Add price comparison if price field exists
          // const priceA = a.price || 0;
          // const priceB = b.price || 0;
          // comparison = priceA - priceB;
          comparison = (a.productName || '').localeCompare(b.productName || '');
          break;
        default:
          comparison = (a.productName || '').localeCompare(b.productName || '');
      }
      
      return this.sortOrder === 'desc' ? -comparison : comparison;
    });

    this.filteredProducts = filtered;
    this.updatePagination();
  }

  private updatePagination(): void {
    this.totalCount = this.filteredProducts.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    // Adjust current page if it's beyond the available pages
    if (this.currentPage > this.totalPages) {
      this.currentPage = 1;
    }
    
    // Apply pagination to get current page products
    if (this.pageSize === 0) {
      // Show all products
      this.paginatedProducts = this.filteredProducts;
    } else {
      const startIndex = (this.currentPage - 1) * this.pageSize;
      const endIndex = startIndex + this.pageSize;
      this.paginatedProducts = this.filteredProducts.slice(startIndex, endIndex);
    }
  }



  viewProductDetails(dealerProductID: number): void {
    this.router.navigate(['/dealer/product-details', dealerProductID]).then(success => {
        // If first navigation fails, try once more after a short delay
        if (!success) {
          setTimeout(() => {
            this.router.navigate(['/dealer/product-details', dealerProductID]);
          }, 10);
        }
      });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.sortBy = 'name';
    this.sortOrder = 'asc';
    this.currentPage = 1;
    this.applyFilters();
  }
}

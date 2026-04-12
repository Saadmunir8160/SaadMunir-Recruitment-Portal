import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProductsService } from '../services/products.service';
import { Router } from '@angular/router'; 
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    FormsModule,
    PaginationComponent
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './products-list.component.html',
  styleUrl: './products-list.component.scss'
})
export class ProductsListComponent implements OnInit {
  allProductsList: any[] = [];
  filteredProducts: any[] = [];
  searchText: string = '';

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 10;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;
  loading: boolean = false;
  isServerSidePagination: boolean = false;

  constructor(
    private productservice: ProductsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.getAllProducts();
  }

  getAllProducts(): void {
    this.loading = true;
    let pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.productservice.getallProducts(this.currentPage, pageSizeToSend).subscribe({
      next: (res: any) => {
        if (Array.isArray(res)) {
          this.allProductsList = res;
          this.totalCount = res.length;
          if (this.pageSize === 0) {
            this.totalPages = 1;
            this.currentPage = 1;
          } else {
            this.totalPages = Math.ceil(this.totalCount / this.pageSize);
          }
          this.filteredProducts = res;
        } else {
          const response = res as PaginatedResponse<any>;
          this.allProductsList = response.data;
          if (this.pageSize === 0) {
            this.currentPage = 1;
            this.totalPages = 1;
          } else {
            this.currentPage = response.metadata.currentPage;
            this.totalPages = response.metadata.totalPages;
          }
          this.totalCount = response.metadata.totalCount;
          this.filteredProducts = response.data;
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
      }
    });
  }

  applyFiltersAndPaginate(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to fetch data from server
      this.getAllProducts();
      return;
    }

    // Client-side pagination logic
    let filtered = this.allProductsList;
    if (this.searchText) {
      const search = this.searchText.toLowerCase();
      filtered = filtered.filter(product =>
        product.productName?.toLowerCase().includes(search) ||
        product.sku?.toLowerCase().includes(search) ||
        product.code?.toLowerCase().includes(search) ||
        product.type?.toLowerCase().includes(search)
      );
    }
    
    this.totalCount = filtered.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    if (this.pageSize === 0) {
      this.filteredProducts = filtered;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.filteredProducts = filtered.slice(start, start + this.pageSize);
    }
  }

  filterProducts(): void {
    if (this.isServerSidePagination) {
      this.currentPage = 1;
      this.getAllProducts();
    } else {
      this.currentPage = 1;
      this.applyFiltersAndPaginate();
    }
  }

  onSearch(): void {
    this.currentPage = 1;
    this.getAllProducts();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.getAllProducts();
    }
  }

  onPageSizeChange(size: number | string): void {
    this.pageSize = Number(size);
    this.currentPage = 1;
    this.getAllProducts();
  }

  createProduct(): void {
    this.router.navigate(['/admin/products/create']);
  }

  editUser(product: any): void {
    this.router.navigate(['/admin/products/edit', product.productId]);
  }

  deleteProduct(productId: number): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.productservice.deleteProduct(productId).subscribe({
        next: () => {
          // Refresh the current page after deletion
          this.getAllProducts();
        },
        error: (err) => {
          // console.error('Error deleting product:', err);
        }
      });
    }
  }

  trackByProductId(index: number, product: any): number {
    return product.productId;
  }
}

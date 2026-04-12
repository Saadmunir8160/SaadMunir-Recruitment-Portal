import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { DealerProductsService } from '../../services/dealer-products.service';
import { DealerProductDetails } from '../../models/order.model';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.scss'
})
export class ProductDetailsComponent implements OnInit {
  product: DealerProductDetails | null = null;
  isLoading = true;
  error: string | null = null;
  
  selectedImageIndex = 0;
  showImageModal = false;
  activeTab = 'description';

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private dealerProductsService: DealerProductsService
  ) {}

  ngOnInit(): void {
    const dealerProductID = this.route.snapshot.paramMap.get('id');
    if (dealerProductID) {
      this.loadProduct(parseInt(dealerProductID));
    } else {
      this.error = 'Product ID not found';
      this.isLoading = false;
    }
  }
  
  private loadProduct(dealerProductID: number): void {
    this.isLoading = true;
    this.error = null;
    
    this.dealerProductsService.getProductById(dealerProductID).subscribe({
      next: (product) => {
        this.product = product;
        this.isLoading = false;
      },
      error: (error) => {
        this.error = 'Failed to load product details';
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dealer/products']);
  }

  // Image gallery methods
  selectImage(index: number): void {
    this.selectedImageIndex = index;
  }

  openImageModal(): void {
    this.showImageModal = true;
  }

  closeImageModal(): void {
    this.showImageModal = false;
  }

  // Tab methods
  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }
}

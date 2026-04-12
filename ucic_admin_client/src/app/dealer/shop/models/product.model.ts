export interface Product {
  dealerProductID: number;
  productName: string;
  productCode?: string;
  product_LnCode?: string;
  erpItemCode?: string; // This is the actual product code from backend
  itemDescription?: string; // This is the actual LN code from backend
  description?: string;
  shortDescription?: string;
  price: number;
  stockQuantity: number;
  productImage?: string;
  productImages?: string[]; // Multiple images for gallery
  category?: string;
  unit?: string; // Unit type from backend
  specifications?: ProductSpecification[];
  features?: string[];
  packagingInfo?: PackagingInfo;
  deliveryInfo?: DeliveryInfo;
  isActive: boolean;
  rating?: number;
  reviewCount?: number;
  minimumOrderQuantity?: number;
  createdDate?: Date;
  updatedDate?: Date;
}

export interface ProductSpecification {
  name: string;
  value: string;
  unit?: string;
}

export interface PackagingInfo {
  weightPerBag: number;
  bagsPerTruck: number;
  storageInstructions?: string;
}

export interface DeliveryInfo {
  estimatedDeliveryDays: number;
  deliveryNotes?: string;
  minimumOrderForFreeDelivery?: number;
}

export interface ProductFilter {
  search?: string;
  category?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: 'name' | 'price' | 'newest';
  sortOrder?: 'asc' | 'desc';
}

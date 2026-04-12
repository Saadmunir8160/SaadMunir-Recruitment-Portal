import { CartItem } from './cart-item.model';
import { Driver } from '../../services/driver.service';
import { Vehicle } from '../../services/vehicle.service';
import { Product } from './product.model';

// Re-export for convenience
export type { Driver, Vehicle };

// Extended Product interface for dealer operations - matches database structure
export interface DealerProductDetails {
  dealerProductID: number;
  dealerID?: number;
  productName: string;
  productCode?: string;
  product_LnCode?: string;
  // API fields (uppercase from backend)
  ERPItemCode?: string;      
  ItemDescription?: string;  
  // Lowercase versions for consistency with Product model
  erpItemCode?: string;      
  itemDescription?: string;  
  description?: string;
  isActive?: boolean;
  isDeleted?: boolean;
  createdDate?: string;
  createdBy?: string;
  modifiedDate?: string;
  modifiedBy?: string;
  unit?: string;
}

// Updated interface to match backend API structure with all database columns
export interface CreateDealerOrderRequest {
  dealerID: number;                    // [DealerID] - Required
  customerOrderNumber?: string;        // [CustomerOrderNumber] - Optional
  ln_OrderNumber?: string;             // [Ln_OrderNumber] - Optional  
  portalOrderNumber?: string;          // [PortalOrderNumber] - Optional
  transporterName?: string;            // [TransporterName] - Optional
  orderDate?: string;                  // [OrderDate] - Optional (will be set by backend)
  status?: string;                     // [Status] - Optional (default: 'pending')
  totalAmount?: number;                // [TotalAmount] - Optional (calculated from items)
  driverID?: number;                   // [DriverID] - Optional driver assignment
  areaID?: number;                     // [AreaID] - Required delivery area
  vehicleID?: number;                  // [VehicleID] - Optional vehicle assignment
  specialInstructions?: string;        // NEW: Special delivery instructions
  isActive?: boolean;                  // [IsActive] - Optional (default: true)
  isDeleted?: boolean;                 // [IsDeleted] - Optional (default: false)
  createdBy?: string;                  // [CreatedBy] - Optional (set by backend from auth)
  orderItems: CreateDealerOrderItemDTO[];  // Order items array
}

// Updated order item structure to match backend
export interface CreateDealerOrderItemDTO {
  dealerOrderID: number;         // Required (set to 0 for new orders)
  dealerProductID: number;       // Changed from productId
  product_LnCode?: string;       // Optional product LN code
  productDescription?: string;   // Optional description
  quantity: number;              // Quantity in the specified unit
  unit: string;                  // Required: Unit type (bags, tons, pieces, meters, kg, liters, etc.)
}

// Updated response structure to match backend
export interface CreateDealerOrderResponse {
  success: boolean;
  message: string;
  data: string;                  // Returns order ID as string only
}

// Support interfaces for selectors
export interface DealerAddress {
  id: string;
  dealerId: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isActive: boolean;
  createdAt?: Date;
  // Backend DTO mapping
  addressID?: number;
  dealerID?: number;
}

export interface DealerProduct {
  dealerProductID: number;
  dealerID?: number;
  productName: string;
  productCode?: string;
  product_LnCode?: string;
  // API fields (uppercase from backend)
  ERPItemCode?: string;      
  ItemDescription?: string;  
  // Lowercase versions for consistency with Product model
  erpItemCode?: string;      
  itemDescription?: string;  
  description?: string;
  isActive?: boolean;
  isDeleted?: boolean;
  createdDate?: string;
  createdBy?: string;
  modifiedDate?: string;
  modifiedBy?: string;
  unit?: string;
}

// Daily limits interface for dealer order limits
export interface DealerDailyLimits {
  totalLimitTons: number;
  usedTodayTons: number;
  remainingTodayTons: number;
  currentOrderTons: number;
  totalLimitBags: number;
  usedTodayBags: number;
  remainingTodayBags: number;
  currentOrderBags: number;
}

// Legacy interfaces kept for compatibility (can be removed later)
export interface DealerOrder {
  orderId?: number;
  orderNumber?: string;
  ln_OrderNumber?: string;  // NEW: LN Order Number from database
  dealerId: number;
  dealerName?: string;
  orderDate: Date;
  deliveryDate?: Date;
  status: OrderStatus;
  items: OrderItem[];
  orderSummary: OrderSummary;
  specialInstructions?: string;
}

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;              // Quantity in the specified unit
  unit: 'bags' | 'tons';         // Unit type (bags or tons are independent)
}

export interface OrderSummary {
  totalBags: number;
  totalTons: number;
  totalTrucks: number;
  itemCount: number;
}

export enum OrderStatus {
  PENDING = 'pending',
  CONFIRMED = 'confirmed',
  PROCESSING = 'processing',
  SHIPPED = 'shipped',
  DELIVERED = 'delivered',
  CANCELLED = 'cancelled'
}

// Type alias for compatibility
export type Order = DealerOrder;

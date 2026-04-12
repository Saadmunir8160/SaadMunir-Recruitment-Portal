import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { 
  DealerOrder, 
  CreateDealerOrderRequest, 
  CreateDealerOrderResponse,
  DealerAddress,
  Driver,
  Vehicle,
  DealerProduct 
} from '../models/order.model';
import { environment } from '../../../../environments/environment';

// Backend response wrapper
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  private currentStep = new BehaviorSubject<number>(1);
  private orderData: Partial<CreateDealerOrderRequest> = {};
  private apiUrl = `${environment.apiUrl}`;

  readonly totalSteps = 4;
  readonly steps = [
    { number: 1, name: 'Browse Products', component: 'ProductsComponent', icon: 'shopping_basket' },
    { number: 2, name: 'Product Details', component: 'ProductDetailsComponent', icon: 'info' },
    { number: 3, name: 'Review Cart', component: 'CartComponent', icon: 'shopping_cart' },
    { number: 4, name: 'Checkout', component: 'CheckoutComponent', icon: 'check_circle' }
  ];

  constructor(private http: HttpClient) {
    this.loadOrderDataFromStorage();
  }

  getCurrentStep() {
    return this.currentStep.asObservable();
  }

  getCurrentStepValue(): number {
    return this.currentStep.value;
  }

  nextStep(): boolean {
    const current = this.currentStep.value;
    if (current < this.totalSteps && this.canProceedFromStep(current)) {
      this.currentStep.next(current + 1);
      this.saveOrderDataToStorage();
      return true;
    }
    return false;
  }

  previousStep(): boolean {
    const current = this.currentStep.value;
    if (current > 1) {
      this.currentStep.next(current - 1);
      return true;
    }
    return false;
  }

  goToStep(stepNumber: number): boolean {
    if (stepNumber >= 1 && stepNumber <= this.totalSteps) {
      // Check if all previous steps are completed
      for (let i = 1; i < stepNumber; i++) {
        if (!this.canProceedFromStep(i)) {
          return false;
        }
      }
      this.currentStep.next(stepNumber);
      return true;
    }
    return false;
  }

  canProceedFromStep(step: number): boolean {
    switch (step) {
      case 1: // Products step - check if items are in cart
        return true; // Always can proceed from products
      case 2: // Cart step - check if cart has items
        return true; // Will be validated by cart service
      case 3: // Details step - check if billing and delivery info is complete
        return this.validateOrderDetails();
      case 4: // Confirmation step - check if order is ready
        return this.validateConfirmation();
      default:
        return true;
    }
  }

  saveStepData(step: number, data: any): void {
    switch (step) {
      case 3:
        // Updated to use new structure
        this.orderData.areaID = data.areaID;
        this.orderData.driverID = data.driverID;
        this.orderData.vehicleID = data.vehicleID;
        break;
      case 4:
        // Confirmation data
        break;
      case 5:
        // Order creation response
        break;
    }
    this.saveOrderDataToStorage();
  }

  getStepData(step: number): any {
    switch (step) {
      case 3:
        return {
          addressID: this.orderData.areaID,
          driverID: this.orderData.driverID,
          vehicleID: this.orderData.vehicleID
        };
      default:
        return {};
    }
  }

  getOrderData(): Partial<CreateDealerOrderRequest> {
    return { ...this.orderData };
  }

  resetOrder(): void {
    this.orderData = {};
    this.currentStep.next(1);
    this.clearOrderDataFromStorage();
  }

  setRecentOrder(order: DealerOrder): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('dealerRecentOrder', JSON.stringify(order));
    }
  }

  getRecentOrder(): DealerOrder | null {
    if (typeof localStorage !== 'undefined') {
      const savedOrder = localStorage.getItem('dealerRecentOrder');
      if (savedOrder) {
        return JSON.parse(savedOrder);
      }
    }
    return null;
  }

  clearRecentOrder(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('dealerRecentOrder');
    }
  }

  private validateOrderDetails(): boolean {
    return !!(this.orderData.orderItems && 
             this.orderData.orderItems.length > 0);
  }

  private validateConfirmation(): boolean {
    return this.validateOrderDetails();
  }

  // NEW: API methods for backend integration
  createDealerOrder(order: CreateDealerOrderRequest): Observable<CreateDealerOrderResponse> {
    return this.http.post<CreateDealerOrderResponse>(`${this.apiUrl}/DealerOrder/Create`, order);
  }

  getDealerAddresses(dealerId: number): Observable<DealerAddress[]> {
    return this.http.get<any>(`${this.apiUrl}/DealerShippingAddress/GetMyAddresses`).pipe(
      map(response => {
        if (response.success) {
          return response.data.map((dto: any) => this.mapDtoToAddress(dto));
        }
        throw new Error(response.message || 'Failed to load addresses');
      })
    );
  }

  // New: return system-wide drivers
  getDrivers(): Observable<Driver[]> {
    return this.http.get<any>(`${this.apiUrl}/dealer/drivers?pageNumber=1&pageSize=1000`).pipe(
      map(response => {
        if (response && response.success !== false) {
          return response.data || [];
        }
        throw new Error(response?.message || 'Failed to load drivers');
      })
    );
  }

  // Backwards-compatible alias (keeps existing callers working)
  getDealerDrivers(dealerId: number): Observable<Driver[]> {
    return this.getDrivers();
  }

  // New: return system-wide vehicles
  getVehicles(): Observable<Vehicle[]> {
    return this.http.get<any>(`${this.apiUrl}/dealer/vehicles?pageNumber=1&pageSize=1000`).pipe(
      map(response => {
        if (response && response.success !== false) {
          return response.data || [];
        }
        throw new Error(response?.message || 'Failed to load vehicles');
      })
    );
  }

  // Backwards-compatible alias
  getDealerVehicles(dealerId: number): Observable<Vehicle[]> {
    return this.getVehicles();
  }

  getDealerDailyLimits(dealerId: number): Observable<any> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/dealer/daily-limits/${dealerId}`).pipe(
      map(response => {
        if (response.success) {
          return response.data;
        }
        throw new Error(response.message || 'Failed to load daily limits');
      })
    );
  }

  getDealerProducts(dealerId: number): Observable<DealerProduct[]> {
    return this.http.get<DealerProduct[]>(`${this.apiUrl}/dealer/products?dealerId=${dealerId}`);
  }

  private saveOrderDataToStorage(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('dealerOrderData', JSON.stringify(this.orderData));
      localStorage.setItem('dealerCurrentStep', this.currentStep.value.toString());
    }
  }

  private loadOrderDataFromStorage(): void {
    if (typeof localStorage !== 'undefined') {
      const savedOrderData = localStorage.getItem('dealerOrderData');
      const savedStep = localStorage.getItem('dealerCurrentStep');
      
      if (savedOrderData) {
        this.orderData = JSON.parse(savedOrderData);
      }
      
      if (savedStep) {
        const step = parseInt(savedStep, 10);
        if (step >= 1 && step <= this.totalSteps) {
          this.currentStep.next(step);
        }
      }
    }
  }

  private clearOrderDataFromStorage(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('dealerOrderData');
      localStorage.removeItem('dealerCurrentStep');
    }
  }

  // Map backend DTO to frontend interface
  private mapDtoToAddress(dto: any): DealerAddress {
    return {
      id: dto.addressID?.toString() || dto.id?.toString() || '',
      dealerId: dto.dealerID?.toString() || dto.dealerId?.toString() || '',
      addressLine1: dto.addressLine1 || '',
      addressLine2: dto.addressLine2 || '',
      city: dto.city || '',
      state: dto.state || '',
      postalCode: dto.postalCode || '',
      country: dto.country || '',
      isActive: dto.isActive || false,
      createdAt: dto.createdDate ? new Date(dto.createdDate) : new Date(),
      // Keep original DTO fields
      addressID: dto.addressID,
      dealerID: dto.dealerID
    };
  }
}

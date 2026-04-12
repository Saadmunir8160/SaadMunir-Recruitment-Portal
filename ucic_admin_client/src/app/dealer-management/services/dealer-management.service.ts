import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

// Interfaces for API responses
export interface DealerProduct {
  dealerProductID: number;
  dealerID: number;
  productID: number;
  productName?: string;
  productDescription?: string;
  pricePerUnit: number;
  availableQuantity: number;
  unitOfMeasure?: string;
  product_LnCode?: string;
  isActive: boolean;
  createdDate: Date;
  modifiedDate: Date;
  dealerName?: string;
  categoryName?: string;
}

export interface DealerOrder {
  dealerOrderID: number;
  dealerID: number;
  dealerName?: string;
  dealerLN_ID?: string;
  customerOrderNumber?: string;
  ln_OrderNumber?: string;  // NEW: LN Order Number from database
  portalOrderNumber?: string;
  transporterName?: string;
  orderDate: Date;
  status: string;
  totalAmount: number;
  driverID?: number;
  driverName?: string;
  vehicleID?: number;
  vehicleName?: string;
  addressID?: number;
  addressName?: string;
  areaID?: number;
  areaName?: string;
  areaCode?: string;
  isActive: boolean;
  createdDate: Date;
  modifiedDate: Date;
  orderItems?: DealerOrderItem[];
}

export interface DealerOrderItem {
  orderItemID: number;
  dealerOrderID: number;
  dealerProductID: number;
  productName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  notes?: string;
}

export interface DealerDriver {
  driverID: number;
  dealerID: number;
  dealerName?: string;
  driverName: string;
  ln_ID?: string;
  iqamaNumber?: string;
  phoneNumber: string;
  email?: string;
  isActive: boolean;
  createdDate: Date;
  modifiedDate: Date;
}

export interface DealerVehicle {
  vehicleID: number;
  dealerID: number;
  dealerName?: string;
  vehicleName: string;
  licensePlate: string;
  vehicleType: string;
  capacity: number;
  isActive: boolean;
  createdDate: Date;
  modifiedDate: Date;
  driverID?: number;
  driverName?: string;
}

export interface Dealer {
  dealerId: number;
  userId: string;
  dealerName: string;
  email?: string;
  phoneNumber?: string;
  fullName?: string;
  userName?: string;
  creditLimit?: number;
  currentBalance?: number;
  ln_ID?: string;
  isActive: boolean;
  createdDate?: Date;
  modifiedDate?: Date;
}

export interface DealerArea {
  areaID: number;
  areaName: string;
  areaCode: string;
  isActive: boolean;
  createdDate: Date;
  modifiedDate: Date;
  orderCount?: number;
}

export interface DealerDailyLimit {
  dailyLimitID: number;
  dealerID: number | null;
  dealerName?: string;
  limitType: string;
  limitValue: number;
  effectiveDate: Date;
  isActive: boolean;
  createdDate: Date;
  updatedDate: Date;
}

// Using shared PaginatedResponse interface

export interface ServiceResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface DeliveryDTO {
  deliveryID: number;
  lnOrderNumber: string;
  customerOrder: string;
  iqn: string;
  internalSalesRepresentative: string;
  quantityShipped: number;
  itemDescription: string;
  dateOut: Date;
  dateIN: Date;
  weightIN: number;
  weightOut: number;
  productionOrder: string;
  item: string;
  line: string;
  shipment: string;
  shipmentLine: string;
  warehouseDescription: string;
  driverName: string;
  car: string;
  deliveryMeans: string;
  customerName: string;
  transporterName: string;
  area: string;
  areaDescription: string;
  isActive: boolean;
  isDeleted: boolean;
  createdDate: Date;
  createdBy: string;
  modifiedDate: Date;
  modifiedBy: string;
}

@Injectable({
  providedIn: 'root'
})
export class DealerManagementService {
  private readonly baseUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) { }

  // Get All Dealers
  getAllDealers(pageNumber: number = 1, pageSize: number = 10, searchTerm?: string, isActive?: boolean): Observable<PaginatedResponse<Dealer>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (isActive !== undefined && isActive !== null) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<PaginatedResponse<Dealer>>(`${this.baseUrl}/Dealer/GetAll`, { params });
  }

  // Get All Dealers for Dropdown (returns array directly)
  getAllDealersForDropdown(): Observable<Dealer[]> {
    let params = new HttpParams()
      .set('pageNumber', '1')
      .set('pageSize', '1000'); // Get a large number for dropdown

    return this.http.get<PaginatedResponse<Dealer>>(`${this.baseUrl}/Dealer/GetAll`, { params })
      .pipe(
        map((response: PaginatedResponse<Dealer>) => response.data || [])
      );
  }

  // Create Dealer User
  createDealerUser(request: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/DealerUser/Create`, request);
  }

  // Update Dealer User
  updateDealerUser(dealerId: number, request: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/DealerUser/Update/${dealerId}`, request);
  }

  // Delete Dealer User
  deleteDealer(dealerId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/DealerUser/Delete/${dealerId}`);
  }

  // Create Dealer Product
  createDealerProduct(request: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Dealer/Products/Create`, request);
  }

  // Update Dealer Product  
  updateDealerProduct(id: number, request: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/Dealer/Products/Update/${id}`, request);
  }

  // Admin Dealer Products Methods
  getDealerProducts(pageNumber: number = 1, pageSize: number = 10, search?: string, dealerId?: number, isActive?: boolean): Observable<PaginatedResponse<DealerProduct>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }

    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<PaginatedResponse<DealerProduct>>(`${this.baseUrl}/Admin/DealerManagement/Products`, { params });
  }

  getDealerProductById(productId: number): Observable<DealerProduct> {
    return this.http.get<DealerProduct>(`${this.baseUrl}/Admin/DealerManagement/Products/${productId}`);
  }

  getDealerProductsStats(dealerId?: number): Observable<any> {
    let params = new HttpParams();
    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/Products/Stats`, { params });
  }

  // Get Dealer Product By ID (Admin)
  getProductById(productId: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/Products/${productId}`);
  }

  // Delete Dealer Product (Admin)
  deleteDealerProduct(productId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Admin/DealerManagement/Products/${productId}`);
  }

  // Update Dealer Product (Admin)
  updateProduct(productId: number, request: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Products/Update/${productId}`, request);
  }

  // Admin Dealer Orders Methods
  getDealerOrders(pageNumber: number = 1, pageSize: number = 10, search?: string, dealerId?: number, status?: string, startDate?: string, endDate?: string): Observable<PaginatedResponse<DealerOrder>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }

    if (status) {
      params = params.set('status', status);
    }

    if (startDate) {
      params = params.set('startDate', startDate);
    }

    if (endDate) {
      params = params.set('endDate', endDate);
    }

    return this.http.get<PaginatedResponse<DealerOrder>>(`${this.baseUrl}/Admin/DealerManagement/Orders`, { params });
  }

  getDealerOrderById(orderId: number): Observable<DealerOrder> {
    return this.http.get<DealerOrder>(`${this.baseUrl}/Admin/DealerManagement/Orders/${orderId}`);
  }

  resendOrder(customerOrderNumber: string): Observable<ServiceResponse<string>> {
    return this.http.post<ServiceResponse<string>>(
      `${this.baseUrl}/DealerOrder/ResendToExternalApi`,
      { customerOrderNumber }
    );
  }

  getDealerOrdersStats(dealerId?: number): Observable<any> {
    let params = new HttpParams();
    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/Orders/Stats`, { params });
  }

  // Admin Dealer Drivers Methods
  getDealerDrivers(pageNumber: number = 1, pageSize: number = 10, search?: string, dealerId?: number, isActive?: boolean): Observable<PaginatedResponse<DealerDriver>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }

    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<PaginatedResponse<DealerDriver>>(`${this.baseUrl}/Admin/DealerManagement/Drivers`, { params });
  }

  getDealerDriverById(driverId: number): Observable<DealerDriver> {
    return this.http.get<DealerDriver>(`${this.baseUrl}/Admin/DealerManagement/Drivers/${driverId}`);
  }

  getDealerDriversStats(dealerId?: number): Observable<any> {
    let params = new HttpParams();
    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/Drivers/Stats`, { params });
  }

  deleteDealerDriver(driverId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Admin/DealerManagement/Drivers/${driverId}`);
  }

  activateDealerDriver(driverId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Drivers/${driverId}/Activate`, {});
  }

  deactivateDealerDriver(driverId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Drivers/${driverId}/Deactivate`, {});
  }

  updateDealerDriver(driverId: number, driverData: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Drivers/Update/${driverId}`, driverData);
  }

  // Admin Dealer Vehicles Methods
  getDealerVehicles(pageNumber: number = 1, pageSize: number = 10, search?: string, dealerId?: number, isActive?: boolean, vehicleType?: string): Observable<PaginatedResponse<DealerVehicle>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }

    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }

    if (vehicleType) {
      params = params.set('vehicleType', vehicleType);
    }

    return this.http.get<PaginatedResponse<DealerVehicle>>(`${this.baseUrl}/Admin/DealerManagement/Vehicles`, { params });
  }

  getDealerVehicleById(vehicleId: number): Observable<DealerVehicle> {
    return this.http.get<DealerVehicle>(`${this.baseUrl}/Admin/DealerManagement/Vehicles/${vehicleId}`);
  }

  getDealerVehiclesStats(dealerId?: number): Observable<any> {
    let params = new HttpParams();
    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/Vehicles/Stats`, { params });
  }

  deleteDealerVehicle(vehicleId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Admin/DealerManagement/Vehicles/${vehicleId}`);
  }

  activateDealerVehicle(vehicleId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Vehicles/${vehicleId}/Activate`, {});
  }

  deactivateDealerVehicle(vehicleId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Vehicles/${vehicleId}/Deactivate`, {});
  }

  updateDealerVehicle(vehicleId: number, vehicleData: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Vehicles/Update/${vehicleId}`, vehicleData);
  }

  // Admin Dashboard Methods
  getDashboardStats(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/Dashboard/Stats`);
  }

  getDealersSummary(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Admin/DealerManagement/Dealers/Summary`);
  }

  // Admin Dealer Daily Limits Methods
  getDealerDailyLimits(params: any): Observable<any> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber?.toString() || '1')
      .set('pageSize', params.pageSize?.toString() || '10');

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.dealerId !== undefined && params.dealerId !== null) {
      httpParams = httpParams.set('dealerId', params.dealerId.toString());
    }

    if (params.limitType) {
      httpParams = httpParams.set('limitType', params.limitType);
    }

    console.log('API call params:', httpParams.toString()); // Debug log
    return this.http.get<any>(`${this.baseUrl}/Admin/DealerManagement/DailyLimits`, { params: httpParams });
  }

  getDealerDailyLimitById(limitId: number): Observable<DealerDailyLimit> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.get<DealerDailyLimit>(`${this.baseUrl}/Admin/DealerManagement/DailyLimits/${limitId}`);
  }

  createDealerDailyLimit(limitData: any): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.post(`${this.baseUrl}/Admin/DealerManagement/DailyLimits/Create`, limitData);
  }

  updateDealerDailyLimit(limitData: any): Observable<any> {
    const limitId = limitData.dailyLimitID;
    // TODO: This endpoint needs to be created in the backend
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/DailyLimits/Update/${limitId}`, limitData);
  }

  deleteDealerDailyLimit(limitId: number): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.delete(`${this.baseUrl}/Admin/DealerManagement/DailyLimits/Delete/${limitId}`);
  }

  getDealerDailyLimitsStats(dealerId?: number): Observable<any> {
    let params = new HttpParams();
    if (dealerId) {
      params = params.set('dealerId', dealerId.toString());
    }
    // TODO: This endpoint needs to be created in the backend
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/DailyLimits/Stats`, { params });
  }

  // Admin Dealer Addresses Methods
  getDealerAddresses(params: any): Observable<PaginatedResponse<any>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber?.toString() || '1')
      .set('pageSize', params.pageSize?.toString() || '10');

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.dealerId !== undefined && params.dealerId !== null) {
      httpParams = httpParams.set('dealerId', params.dealerId.toString());
    }

    if (params.addressType) {
      httpParams = httpParams.set('addressType', params.addressType);
    }

    // TODO: This endpoint needs to be created in the backend
    return this.http.get<PaginatedResponse<any>>(`${this.baseUrl}/Admin/DealerManagement/Addresses`, { params: httpParams });
  }

  createDealerAddress(addressData: any): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.post(`${this.baseUrl}/Admin/DealerManagement/Addresses/Create`, addressData);
  }

  updateDealerAddress(addressData: any): Observable<any> {
    const addressId = addressData.addressId;
    // TODO: This endpoint needs to be created in the backend
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Addresses/Update/${addressId}`, addressData);
  }

  deleteDealerAddress(addressId: number): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.delete(`${this.baseUrl}/Admin/DealerManagement/Addresses/Delete/${addressId}`);
  }

  setDefaultDealerAddress(addressData: any): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Addresses/SetDefault`, addressData);
  }

  // Admin Dealer Areas Methods
  getDealerAreas(pageNumber: number = 1, pageSize: number = 10, search?: string, isActive?: boolean): Observable<PaginatedResponse<DealerArea>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<PaginatedResponse<DealerArea>>(`${this.baseUrl}/Admin/DealerManagement/Areas`, { params });
  }

  getDealerAreaById(areaId: number): Observable<DealerArea> {
    return this.http.get<DealerArea>(`${this.baseUrl}/Admin/DealerManagement/Areas/${areaId}`);
  }

  createDealerArea(areaData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Admin/DealerManagement/Areas/Create`, areaData);
  }

  updateDealerArea(areaId: number, areaData: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/Areas/Update/${areaId}`, areaData);
  }

  deleteDealerArea(areaId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Admin/DealerManagement/Areas/Delete/${areaId}`);
  }

  getDealerAreasStats(): Observable<any> {
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/Areas/Stats`);
  }

  // Admin Support Tickets Methods
  getSupportTickets(params: any): Observable<PaginatedResponse<any>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber?.toString() || '1')
      .set('pageSize', params.pageSize?.toString() || '10');

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.dealerId !== undefined && params.dealerId !== null) {
      httpParams = httpParams.set('dealerId', params.dealerId.toString());
    }

    if (params.category) {
      httpParams = httpParams.set('category', params.category);
    }

    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }

    if (params.priority) {
      httpParams = httpParams.set('priority', params.priority);
    }

    // TODO: This endpoint needs to be created in the backend
    return this.http.get<PaginatedResponse<any>>(`${this.baseUrl}/Admin/DealerManagement/SupportTickets`, { params: httpParams });
  }

  getSupportTicketById(ticketId: number): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/SupportTickets/${ticketId}`);
  }

  addTicketResponse(responseData: any): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.post(`${this.baseUrl}/Admin/DealerManagement/SupportTickets/AddResponse`, responseData);
  }

  updateTicketStatus(updateData: any): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/SupportTickets/UpdateStatus`, updateData);
  }

  assignTicket(updateData: any): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/SupportTickets/Assign`, updateData);
  }

  updateTicketPriority(updateData: any): Observable<any> {
    // TODO: This endpoint needs to be created in the backend
    return this.http.put(`${this.baseUrl}/Admin/DealerManagement/SupportTickets/UpdatePriority`, updateData);
  }

  exportSupportTickets(params: any): Observable<Blob> {
    let httpParams = new HttpParams();

    if (params.dealerId) {
      httpParams = httpParams.set('dealerId', params.dealerId.toString());
    }

    if (params.category) {
      httpParams = httpParams.set('category', params.category);
    }

    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }

    if (params.priority) {
      httpParams = httpParams.set('priority', params.priority);
    }

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    // TODO: This endpoint needs to be created in the backend
    return this.http.get(`${this.baseUrl}/Admin/DealerManagement/SupportTickets/Export`, {
      params: httpParams,
      responseType: 'blob'
    });
  }

  getDeliveryByLnOrderNumber(lnOrderNumber: string): Observable<ServiceResponse<DeliveryDTO>> {
    return this.http.get<ServiceResponse<DeliveryDTO>>(`${this.baseUrl}/Delivery/${lnOrderNumber}`);
  }
}
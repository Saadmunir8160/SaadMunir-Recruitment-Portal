import { Component, EventEmitter, OnInit, Output, ElementRef, HostListener, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CartService } from '../../services/cart.service';
import { ShopService } from '../../services/shop.service';
import { AreaService, DealerArea } from '../../../../services/area.service';
import { DealerAuthService } from '../../../services/dealer-auth.service';
import { DailyLimitsService } from '../../../services/daily-limits.service';
import { DriverService, CreateDriverRequest } from '../../../services/driver.service';
import { VehicleService, CreateVehicleRequest } from '../../../services/vehicle.service';
import { CartItem, CartSummary } from '../../models/cart-item.model';
import { 
  CreateDealerOrderRequest, 
  CreateDealerOrderItemDTO, 
  CreateDealerOrderResponse,
  DealerDailyLimits,
  Driver,
  Vehicle 
} from '../../models/order.model';

@Component({
  selector: 'app-step4-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule],
  templateUrl: './step4-checkout.component.html',
  styleUrls: ['./step4-checkout.component.scss']
})
export class Step4CheckoutComponent implements OnInit {
  @Output() orderComplete = new EventEmitter<any>();
  @Output() previousStep = new EventEmitter<void>();
  
  // ViewChild references for dropdown inputs
  @ViewChild('areaSearchInput') areaSearchInput!: ElementRef;
  @ViewChild('driverSearchInput') driverSearchInput!: ElementRef;
  @ViewChild('vehicleSearchInput') vehicleSearchInput!: ElementRef;

  checkoutForm!: FormGroup;
  cartSummary: CartSummary = {
    items: [],
    totalBags: 0,
    totalTons: 0,
    totalTrucks: 0,
    itemCount: 0
  };
  
  // Daily order limit data (fetched from database)
  dailyOrderLimit: DealerDailyLimits = {
    totalLimitTons: 0,
    usedTodayTons: 0,
    remainingTodayTons: 0,
    currentOrderTons: 0,
    totalLimitBags: 0,
    usedTodayBags: 0,
    remainingTodayBags: 0,
    currentOrderBags: 0
  };

  
  selectedArea: DealerArea | null = null;
  areas: DealerArea[] = [];
  drivers: Driver[] = [];
  vehicles: Vehicle[] = [];
  dealerId: number = 1;
  isSubmitting = false;
  errorMessage = '';
  Math = Math;
  
  // Loading states
  isLoadingData = true;
  isLoadingAreas = false;
  isLoadingDrivers = false;
  isLoadingVehicles = false;
  isLoadingLimits = false;
  
  // Searchable dropdown states
  isAreaDropdownOpen = false;
  isDriverDropdownOpen = false;
  isVehicleDropdownOpen = false;
  
  // Search terms
  areaSearchTerm = '';
  driverSearchTerm = '';
  vehicleSearchTerm = '';
  
  // Filtered arrays
  filteredAreas: DealerArea[] = [];
  filteredDrivers: Driver[] = [];
  filteredVehicles: Vehicle[] = [];
  
  // Selected items
  selectedDriver: Driver | null = null;
  selectedVehicle: Vehicle | null = null;
  
  // Modal states
  showAddDriverModal = false;
  showAddVehicleModal = false;
  isAddingDriver = false;
  isAddingVehicle = false;
  driverModalError = '';
  vehicleModalError = '';
  
  // Modal forms
  addDriverForm!: FormGroup;
  addVehicleForm!: FormGroup;
  
  constructor(
    private fb: FormBuilder,
    private cartService: CartService,
    private shopService: ShopService,
    private areaService: AreaService,
    private dealerAuthService: DealerAuthService,
    private dailyLimitsService: DailyLimitsService,
    private driverService: DriverService,
    private vehicleService: VehicleService,
    private router: Router,
    private translate: TranslateService,
    private elementRef: ElementRef
  ) {}

  ngOnInit(): void {
    // Subscribe to cart items
        this.cartService.getCartItems().subscribe((items: CartItem[]) => {
          // If cart becomes empty, redirect to products page
          if (items.length === 0) {
            this.router.navigate(['/dealer/shop/products']);
          }
        });
    this.initForm();
    this.initModalForms();
    this.updateTransporterNameValidation(); // Set initial validation state
    this.loadData();
  }

  initForm(): void {
    this.checkoutForm = this.fb.group({
      selectedArea: [null, Validators.required],
      customerOrderNumber: ['', [Validators.required]], // Customer order number is mandatory
      driverID: [''],
      vehicleID: [''],
      transporterName: [''] // Add transporter name field
    });

    // Add validation listeners for driver/vehicle changes
    this.checkoutForm.get('driverID')?.valueChanges.subscribe(() => {
      this.updateTransporterNameValidation();
    });
    
    this.checkoutForm.get('vehicleID')?.valueChanges.subscribe(() => {
      this.updateTransporterNameValidation();
    });

    // Keep component selectedArea in sync with form control value
    this.checkoutForm.get('selectedArea')?.valueChanges.subscribe((val: DealerArea | null) => {
      this.onAreaSelected(val);
    });
  }

  updateTransporterNameValidation(): void {
    const transporterNameControl = this.checkoutForm.get('transporterName');

    if (transporterNameControl) {
      // Transporter name is always required
      transporterNameControl.setValidators([Validators.required]);
      transporterNameControl.updateValueAndValidity();
    }
  }

  loadData(): void {
    this.isLoadingData = true;
    
    // Get dealer ID
    const currentDealer = this.dealerAuthService.getCurrentDealer();
    if (currentDealer) {
      this.dealerId = currentDealer.id;
    }

    // Load cart summary
    this.cartService.getCartSummary().subscribe(summary => {
      this.cartSummary = summary;
      this.calculateOrderValue();
    });

    // Load areas, drivers and vehicles
    this.loadAreas();
    this.loadDriversAndVehicles();
    
    // Load daily order limits
    this.loadDailyOrderLimits();
  }

  loadAreas(): void {
    this.isLoadingAreas = true;
    
    this.areaService.getAreas(1, 100).subscribe({
      next: (response: any) => {
        if (response.success) {
          // Filter out inactive areas - only show active areas
          this.areas = response.data.filter((area: DealerArea) => area.isActive === true);
          this.filteredAreas = [...this.areas];
        }
        this.isLoadingAreas = false;
        this.checkDataLoadingComplete();
      },
      error: (error: any) => {
        console.warn('Failed to load areas:', error);
        this.isLoadingAreas = false;
        this.checkDataLoadingComplete();
      }
    });
  }

  loadDriversAndVehicles(): void {
    this.isLoadingDrivers = true;
    this.isLoadingVehicles = true;
    
    // Load drivers (system-wide)
    this.shopService.getDrivers().subscribe({
      next: (drivers) => {
        // Filter out inactive drivers - only show active drivers
        this.drivers = drivers.filter((driver: Driver) => driver.isActive === true);
        this.filteredDrivers = [...this.drivers];
        this.isLoadingDrivers = false;
        this.checkDataLoadingComplete();
      },
      error: (error) => {
        // Handle driver loading error silently
        this.isLoadingDrivers = false;
        this.checkDataLoadingComplete();
      }
    });

    // Load vehicles
    this.shopService.getVehicles().subscribe({
      next: (vehicles) => {
        // Filter out inactive vehicles - only show active vehicles
        this.vehicles = vehicles.filter((vehicle: Vehicle) => vehicle.isActive === true);
        this.filteredVehicles = [...this.vehicles];
        this.isLoadingVehicles = false;
        this.checkDataLoadingComplete();
      },
      error: (error) => {
        // Handle vehicle loading error silently
        this.isLoadingVehicles = false;
        this.checkDataLoadingComplete();
      }
    });
  }

  loadDailyOrderLimits(): void {
    this.isLoadingLimits = true;
    
    // Use the dedicated DailyLimitsService instead of ShopService
    this.dailyLimitsService.getDealerDailyLimits(this.dealerId).subscribe({
      next: (limits: DealerDailyLimits) => {
        this.dailyOrderLimit.totalLimitTons = limits.totalLimitTons;
        this.dailyOrderLimit.usedTodayTons = limits.usedTodayTons;
        this.dailyOrderLimit.remainingTodayTons = limits.remainingTodayTons;
        this.dailyOrderLimit.totalLimitBags = limits.totalLimitBags;
        this.dailyOrderLimit.usedTodayBags = limits.usedTodayBags;
        this.dailyOrderLimit.remainingTodayBags = limits.remainingTodayBags;
        // Calculate current order values
        this.calculateOrderValue();
        this.isLoadingLimits = false;
        this.checkDataLoadingComplete();
      },
      error: (error) => {
        console.warn('Daily limits API not available, using fallback values:', error);
        // Fallback to default values if API fails
        this.dailyOrderLimit.totalLimitTons = 500;
        this.dailyOrderLimit.usedTodayTons = 125;
        this.dailyOrderLimit.remainingTodayTons = 375;
        this.dailyOrderLimit.totalLimitBags = 10000;
        this.dailyOrderLimit.usedTodayBags = 2500;
        this.dailyOrderLimit.remainingTodayBags = 7500;
        this.calculateOrderValue();
        this.isLoadingLimits = false;
        this.checkDataLoadingComplete();
      }
    });
  }

  calculateOrderValue(): void {
    // Calculate current order quantities in tons and bags
    this.dailyOrderLimit.currentOrderTons = this.cartSummary.totalTons;
    this.dailyOrderLimit.currentOrderBags = this.cartSummary.totalBags;
  }

  checkDataLoadingComplete(): void {
    // Check if all data loading is complete
    if (!this.isLoadingAreas && !this.isLoadingDrivers && !this.isLoadingVehicles && !this.isLoadingLimits) {
      this.isLoadingData = false;
      // Auto-select default values
      this.selectDefaultValues();
    }
  }

  selectDefaultValues(): void {
    // Auto-select area with code 'Z99', fallback to first available area
    if (this.areas.length > 0 && !this.selectedArea) {
      const defaultArea = this.findAreaByCode('Z99') || this.areas[0];
      this.checkoutForm.patchValue({ selectedArea: defaultArea });
      this.onAreaSelected(defaultArea);
    }
    
    // Auto-select driver with IQAMA '999', fallback to first available driver
    if (this.drivers.length > 0 && !this.checkoutForm.get('driverID')?.value) {
      const defaultDriver = this.findDriverByIqama('999') || this.drivers[0];
      this.selectedDriver = defaultDriver;
      this.checkoutForm.patchValue({ driverID: defaultDriver.driverID });
    }
    
    // Auto-select vehicle with plate number '999', fallback to first available vehicle
    if (this.vehicles.length > 0 && !this.checkoutForm.get('vehicleID')?.value) {
      const defaultVehicle = this.findVehicleByPlateNumber('999') || this.vehicles[0];
      this.selectedVehicle = defaultVehicle;
      this.checkoutForm.patchValue({ vehicleID: defaultVehicle.vehicleID });
    }
    
    // Set default transporter name
    if (!this.checkoutForm.get('transporterName')?.value) {
      this.checkoutForm.patchValue({ transporterName: 'Default Transporter' });
    }
  }

  private findAreaByCode(code: string): DealerArea | undefined {
    return this.areas.find(area => area.areaCode?.toLowerCase() === code.toLowerCase());
  }

  private findDriverByIqama(ln_ID: string): Driver | undefined {
    return this.drivers.find(driver => driver.ln_ID === ln_ID);
  }

  private findVehicleByPlateNumber(plateNumber: string): Vehicle | undefined {
    return this.vehicles.find(vehicle => vehicle.plateNumber === plateNumber);
  }

  onAreaSelected(area: DealerArea | null): void {
    this.selectedArea = area;
    // mark touched when user selects an area
    this.checkoutForm.get('selectedArea')?.markAsTouched();
  }

  private getAreaId(): number | undefined {
    if (!this.selectedArea) return undefined;
    return this.selectedArea.areaID;
  }

  canPlaceOrder(): boolean {
    const hasValidArea = this.selectedArea !== null && this.getAreaId() !== undefined;
    const isFormValid = this.checkoutForm?.valid === true;
    const hasItems = this.cartSummary.items.length > 0;
    
    // Validate against daily limits using the service
    if (this.dailyOrderLimit && hasItems) {
      const limitsValidation = this.dailyLimitsService.validateCartAgainstLimits(
        this.cartSummary.items, 
        this.dailyOrderLimit
      );
      
      if (!limitsValidation.isValid) {
        return false;
      }
    }
    
    return isFormValid && hasValidArea && hasItems;
  }

  // Separate method to get validation errors without side effects
  getValidationErrors(): string {
    if (!this.cartSummary.items.length) {
      return this.translate.instant('DEALER.SHOP.CHECKOUT.VALIDATION.CART_EMPTY');
    }
    
    if (!this.selectedArea || this.getAreaId() === undefined) {
      return this.translate.instant('DEALER.SHOP.CHECKOUT.VALIDATION.SELECT_AREA');
    }

    const custOrder = this.checkoutForm.get('customerOrderNumber')?.value;
    if (!custOrder || !custOrder.toString().trim()) {
      return this.translate.instant('DEALER.SHOP.CHECKOUT.VALIDATION.CUSTOMER_ORDER_REQUIRED');
    }
    
    // Check delivery assignment logic
    const driverID = this.checkoutForm.get('driverID')?.value;
    const vehicleID = this.checkoutForm.get('vehicleID')?.value;
    const transporterName = this.checkoutForm.get('transporterName')?.value;
    
    if (!driverID && !vehicleID && !transporterName) {
      return this.translate.instant('DEALER.SHOP.CHECKOUT.VALIDATION.SELECT_DELIVERY_METHOD');
    }
    
    if (!this.checkoutForm?.valid) {
      return this.translate.instant('DEALER.SHOP.CHECKOUT.VALIDATION.FILL_REQUIRED_FIELDS');
    }
    
    // Check daily limits
    if (this.dailyOrderLimit) {
      const limitsValidation = this.dailyLimitsService.validateCartAgainstLimits(
        this.cartSummary.items, 
        this.dailyOrderLimit
      );
      
      // if (!limitsValidation.isValid) {
      //   return limitsValidation.errorMessage || this.translate.instant('DEALER.SHOP.CHECKOUT.VALIDATION.EXCEEDS_LIMITS');
      // }
    }
    
    return '';
  }

  getRemainingTonsAfterOrder(): number {
    return this.dailyOrderLimit.remainingTodayTons - this.dailyOrderLimit.currentOrderTons;
  }

  getRemainingBagsAfterOrder(): number {
    return this.dailyOrderLimit.remainingTodayBags - this.dailyOrderLimit.currentOrderBags;
  }

  placeOrder(): void {
    const validationError = this.getValidationErrors();
    if (validationError || !this.canPlaceOrder()) {
      this.errorMessage = validationError || this.translate.instant('DEALER.SHOP.CHECKOUT.VALIDATION.CHECK_ALL_FIELDS');
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    // Calculate total amount from cart items (currently 0 since dealer products don't have price)
    const totalAmount = 0; // Set to 0 since dealer products don't have pricing yet
    
    // If products get pricing in future, use this:
    // const totalAmount = this.cartSummary.items.reduce((total, item) => {
    //   return total + ((item.price || 0) * item.quantity);
    // }, 0);

    // Create order items
    const orderItems: CreateDealerOrderItemDTO[] = this.cartSummary.items.map(item => ({
      dealerOrderID: 0,
      dealerProductID: parseInt(item.id),
      productDescription: item.name,
      quantity: item.quantity,
      unit: item.unit // Use the item's unit directly
    }));

    // Generate unique order numbers
    const now = new Date();
    const orderDate = now.toISOString();
    const dealerPrefix = this.dealerId.toString().padStart(3, '0');
    
    // Generate portal order number: YYMMDDHH + full DealerID
    const yy = now.getFullYear().toString().slice(-2);
    const mm = (now.getMonth() + 1).toString().padStart(2, '0');
    const dd = now.getDate().toString().padStart(2, '0');
    const hh = now.getHours().toString().padStart(2, '0');
    
    // Portal order number: YYMMDDHH + full dealer ID (no padding, use actual dealer ID)
    const portalOrderNumber = `PRT-${yy}${mm}${dd}${hh}${this.dealerId}`;
    
    // Use custom customer order number if provided, otherwise generate one
    const timestamp = Date.now();
    const randomSuffix = Math.random().toString(36).substr(2, 6).toUpperCase();
    const customOrderNumber = this.checkoutForm.value.customerOrderNumber?.trim();
    const customerOrderNumber = customOrderNumber 
      ? `${customOrderNumber}` 
      : `${dealerPrefix}-${timestamp}-${randomSuffix}`;

    const order: CreateDealerOrderRequest = {
      dealerID: this.dealerId,
      customerOrderNumber: customerOrderNumber,
      portalOrderNumber: portalOrderNumber,
      transporterName: this.checkoutForm.value.transporterName || undefined,
      orderDate: orderDate,
      status: 'pending',
      totalAmount: totalAmount,
      areaID: this.getAreaId(),
      driverID: this.checkoutForm.value.driverID || undefined,
      vehicleID: this.checkoutForm.value.vehicleID || undefined,
      isActive: true,
      isDeleted: false,
      orderItems: orderItems
    };

    this.shopService.createDealerOrder(order).subscribe({
      next: (response: CreateDealerOrderResponse) => {
        if (response.success) {
          // Clear cart
          this.cartService.clearCart();
          
          // Emit order completion
          this.orderComplete.emit({ 
            orderId: response.data,
            orderNumber: customerOrderNumber,
            totalAmount: totalAmount,
            message: this.translate.instant('DEALER.SHOP.CHECKOUT.SUCCESS.ORDER_PLACED')
          });
          
          // Navigate to success page or show success message
          this.router.navigate(['/dealer/orders']);
        } else {
          this.errorMessage = response.message || this.translate.instant('DEALER.SHOP.CHECKOUT.ERROR.FAILED_TO_PLACE');
          this.isSubmitting = false;
        }
      },
      error: (error: any) => {
        // Handle order placement error silently
        this.errorMessage = error.error?.message || this.translate.instant('DEALER.SHOP.CHECKOUT.ERROR.FAILED_TO_PLACE');
        this.isSubmitting = false;
      }
    });
  }

  /** Back button arrow: arrow_forward (->) for English, arrow_back (<-) for Arabic/RTL */
  get backArrowIcon(): string {
    return this.translate.currentLang === 'ar' ? 'arrow_back' : 'arrow_back';
  }

  goBack(): void {
    this.router.navigate(['/dealer/shop/cart']);
  }

  getDriverDisplay(driver: Driver): string {
    const name = driver.fullName || driver.userName || driver.name || this.translate.instant('DEALER.SHOP.CHECKOUT.UNKNOWN_DRIVER');
    return `${name}${driver.ln_ID ? ' (' + driver.ln_ID + ')' : ''}`;
  }

  getVehicleDisplay(vehicle: Vehicle): string {
    return `${vehicle.plateNumber}${vehicle.type ? ' (' + vehicle.type + ')' : ''}`;
  }

  getProductImage(imagePath: string): string {
    if (!imagePath) {
      return 'assets/images/default-product.jpg';
    }
    
    if (imagePath.startsWith('http')) {
      return imagePath;
    }
    
    return `assets/images/${imagePath}`;
  }
  
  // ============================================
  // Searchable Dropdown Methods
  // ============================================
  
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    
    // Check if click is inside specific dropdowns
    const areaDropdown = this.elementRef.nativeElement.querySelector('.area-dropdown');
    const driverDropdown = this.elementRef.nativeElement.querySelector('.driver-dropdown');
    const vehicleDropdown = this.elementRef.nativeElement.querySelector('.vehicle-dropdown');
    
    // Only close dropdowns that were NOT clicked
    if (areaDropdown && !areaDropdown.contains(target)) {
      this.isAreaDropdownOpen = false;
    }
    if (driverDropdown && !driverDropdown.contains(target)) {
      this.isDriverDropdownOpen = false;
    }
    if (vehicleDropdown && !vehicleDropdown.contains(target)) {
      this.isVehicleDropdownOpen = false;
    }
  }
  
  closeAllDropdowns(): void {
    this.isAreaDropdownOpen = false;
    this.isDriverDropdownOpen = false;
    this.isVehicleDropdownOpen = false;
  }
  
  // Area dropdown methods
  toggleAreaDropdown(): void {
    this.isAreaDropdownOpen = !this.isAreaDropdownOpen;
    if (this.isAreaDropdownOpen) {
      this.isDriverDropdownOpen = false;
      this.isVehicleDropdownOpen = false;
      this.filterAreas();
    }
  }
  
  openAreaDropdown(): void {
    this.isAreaDropdownOpen = true;
    this.isDriverDropdownOpen = false;
    this.isVehicleDropdownOpen = false;
    this.filterAreas();
  }
  
  onAreaFocus(): void {
    // When focusing, show the display value in input for editing, or clear if searching
    if (this.selectedArea && !this.isAreaDropdownOpen) {
      this.areaSearchTerm = this.getAreaDisplayValue();
    }
    this.openAreaDropdown();
  }
  
  onAreaSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.areaSearchTerm = input.value;
    this.filterAreas();
    if (!this.isAreaDropdownOpen) {
      this.isAreaDropdownOpen = true;
    }
  }
  
  filterAreas(): void {
    const term = this.areaSearchTerm.toLowerCase().trim();
    // Don't filter if the search term matches the selected area's display value
    const selectedDisplayValue = this.selectedArea ? this.getAreaDisplayValue().toLowerCase() : '';
    if (!term || term === selectedDisplayValue) {
      this.filteredAreas = [...this.areas];
    } else {
      this.filteredAreas = this.areas.filter(area => 
        area.areaName.toLowerCase().includes(term) ||
        area.areaCode.toLowerCase().includes(term)
      );
    }
    // Open dropdown when filtering
    if (!this.isAreaDropdownOpen) {
      this.isAreaDropdownOpen = true;
    }
  }
  
  selectArea(area: DealerArea): void {
    this.selectedArea = area;
    this.checkoutForm.get('selectedArea')?.setValue(area);
    this.checkoutForm.get('selectedArea')?.markAsTouched();
    this.areaSearchTerm = this.getAreaDisplayValue();
    this.isAreaDropdownOpen = false;
  }
  
  clearAreaSelection(): void {
    this.selectedArea = null;
    this.checkoutForm.get('selectedArea')?.setValue(null);
    this.areaSearchTerm = '';
    this.filterAreas();
  }
  
  getAreaDisplayValue(): string {
    if (this.selectedArea) {
      return `${this.selectedArea.areaName} (${this.selectedArea.areaCode})`;
    }
    return '';
  }
  
  // Driver dropdown methods
  toggleDriverDropdown(): void {
    this.isDriverDropdownOpen = !this.isDriverDropdownOpen;
    if (this.isDriverDropdownOpen) {
      this.isAreaDropdownOpen = false;
      this.isVehicleDropdownOpen = false;
      this.filterDrivers();
    }
  }
  
  onDriverFocus(): void {
    // When focusing, show the display value in input for editing
    if (this.selectedDriver && !this.isDriverDropdownOpen) {
      this.driverSearchTerm = this.getDriverDisplayValue();
    }
    this.openDriverDropdown();
  }
  
  openDriverDropdown(): void {
    this.isDriverDropdownOpen = true;
    this.isAreaDropdownOpen = false;
    this.isVehicleDropdownOpen = false;
    this.filterDrivers();
  }
  
  onDriverSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.driverSearchTerm = input.value;
    this.filterDrivers();
    if (!this.isDriverDropdownOpen) {
      this.isDriverDropdownOpen = true;
    }
  }
  
  filterDrivers(): void {
    const term = this.driverSearchTerm.toLowerCase().trim();
    // Don't filter if the search term matches the selected driver's display value
    const selectedDisplayValue = this.selectedDriver ? this.getDriverDisplay(this.selectedDriver).toLowerCase() : '';
    if (!term || term === selectedDisplayValue) {
      this.filteredDrivers = [...this.drivers];
    } else {
      this.filteredDrivers = this.drivers.filter(driver => {
        const name = (driver.fullName || driver.userName || driver.name || '').toLowerCase();
        const lnId = (driver.ln_ID || '').toLowerCase();
        return name.includes(term) || lnId.includes(term);
      });
    }
    // Open dropdown when filtering
    if (!this.isDriverDropdownOpen) {
      this.isDriverDropdownOpen = true;
    }
  }
  
  selectDriver(driver: Driver): void {
    this.selectedDriver = driver;
    this.checkoutForm.get('driverID')?.setValue(driver.driverID);
    this.driverSearchTerm = this.getDriverDisplayValue();
    this.isDriverDropdownOpen = false;
    this.updateTransporterNameValidation();
  }
  
  clearDriverSelection(): void {
    this.selectedDriver = null;
    this.checkoutForm.get('driverID')?.setValue('');
    this.driverSearchTerm = '';
    this.filterDrivers();
    this.updateTransporterNameValidation();
    this.isDriverDropdownOpen = false;
  }
  
  getDriverDisplayValue(): string {
    if (this.selectedDriver) {
      return this.getDriverDisplay(this.selectedDriver);
    }
    return '';
  }
  
  // Vehicle dropdown methods
  toggleVehicleDropdown(): void {
    this.isVehicleDropdownOpen = !this.isVehicleDropdownOpen;
    if (this.isVehicleDropdownOpen) {
      this.isAreaDropdownOpen = false;
      this.isDriverDropdownOpen = false;
      this.filterVehicles();
    }
  }
  
  onVehicleFocus(): void {
    // When focusing, show the display value in input for editing
    if (this.selectedVehicle && !this.isVehicleDropdownOpen) {
      this.vehicleSearchTerm = this.getVehicleDisplayValue();
    }
    this.openVehicleDropdown();
  }
  
  openVehicleDropdown(): void {
    this.isVehicleDropdownOpen = true;
    this.isAreaDropdownOpen = false;
    this.isDriverDropdownOpen = false;
    this.filterVehicles();
  }
  
  onVehicleSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.vehicleSearchTerm = input.value;
    this.filterVehicles();
    if (!this.isVehicleDropdownOpen) {
      this.isVehicleDropdownOpen = true;
    }
  }
  
  filterVehicles(): void {
    const term = this.vehicleSearchTerm.toLowerCase().trim();
    // Don't filter if the search term matches the selected vehicle's display value
    const selectedDisplayValue = this.selectedVehicle ? this.getVehicleDisplay(this.selectedVehicle).toLowerCase() : '';
    if (!term || term === selectedDisplayValue) {
      this.filteredVehicles = [...this.vehicles];
    } else {
      this.filteredVehicles = this.vehicles.filter(vehicle => 
        vehicle.plateNumber.toLowerCase().includes(term) ||
        (vehicle.type && vehicle.type.toLowerCase().includes(term))
      );
    }
    // Open dropdown when filtering
    if (!this.isVehicleDropdownOpen) {
      this.isVehicleDropdownOpen = true;
    }
  }
  
  selectVehicle(vehicle: Vehicle): void {
    this.selectedVehicle = vehicle;
    this.checkoutForm.get('vehicleID')?.setValue(vehicle.vehicleID);
    this.vehicleSearchTerm = this.getVehicleDisplayValue();
    this.isVehicleDropdownOpen = false;
    this.updateTransporterNameValidation();
  }
  
  clearVehicleSelection(): void {
    this.selectedVehicle = null;
    this.checkoutForm.get('vehicleID')?.setValue('');
    this.vehicleSearchTerm = '';
    this.filterVehicles();
    this.updateTransporterNameValidation();
    this.isVehicleDropdownOpen = false;
  }
  
  getVehicleDisplayValue(): string {
    if (this.selectedVehicle) {
      return this.getVehicleDisplay(this.selectedVehicle);
    }
    return '';
  }
  
  // ============================================
  // Modal Methods for Adding Driver/Vehicle
  // ============================================
  
  initModalForms(): void {
    // Driver form - same fields as driver create page
    this.addDriverForm = this.fb.group({
      fullName: ['', [Validators.required]],
      userName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmationPassword: ['', [Validators.required]],
      iqamaNumber: [''],
      ln_ID: ['']
    }, { validators: this.passwordMatchValidator });
    
    // Vehicle form - only 3 fields (plate number, vehicle type, ln_ID)
    this.addVehicleForm = this.fb.group({
      plateNumber: ['', [Validators.required, Validators.maxLength(50)]],
      type: ['', [Validators.required]],
      ln_ID: ['', [Validators.maxLength(50)]]
    });
  }
  
  // Password match validator
  passwordMatchValidator(form: AbstractControl): ValidationErrors | null {
    const password = form.get('password');
    const confirmPassword = form.get('confirmationPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true });
      return { mismatch: true };
    } else {
      if (confirmPassword?.errors?.['mismatch']) {
        delete confirmPassword.errors['mismatch'];
        if (Object.keys(confirmPassword.errors).length === 0) {
          confirmPassword.setErrors(null);
        }
      }
    }
    return null;
  }
  
  // Open Add Driver Modal
  openAddDriverModal(): void {
    // Clear current driver selection when opening modal
    this.clearDriverSelection();
    this.showAddDriverModal = true;
    this.driverModalError = '';
    this.addDriverForm.reset();
  }
  
  // Close Add Driver Modal
  closeAddDriverModal(): void {
    this.showAddDriverModal = false;
    this.driverModalError = '';
    this.addDriverForm.reset();
  }
  
  // Submit Add Driver
  submitAddDriver(): void {
    if (this.addDriverForm.valid) {
      this.isAddingDriver = true;
      this.driverModalError = '';
      
      const driverData: CreateDriverRequest = {
        fullName: this.addDriverForm.value.fullName,
        userName: this.addDriverForm.value.userName,
        email: this.addDriverForm.value.email,
        phone: this.addDriverForm.value.phone,
        password: this.addDriverForm.value.password,
        confirmationPassword: this.addDriverForm.value.confirmationPassword,
        ln_ID: this.addDriverForm.value.ln_ID || undefined,
        iqamaNumber: this.addDriverForm.value.iqamaNumber || undefined
      };
      
      // Store the driver name to find and select after reload
      const newDriverName = this.addDriverForm.value.fullName;
      
      this.driverService.createDriver(driverData).subscribe({
        next: (result: any) => {
          this.isAddingDriver = false;
          this.closeAddDriverModal();
          // Reload drivers list and select the new driver
          this.loadDriversAndSelectNew(newDriverName);
        },
        error: (error: any) => {
          this.isAddingDriver = false;
          this.driverModalError = error.error?.message || 'Failed to create driver';
        }
      });
    } else {
      // Mark all fields as touched
      Object.keys(this.addDriverForm.controls).forEach(key => {
        this.addDriverForm.get(key)?.markAsTouched();
      });
    }
  }
  
  // Load drivers and select the newly created one
  loadDriversAndSelectNew(newDriverName: string): void {
    this.isLoadingDrivers = true;
    
    this.shopService.getDrivers().subscribe({
      next: (drivers) => {
        this.drivers = drivers.filter((driver: Driver) => driver.isActive === true);
        this.filteredDrivers = [...this.drivers];
        this.isLoadingDrivers = false;
        
        // Find and select the newly created driver by name
        const newDriver = this.drivers.find(d => 
          (d.fullName || d.userName || d.name) === newDriverName
        );
        
        if (newDriver) {
          this.selectDriver(newDriver);
        }
      },
      error: (error) => {
        this.isLoadingDrivers = false;
      }
    });
  }
  
  // Open Add Vehicle Modal
  openAddVehicleModal(): void {
    // Clear current vehicle selection when opening modal
    this.clearVehicleSelection();
    this.showAddVehicleModal = true;
    this.vehicleModalError = '';
    this.addVehicleForm.reset();
  }
  
  // Close Add Vehicle Modal
  closeAddVehicleModal(): void {
    this.showAddVehicleModal = false;
    this.vehicleModalError = '';
    this.addVehicleForm.reset();
  }
  
  // Submit Add Vehicle
  submitAddVehicle(): void {
    if (this.addVehicleForm.valid) {
      this.isAddingVehicle = true;
      this.vehicleModalError = '';
      
      // Set dummy values for required fields not shown in popup
      const today = new Date();
      const nextYear = new Date(today.getFullYear() + 1, today.getMonth(), today.getDate());
      
      // Store the plate number to find and select after reload
      const newPlateNumber = this.addVehicleForm.value.plateNumber;
      
      const vehicleData: CreateVehicleRequest = {
        dealerID: this.dealerId,
        plateNumber: newPlateNumber,
        type: this.addVehicleForm.value.type,
        ln_ID: this.addVehicleForm.value.ln_ID || undefined,
        // Dummy values for required fields
        capacity: 20, // Default capacity
        registrationDate: today,
        registrationExpiryDate: nextYear,
        insuranceExpiryDate: nextYear,
        isActive: true
      };
      
      this.vehicleService.createVehicle(vehicleData).subscribe({
        next: (result: any) => {
          this.isAddingVehicle = false;
          this.closeAddVehicleModal();
          // Reload vehicles list and select the new vehicle
          this.loadVehiclesAndSelectNew(newPlateNumber);
        },
        error: (error: any) => {
          this.isAddingVehicle = false;
          this.vehicleModalError = error.error?.message || 'Failed to create vehicle';
        }
      });
    } else {
      // Mark all fields as touched
      Object.keys(this.addVehicleForm.controls).forEach(key => {
        this.addVehicleForm.get(key)?.markAsTouched();
      });
    }
  }
  
  // Load vehicles and select the newly created one
  loadVehiclesAndSelectNew(newPlateNumber: string): void {
    this.isLoadingVehicles = true;
    
    this.shopService.getVehicles().subscribe({
      next: (vehicles) => {
        this.vehicles = vehicles.filter((vehicle: Vehicle) => vehicle.isActive === true);
        this.filteredVehicles = [...this.vehicles];
        this.isLoadingVehicles = false;
        
        // Find and select the newly created vehicle by plate number
        const newVehicle = this.vehicles.find(v => v.plateNumber === newPlateNumber);
        
        if (newVehicle) {
          this.selectVehicle(newVehicle);
        }
      },
      error: (error) => {
        this.isLoadingVehicles = false;
      }
    });
  }
}
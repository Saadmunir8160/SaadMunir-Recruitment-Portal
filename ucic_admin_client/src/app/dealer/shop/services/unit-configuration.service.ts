import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { 
  UnitConfiguration, 
  DEFAULT_UNIT_CONFIGURATIONS, 
  UnitCalculationResult, 
  ProductQuantityState,
  TruckBasedConfig 
} from '../models/unit-configuration.model';
// import { environment } from '../../../../environments/environment'; // Uncomment when ready

@Injectable({
  providedIn: 'root'
})
export class UnitConfigurationService {
  private unitConfigurations = new BehaviorSubject<UnitConfiguration[]>(DEFAULT_UNIT_CONFIGURATIONS);
  private productQuantityStates = new Map<number, ProductQuantityState>();

  constructor(
    private http: HttpClient // Ready for API integration
  ) {
    this.loadUnitConfigurations();
  }

  // Get all available unit configurations
  getUnitConfigurations(): Observable<UnitConfiguration[]> {
    return this.unitConfigurations.asObservable();
  }

  // Get configuration for a specific unit
  getUnitConfiguration(unitType: string): UnitConfiguration | null {
    return this.unitConfigurations.value.find(config => config.unitType === unitType) || null;
  }

  // Check if unit uses truck-based calculation
  isTruckBasedUnit(unitType: string): boolean {
    const config = this.getUnitConfiguration(unitType);
    return config?.calculationType === 'truck-based';
  }

  // Get product quantity state
  getProductQuantityState(productId: number, unit: string): ProductQuantityState {
    const key = productId;
    if (!this.productQuantityStates.has(key)) {
      this.initializeProductQuantityState(productId, unit);
    }
    return this.productQuantityStates.get(key)!;
  }

  // Initialize product quantity state
  private initializeProductQuantityState(productId: number, unit: string): void {
    const config = this.getUnitConfiguration(unit);
    if (!config) return;

    let initialState: ProductQuantityState;

    if (config.calculationType === 'truck-based' && config.truckBasedConfig) {
      // Initialize truck-based state
      const trucks = config.truckBasedConfig.minTrucks;
      const bagsPerTruck = config.truckBasedConfig.defaultBagsPerTruck;
      const calculatedQuantity = trucks * bagsPerTruck;
      
      initialState = {
        productId,
        unit,
        quantity: calculatedQuantity,
        trucks,
        bagsPerTruck,
        calculatedQuantity,
        displayText: `${calculatedQuantity} ${config.displayName}`
      };
    } else {
      // Initialize direct input state
      const quantity = config.defaultValue;
      initialState = {
        productId,
        unit,
        quantity,
        calculatedQuantity: quantity,
        displayText: `${quantity} ${config.displayName}`
      };
    }

    this.productQuantityStates.set(productId, initialState);
  }

  // Update truck-based quantity
  updateTruckBasedQuantity(productId: number, trucks: number, bagsPerTruck?: number): UnitCalculationResult {
    const state = this.productQuantityStates.get(productId);
    if (!state) throw new Error(`Product state not found for product ${productId}`);

    const config = this.getUnitConfiguration(state.unit);
    if (!config?.truckBasedConfig) throw new Error(`Invalid truck-based configuration for unit ${state.unit}`);

    // Use provided bagsPerTruck or keep current value
    const currentBagsPerTruck = bagsPerTruck || state.bagsPerTruck || config.truckBasedConfig.defaultBagsPerTruck;
    
    // Validate values
    const validTrucks = Math.max(
      config.truckBasedConfig.minTrucks,
      Math.min(config.truckBasedConfig.maxTrucks, trucks)
    );

    const calculatedQuantity = validTrucks * currentBagsPerTruck;

    // Update state
    const updatedState: ProductQuantityState = {
      ...state,
      trucks: validTrucks,
      bagsPerTruck: currentBagsPerTruck,
      quantity: calculatedQuantity,
      calculatedQuantity,
      displayText: `${calculatedQuantity} ${config.displayName}`
    };

    this.productQuantityStates.set(productId, updatedState);

    return {
      quantity: calculatedQuantity,
      displayText: updatedState.displayText,
      additionalInfo: {
        trucks: validTrucks,
        bagsPerTruck: currentBagsPerTruck
      }
    };
  }

  // Update direct quantity
  updateDirectQuantity(productId: number, quantity: number): UnitCalculationResult {
    const state = this.productQuantityStates.get(productId);
    if (!state) throw new Error(`Product state not found for product ${productId}`);

    const config = this.getUnitConfiguration(state.unit);
    if (!config) throw new Error(`Configuration not found for unit ${state.unit}`);

    // Validate quantity
    const validQuantity = Math.max(
      config.minValue,
      Math.min(config.maxValue, quantity)
    );

    // Round to appropriate decimal places
    const roundedQuantity = Math.round(validQuantity * Math.pow(10, config.decimalPlaces)) / Math.pow(10, config.decimalPlaces);

    // Update state
    const updatedState: ProductQuantityState = {
      ...state,
      quantity: roundedQuantity,
      calculatedQuantity: roundedQuantity,
      displayText: `${roundedQuantity} ${config.displayName}`
    };

    this.productQuantityStates.set(productId, updatedState);

    return {
      quantity: roundedQuantity,
      displayText: updatedState.displayText
    };
  }

  // Get available bags per truck options for a truck-based unit
  getAvailableBagsPerTruck(unitType: string): number[] {
    const config = this.getUnitConfiguration(unitType);
    return config?.truckBasedConfig?.availableBagsPerTruck || [];
  }

  // Calculate trucks needed for a given quantity (for display purposes)
  calculateTrucksNeeded(unitType: string, quantity: number, bagsPerTruck?: number): number {
    if (!this.isTruckBasedUnit(unitType)) return 0;
    
    const config = this.getUnitConfiguration(unitType);
    const effectiveBagsPerTruck = bagsPerTruck || config?.truckBasedConfig?.defaultBagsPerTruck || 500;
    
    return Math.ceil(quantity / effectiveBagsPerTruck);
  }

  // Get display configuration for form controls
  getFormControlConfig(unitType: string) {
    const config = this.getUnitConfiguration(unitType);
    if (!config) return null;

    return {
      type: config.calculationType === 'direct' ? 'number' : 'truck-controls',
      min: config.minValue,
      max: config.maxValue,
      step: config.stepValue,
      decimalPlaces: config.decimalPlaces,
      placeholder: `Enter ${config.displayName.toLowerCase()}`,
      unit: config.displayName
    };
  }

  // Reset product quantity state
  resetProductQuantityState(productId: number, unit: string): void {
    this.productQuantityStates.delete(productId);
    this.initializeProductQuantityState(productId, unit);
  }

  // Get summary of all quantities by unit type
  getQuantitySummaryByUnit(productStates: ProductQuantityState[]): { [unitType: string]: number } {
    const summary: { [unitType: string]: number } = {};
    
    productStates.forEach(state => {
      if (!summary[state.unit]) {
        summary[state.unit] = 0;
      }
      summary[state.unit] += state.calculatedQuantity;
    });

    return summary;
  }

  // Load unit configurations from API/database
  private loadUnitConfigurations(): void {
    // For now, use default configurations
    // TODO: Uncomment when backend is ready
    /*
    this.http.get<UnitConfiguration[]>('/api/unit-configurations').subscribe({
      next: (configs) => {
        this.unitConfigurations.next(configs);
      },
      error: (error) => {
        console.error('Failed to load unit configurations from API, using defaults', error);
        // Fallback to default configurations
        this.unitConfigurations.next(DEFAULT_UNIT_CONFIGURATIONS);
      }
    });
    */
    
    // Use default configurations for now
    this.unitConfigurations.next(DEFAULT_UNIT_CONFIGURATIONS);
  }

  // Future: Add new unit configuration
  addUnitConfiguration(config: UnitConfiguration): void {
    const current = this.unitConfigurations.value;
    const updated = [...current, config];
    this.unitConfigurations.next(updated);
    
    // In the future, this would also save to the backend
    // this.http.post('/api/unit-configurations', config).subscribe(...)
  }

  // Future: Update existing unit configuration
  updateUnitConfiguration(unitType: string, updates: Partial<UnitConfiguration>): void {
    const current = this.unitConfigurations.value;
    const updated = current.map(config => 
      config.unitType === unitType ? { ...config, ...updates } : config
    );
    this.unitConfigurations.next(updated);
  }
}
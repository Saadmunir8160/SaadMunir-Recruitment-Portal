export interface UnitConfiguration {
  unitType: string;
  displayName: string;
  calculationType: 'truck-based' | 'direct' | 'custom';
  decimalPlaces: number;
  minValue: number;
  maxValue: number;
  stepValue: number;
  defaultValue: number;
  
  // Truck-based configuration (for bags)
  truckBasedConfig?: TruckBasedConfig;
  
  // Custom calculation configuration
  customConfig?: CustomUnitConfig;
}

export interface TruckBasedConfig {
  availableBagsPerTruck: number[];  // e.g., [500, 600]
  defaultBagsPerTruck: number;      // e.g., 500
  maxTrucks: number;                // e.g., 100
  minTrucks: number;                // e.g., 1
}

export interface CustomUnitConfig {
  calculationFunction?: string;     // For future extensibility
  conversionRates?: { [key: string]: number };
  additionalProperties?: { [key: string]: any };
}

export interface UnitCalculationResult {
  quantity: number;
  displayText: string;
  additionalInfo?: {
    trucks?: number;
    bagsPerTruck?: number;
    equivalentUnits?: { [unitType: string]: number };
  };
}

// Pre-defined unit configurations
export const DEFAULT_UNIT_CONFIGURATIONS: UnitConfiguration[] = [
  {
    unitType: 'BAG',
    displayName: 'Bags',
    calculationType: 'truck-based',
    decimalPlaces: 0,
    minValue: 1,
    maxValue: 50000,
    stepValue: 1,
    defaultValue: 500,
    truckBasedConfig: {
      availableBagsPerTruck: [500, 600],
      defaultBagsPerTruck: 500,
      maxTrucks: 100,
      minTrucks: 1
    }
  },
  {
    unitType: 'TON',
    displayName: 'Tons',
    calculationType: 'direct',
    decimalPlaces: 2,
    minValue: 0.01,
    maxValue: 10000,
    stepValue: 0.01,
    defaultValue: 1
  },
  {
    unitType: 'EA',
    displayName: 'Each',
    calculationType: 'direct',
    decimalPlaces: 0,
    minValue: 1,
    maxValue: 100000,
    stepValue: 1,
    defaultValue: 1
  },
  {
    unitType: 'TRK',
    displayName: 'Truck',
    calculationType: 'direct',
    decimalPlaces: 0,
    minValue: 1,
    maxValue: 100,
    stepValue: 1,
    defaultValue: 1
  },
  {
    unitType: 'PCS',
    displayName: 'Pieces',
    calculationType: 'direct',
    decimalPlaces: 0,
    minValue: 1,
    maxValue: 100000,
    stepValue: 1,
    defaultValue: 1
  }
];

export interface ProductQuantityState {
  productId: number;
  unit: string;
  quantity: number;
  
  // For truck-based units
  trucks?: number;
  bagsPerTruck?: number;
  
  // Calculated values
  calculatedQuantity: number;
  displayText: string;
}
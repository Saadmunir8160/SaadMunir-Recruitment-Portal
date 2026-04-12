export interface CartItem {
  id: string;
  name: string;
  quantity: number;    // The actual quantity in the specified unit
  unit: string;        // Required: Unit type from product (BAG, TON, EA, TRK, PCS)
  price?: number;      // Price per unit (optional - dealer products don't have pricing yet)
  image?: string;
  specifications?: any;
}

export interface CartSummary {
  items: CartItem[];
  totalBags: number;      // Sum of all items where unit = 'bags'
  totalTons: number;      // Sum of all items where unit = 'tons'
  totalTrucks: number;    // Estimated trucks needed (can be calculated based on business rules)
  itemCount: number;      // Number of different products
  totalAmount?: number;   // Total price of all items
}

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

export interface CreateAddressRequest {
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isActive?: boolean;
}

export interface UpdateAddressRequest extends CreateAddressRequest {
  id: string;
}
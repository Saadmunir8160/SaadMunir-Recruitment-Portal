export interface DealerInfo {
  dealerId: number;
  dealerCode: string;
  companyName: string;
  contactPerson: string;
  email: string;
  phone: string;
  address: DealerAddress;
  status: DealerStatus;
  registrationDate: Date;
  creditLimit?: number;
  outstandingAmount?: number;
}

export interface DealerAddress {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

export enum DealerStatus {
  ACTIVE = 'active',
  INACTIVE = 'inactive',
  SUSPENDED = 'suspended',
  PENDING_APPROVAL = 'pending_approval'
}

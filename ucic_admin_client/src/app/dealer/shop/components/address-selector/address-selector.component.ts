import { Component, forwardRef, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectorRef, ViewChild, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DealerAddressService } from '../../../services/dealer-address.service';
import { DealerAddress } from '../../models/order.model';

@Component({
  selector: 'app-address-selector',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AddressSelectorComponent),
      multi: true
    }
  ],
  templateUrl: './address-selector.component.html',
  styleUrls: ['./address-selector.component.scss']
})
export class AddressSelectorComponent implements ControlValueAccessor, OnInit, OnDestroy {
  @Output() addressSelected = new EventEmitter<DealerAddress | null>();
  @Output() loadingStateChanged = new EventEmitter<boolean>();
  @ViewChild('searchInput', { static: false }) searchInput!: ElementRef<HTMLInputElement>;
  
  addresses: DealerAddress[] = [];
  filteredAddresses: DealerAddress[] = [];
  addressControl = new FormControl('');
  selectedAddress: DealerAddress | null = null;
  showNewAddressForm = false;
  isCreating = false;
  isLoadingAddresses = true;
  
  // Search functionality
  isDropdownOpen = false;
  searchTerm = '';
  displayValue = '';
  
  // Store the unique signature of the address being created
  private pendingAddressSignature: string | null = null;
  
  newAddressForm = new FormGroup({
    addressLine1: new FormControl('', { nonNullable: true }),
    addressLine2: new FormControl('', { nonNullable: true }),
    city: new FormControl('', { nonNullable: true }),
    state: new FormControl('', { nonNullable: true }),
    postalCode: new FormControl('', { nonNullable: true }),
    country: new FormControl('Saudi Arabia', { nonNullable: true }),
    isActive: new FormControl(true, { nonNullable: true })
  });

  private onChange = (value: DealerAddress | null) => {};
  private onTouched = () => {};

  constructor(
    private addressService: DealerAddressService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadAddresses();
  }

  ngOnDestroy() {
    // Clear any pending signature
    this.pendingAddressSignature = null;
  }

  writeValue(value: DealerAddress | null): void {
    if (value) {
      this.selectedAddress = value;
      this.addressControl.setValue(value.id);
      this.updateDisplayValue();
    } else {
      this.selectedAddress = null;
      this.displayValue = '';
    }
  }

  registerOnChange(fn: (value: DealerAddress | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  private loadAddresses(): void {
    this.isLoadingAddresses = true;
    this.loadingStateChanged.emit(true);
    
    this.addressService.getAddresses().subscribe({
      next: (addresses: DealerAddress[]) => {
        this.addresses = addresses;
        this.filteredAddresses = [...addresses];
        
        // If there's a pre-selected address in the form control, find and set it
        const currentValue = this.addressControl.value;
        if (currentValue) {
          const selectedAddress = this.addresses.find(addr => addr.id === currentValue);
          if (selectedAddress) {
            this.selectedAddress = selectedAddress;
            this.updateDisplayValue();
          }
        }
        
        // Trigger change detection
        this.cdr.detectChanges();
        
        // Don't auto-select any address - let user choose explicitly
        // The first address can be marked as default but not auto-selected
        this.isLoadingAddresses = false;
        this.loadingStateChanged.emit(false);
      },
      error: (error: any) => {
        // Handle error silently or show user-friendly message
        this.isLoadingAddresses = false;
        this.loadingStateChanged.emit(false);
      }
    });
  }

  private setSelectedAddressById(addressId: string): void {
    const address = this.addresses.find(a => a.id === addressId);
    if (address) {
      this.selectedAddress = address;
      this.addressControl.setValue(address.id);
      this.onChange(address);
      this.addressSelected.emit(address);
    }
  }

  private setSelectedAddress(address: DealerAddress): void {
    this.selectedAddress = address;
    this.addressControl.setValue(address.id);
    this.onChange(address);
    this.addressSelected.emit(address);
  }

  onAddressChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const value = target.value;
    this.onAddressChangeInternal(value);
  }

  private onAddressChangeInternal(value: string): void {
    if (value) {
      const address = this.addresses.find(a => a.id === value);
      this.selectedAddress = address || null;
      this.showNewAddressForm = false;
      this.onChange(this.selectedAddress);
      this.addressSelected.emit(this.selectedAddress);
    } else {
      this.selectedAddress = null;
      this.showNewAddressForm = false;
      this.onChange(null);
      this.addressSelected.emit(null);
    }
    
    this.onTouched();
  }

  // Search functionality methods
  onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm = target.value;
    this.filterAddresses();
    
    if (!this.isDropdownOpen) {
      this.openDropdown();
    }
  }

  openDropdown(): void {
    this.isDropdownOpen = true;
    this.filterAddresses();
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
    this.updateDisplayValue();
  }

  toggleDropdown(): void {
    if (this.isDropdownOpen) {
      this.closeDropdown();
    } else {
      this.openDropdown();
    }
  }

  onBlur(): void {
    // Delay closing to allow for selection
    setTimeout(() => {
      if (!this.isDropdownOpen) return; // Don't close if already closed
      this.closeDropdown();
    }, 200);
  }

  selectAddress(address: DealerAddress): void {
    this.selectedAddress = address;
    this.searchTerm = '';
    this.updateDisplayValue();
    this.closeDropdown();
    
    this.addressControl.setValue(address.id);
    this.onChange(address);
    this.addressSelected.emit(address);
    this.onTouched();
    
    this.showNewAddressForm = false;
  }

  clearSelection(): void {
    this.selectedAddress = null;
    this.searchTerm = '';
    this.displayValue = '';
    this.addressControl.setValue('');
    this.onChange(null);
    this.addressSelected.emit(null);
    this.onTouched();
    
    if (this.searchInput?.nativeElement) {
      this.searchInput.nativeElement.focus();
    }
  }

  private filterAddresses(): void {
    if (!this.addresses) {
      this.filteredAddresses = [];
      return;
    }
    
    if (!this.searchTerm.trim()) {
      this.filteredAddresses = [...this.addresses];
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredAddresses = this.addresses.filter(address => 
        address.addressLine1?.toLowerCase().includes(term) ||
        (address.addressLine2 && address.addressLine2.toLowerCase().includes(term)) ||
        address.city?.toLowerCase().includes(term) ||
        address.state?.toLowerCase().includes(term) ||
        address.postalCode?.toLowerCase().includes(term)
      );
    }
  }

  private updateDisplayValue(): void {
    if (this.selectedAddress) {
      this.displayValue = `${this.selectedAddress.addressLine1}${this.selectedAddress.addressLine2 ? ', ' + this.selectedAddress.addressLine2 : ''} - ${this.selectedAddress.city}`;
    } else {
      this.displayValue = '';
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    if (!this.isDropdownOpen) {
      if (event.key === 'Enter' || event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        event.preventDefault();
        this.openDropdown();
      }
      return;
    }

    switch (event.key) {
      case 'Escape':
        event.preventDefault();
        this.closeDropdown();
        break;
      case 'Enter':
        event.preventDefault();
        if (this.filteredAddresses.length > 0) {
          this.selectAddress(this.filteredAddresses[0]);
        }
        break;
      case 'ArrowDown':
      case 'ArrowUp':
        event.preventDefault();
        // Basic implementation - could be enhanced with highlight navigation
        break;
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    const dropdown = target.closest('.searchable-dropdown');
    if (!dropdown) {
      this.closeDropdown();
    }
  }

  private createAddressSignature(addressData: any): string {
    // Create a unique signature based on form data + timestamp
    const signature = `${addressData.addressLine1?.trim().toLowerCase()}_${addressData.city?.trim().toLowerCase()}_${addressData.postalCode?.trim()}_${Date.now()}`;
    return signature;
  }

  private findAddressBySignature(signature: string): DealerAddress | null {
    const parts = signature.split('_');
    const addressLine1 = parts[0];
    const city = parts[1];
    const postalCode = parts[2];
    
    // Find address by matching the key components
    return this.addresses.find(a => 
      a.addressLine1?.trim().toLowerCase() === addressLine1 &&
      a.city?.trim().toLowerCase() === city &&
      a.postalCode?.trim() === postalCode
    ) || null;
  }

  createNewAddress(): void {
    if (this.newAddressForm.valid) {
      this.isCreating = true;
      
      // Create unique signature for the address being created
      const addressData = this.newAddressForm.value;
      this.pendingAddressSignature = this.createAddressSignature(addressData);
      
      
      this.addressService.createAddress(addressData as any).subscribe({
        next: (newAddress: DealerAddress) => {
          
          // Reload addresses to ensure we have the updated list
          this.addressService.getAddresses().subscribe({
            next: (addresses: DealerAddress[]) => {
              this.addresses = addresses;
              
              // Find the address using our signature
              let createdAddress = null;
              if (this.pendingAddressSignature) {
                createdAddress = this.findAddressBySignature(this.pendingAddressSignature);
              }
              
              // If signature matching fails, fall back to the original methods
              if (!createdAddress) {
                
                // Try ID match first
                if (newAddress.id) {
                  createdAddress = this.addresses.find(a => a.id === newAddress.id);
                }
                
                // Then try exact address line match
                if (!createdAddress) {
                  createdAddress = this.addresses.find(a => 
                    a.addressLine1 === newAddress.addressLine1 && 
                    a.city === newAddress.city
                  );
                }
                
                // Finally, use the returned address as fallback
                if (!createdAddress) {
                  this.addresses.push(newAddress);
                  createdAddress = newAddress;
                }
              }
              
              // Clear the pending signature
              this.pendingAddressSignature = null;
              
              // Now select the found address
              if (createdAddress) {
                setTimeout(() => {
                  this.selectedAddress = createdAddress;
                  
                  // Ensure the value is a string (as HTML select expects string values)
                  const addressId = String(createdAddress.id);
                  
                  // Set the form control value
                  this.addressControl.setValue(addressId);
                  this.addressControl.patchValue(addressId);
                  this.addressControl.markAsTouched();
                  this.addressControl.updateValueAndValidity();
                  
                  // Update the searchable dropdown display
                  this.selectedAddress = createdAddress;
                  this.updateDisplayValue();
                  
                  // Trigger change detection
                  this.cdr.detectChanges();
                  
                  
                  this.onChange(createdAddress);
                  this.addressSelected.emit(createdAddress);
                }, 300);
              } else {
              }
              
              this.showNewAddressForm = false;
              this.isCreating = false;
              this.newAddressForm.reset({ country: 'Saudi Arabia', isActive: true });
            },
            error: (error: any) => {
              // Fallback: still add the new address to local list
              this.addresses.push(newAddress);
              
              setTimeout(() => {
                this.selectedAddress = newAddress;
                
                // Ensure the value is a string
                const addressId = String(newAddress.id);
                this.addressControl.setValue(addressId);
                this.addressControl.markAsTouched();
                
                // Trigger change detection
                this.cdr.detectChanges();
                
                // Trigger change handler manually
                this.onAddressChangeInternal(addressId);
                
                
                this.onChange(newAddress);
                this.addressSelected.emit(newAddress);
              }, 100);
              
              this.showNewAddressForm = false;
              this.isCreating = false;
              this.newAddressForm.reset({ country: 'Saudi Arabia', isActive: true });
            }
          });
        },
        error: (error: any) => {
          // Clear the pending signature on error
          this.pendingAddressSignature = null;
          this.isCreating = false;
        }
      });
    }
  }

  cancelNewAddress(): void {
    this.showNewAddressForm = false;
    this.addressControl.setValue('');
    this.newAddressForm.reset({ country: 'Saudi Arabia', isActive: true });
  }

  startNewAddress(): void {
    // Clear any selected address
    this.selectedAddress = null;
    this.addressControl.setValue('');
    this.onChange(null);
    this.addressSelected.emit(null);
    
    // Show the new address form
    this.showNewAddressForm = true;
    
    // Reset the form to default values
    this.newAddressForm.reset({ country: 'Saudi Arabia', isActive: true });
  }
}
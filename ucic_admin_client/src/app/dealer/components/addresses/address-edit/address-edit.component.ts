import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { DealerAddressService, DealerAddress, CreateAddressRequest } from '../../../services/dealer-address.service';

@Component({
  selector: 'app-address-edit',
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './address-edit.component.html',
  styleUrl: './address-edit.component.scss'
})
export class AddressEditComponent implements OnInit {
  addressForm: FormGroup;
  loading = false;
  errorMessage = '';
  addressId: string = '';
  address: DealerAddress | null = null;

  constructor(
    private fb: FormBuilder,
    private dealerAddressService: DealerAddressService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.addressForm = this.fb.group({
      addressLine1: ['', Validators.required],
      addressLine2: [''],
      city: ['', Validators.required],
      state: [''],
      postalCode: [''],
      country: ['Saudi Arabia', Validators.required],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.addressId = this.route.snapshot.params['id'];
    if (this.addressId) {
      this.loadAddress();
    }
  }

  private loadAddress(): void {
    this.loading = true;
    this.dealerAddressService.getAddressById(this.addressId).subscribe({
      next: (address) => {
        this.address = address;
        this.addressForm.patchValue({
          addressLine1: address.addressLine1,
          addressLine2: address.addressLine2 || '',
          city: address.city,
          state: address.state || '',
          postalCode: address.postalCode || '',
          country: address.country || 'Saudi Arabia',
          isActive: address.isActive
        });
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load address. Please try again.';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.addressForm.valid) {
      this.loading = true;
      this.errorMessage = '';

      const addressData: CreateAddressRequest = this.addressForm.value;

      this.dealerAddressService.updateAddress(this.addressId, addressData).subscribe({
        next: () => {
          this.router.navigate(['/dealer/addresses']);
        },
        error: (error) => {
          this.errorMessage = 'Failed to update address. Please try again.';
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/dealer/addresses']);
  }
}

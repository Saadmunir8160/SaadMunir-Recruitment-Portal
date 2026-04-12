import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { DealerAddressService, CreateAddressRequest } from '../../../services/dealer-address.service';

@Component({
  selector: 'app-address-create',
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './address-create.component.html',
  styleUrl: './address-create.component.scss'
})
export class AddressCreateComponent implements OnInit {
  addressForm: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private dealerAddressService: DealerAddressService,
    private router: Router
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
    // Component initialization
  }

  onSubmit(): void {
    if (this.addressForm.valid) {
      this.loading = true;
      this.errorMessage = '';

      const addressData: CreateAddressRequest = this.addressForm.value;

      this.dealerAddressService.createAddress(addressData).subscribe({
        next: () => {
          this.router.navigate(['/dealer/addresses']);
        },
        error: (error) => {
          this.errorMessage = 'Failed to create address. Please try again.';
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/dealer/addresses']);
  }
}

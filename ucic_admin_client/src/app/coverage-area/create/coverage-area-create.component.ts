import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CoverageAreaService } from '../../services/coverage-area.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-coverage-area-create',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './coverage-area-create.component.html',
  styleUrls: ['./coverage-area-create.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class CoverageAreaCreateComponent {
  coverageAreaForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private coverageAreaService: CoverageAreaService,
    public router: Router
  ) {
    this.coverageAreaForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      arabicName: ['', [Validators.maxLength(100)]]
    });
  }

  onSubmit(): void {
    if (this.coverageAreaForm.valid) {
      const newCoverageArea = {
        name: this.coverageAreaForm.value.name,
        arabicName: this.coverageAreaForm.value.arabicName,
        imagePath: "1" // Hardcoded value as requested
      };

      this.coverageAreaService.createCoverageArea(newCoverageArea).subscribe({
        next: () => {
          this.router.navigate(['/admin/coverage-area/list']);
        },
        error: (error) => {
        }
      });
    }
  }

  back(): void {
    this.router.navigate(['/admin/coverage-area/list']);
  }
} 
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CoverageAreaService, CoverageArea } from '../../services/coverage-area.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-coverage-area-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './coverage-area-edit.component.html',
  styleUrls: ['./coverage-area-edit.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class CoverageAreaEditComponent implements OnInit {
  coverageAreaForm: FormGroup;
  coverageAreaId: number;

  constructor(
    private fb: FormBuilder,
    private coverageAreaService: CoverageAreaService,
    private route: ActivatedRoute,
    public router: Router
  ) {
    this.coverageAreaForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      arabicName: ['', [Validators.maxLength(100)]]
    });
    this.coverageAreaId = 0;
  }

  ngOnInit(): void {
    this.coverageAreaId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCoverageArea();
  }

  loadCoverageArea(): void {
    this.coverageAreaService.getAllCoverageAreas().subscribe({
      next: (data) => {
        // Handle both array and paginated response
        const areas = Array.isArray(data) ? data : data.data;
        const coverageArea = areas.find((ca: CoverageArea) => ca.coverageAreaId === this.coverageAreaId);
        if (coverageArea) {
          this.coverageAreaForm.patchValue({
            name: coverageArea.name,
            arabicName: coverageArea.arabicName
          });
        }
      },
      error: (error) => {
      }
    });
  }

  onSubmit(): void {
    if (this.coverageAreaForm.valid) {
      const updatedCoverageArea: CoverageArea = {
        coverageAreaId: this.coverageAreaId,
        name: this.coverageAreaForm.value.name,
        arabicName: this.coverageAreaForm.value.arabicName,
        imagePath: "1" // Hardcoded value as requested
      };

      this.coverageAreaService.updateCoverageArea(updatedCoverageArea).subscribe({
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
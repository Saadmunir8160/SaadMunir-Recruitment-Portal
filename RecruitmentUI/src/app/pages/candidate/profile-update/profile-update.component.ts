import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { CandidateService } from '../../../core/services/candidate.service';
import { LookupService } from '../../../core/services/lookup.service';
import {
  CandidateDetailDto,
  CandidateDocumentDto,
  LookupItemDto,
  CreateCandidateDto,
  CreateEducationDto,
  CreateExperienceDto,
  CvParseResult,
  DocumentTypeLabels
} from '../../../shared/interfaces/models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-profile-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatDividerModule,
    MatTabsModule,
    MatExpansionModule,
    MatChipsModule
  ],
  templateUrl: './profile-update.component.html',
  styleUrl: './profile-update.component.scss'
})
export class ProfileUpdateComponent implements OnInit {
  @ViewChild('cvFileInput') cvFileInput: any;

  // --- State ---
  candidate: CandidateDetailDto | null = null;
  loading = false;
  saving = false;
  step: 'choice' | 'cv-upload' | 'edit' = 'choice';
  cvFile: File | null = null;
  parsing = false;
  cvAnalyzed = false;
  autoFill = true;
  private hasNewCvParsed = false;
  private parsedLookupRawValues: Record<'nationalityId' | 'residenceCountryId' | 'residenceRegionId' | 'residenceCityId' | 'residenceDistrictId' | 'residenceDistrictCodeId', string> = {
    nationalityId: '',
    residenceCountryId: '',
    residenceRegionId: '',
    residenceCityId: '',
    residenceDistrictId: '',
    residenceDistrictCodeId: ''
  };
  private parsedMandatoryRawValues: Record<'fullName' | 'email' | 'mobileNumber' | 'residenceCountryId' | 'residenceCityId', string> = {
    fullName: '',
    email: '',
    mobileNumber: '',
    residenceCountryId: '',
    residenceCityId: ''
  };
  private parsedEducationLookupRawValues: Record<string, string> = {};
  private parsedExperienceLookupRawValues: Record<string, string> = {};

  // --- Forms ---
  personalForm!: FormGroup;
  educationForm!: FormGroup;
  experienceForm!: FormGroup;

  // --- Lookups ---
  nationalities: LookupItemDto[] = [];
  countries: LookupItemDto[] = [];
  regions: LookupItemDto[] = [];
  cities: LookupItemDto[] = [];
  districts: LookupItemDto[] = [];
  districtCodes: LookupItemDto[] = [];
  qualificationTypes: LookupItemDto[] = [];
  degrees: LookupItemDto[] = [];
  certificates: LookupItemDto[] = [];
  majors: LookupItemDto[] = [];
  institutions: LookupItemDto[] = [];
  universities: LookupItemDto[] = [];
  currencies: LookupItemDto[] = [];
  private allRegions: LookupItemDto[] = [];
  private allCities: LookupItemDto[] = [];
  private allDistricts: LookupItemDto[] = [];
  private allDistrictCodes: LookupItemDto[] = [];
  private lookupsLoaded = false;
  documents: CandidateDocumentDto[] = [];
  uploading = false;
  showUploadForm = false;
  selectedFile: File | null = null;
  selectedDocType = 0;

  lookupDisplayLanguage: 'en' | 'ar' = 'en';
  docTypes = Object.entries(DocumentTypeLabels).map(([value, label]) => ({
    value: Number(value), label
  }));

  constructor(
    private candidateService: CandidateService,
    private lookupService: LookupService,
    private fb: FormBuilder,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.buildForms();
  }

  ngOnInit(): void {
    this.loadProfile();
    this.loadLookups();
  }

  get educations(): FormArray {
    return this.educationForm.get('educations') as FormArray;
  }

  get experiences(): FormArray {
    return this.experienceForm.get('experiences') as FormArray;
  }

  buildForms(): void {
    this.personalForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      mobileNumber: ['', Validators.required],
      nationalId: [''],
      idType: [0],
      gender: [null],
      dateOfBirth: [null],
      nationality: [''],
      nationalityId: [null],
      nationalAddress: [''],
      residenceCountryId: [null, Validators.required],
      residenceRegionId: [null],
      residenceRegion: [''],
      residenceCity: [''],
      residenceCityId: [null, Validators.required],
      residenceDistrictId: [null],
      residenceDistrictCodeId: [null],
      postalCode: ['', Validators.maxLength(20)],
      buildingNumber: ['', Validators.maxLength(100)],
      street: ['', Validators.maxLength(200)]
    });
    this.educationForm = this.fb.group({ educations: this.fb.array([]) });
    this.experienceForm = this.fb.group({ experiences: this.fb.array([]) });
  }

  loadProfile(): void {
    this.loading = true;
    this.candidateService.getProfile().subscribe({
      next: (res) => {
        this.candidate = res.data;
        this.documents = res.data.documents || [];
        // If a new CV has already been parsed in this session, never re-apply old profile values.
        if (!this.hasNewCvParsed) {
          this.patchForms(res.data);
          if (this.lookupsLoaded) {
            this.loadLocationLookupsFromForm();
            this.syncAllLookupNames();
          }
        }
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load profile', 'Close', { duration: 4000 });
      }
    });
  }

  patchForms(data: CandidateDetailDto): void {
    this.updateLookupDisplayLanguage(
      data.fullName,
      data.nationality,
      data.nationalAddress,
      data.residenceCity,
      ...(data.educations || []).flatMap(edu => [edu.qualification, edu.major, edu.institution, edu.country]),
      ...(data.experiences || []).flatMap(exp => [exp.jobTitle, exp.employer, exp.country, exp.description])
    );

    this.personalForm.patchValue({
      fullName: data.fullName,
      email: data.email,
      mobileNumber: data.mobileNumber,
      nationalId: data.nationalId,
      idType: data.idType,
      gender: data.gender,
      dateOfBirth: data.dateOfBirth ? new Date(data.dateOfBirth) : null,
      nationality: data.nationality,
      nationalityId: data.nationalityId ?? null,
      nationalAddress: data.nationalAddress,
      residenceCity: data.residenceCity,
      residenceCityId: data.residenceCityId ?? null,
      residenceCountryId: data.residenceCountryId ?? null,
      residenceRegionId: data.residenceRegionId ?? null,
      residenceRegion: '',
      residenceDistrictId: data.residenceDistrictId ?? null,
      residenceDistrictCodeId: data.residenceDistrictCodeId ?? null,
      postalCode: data.postalCode ?? '',
      buildingNumber: data.buildingNumber ?? '',
      street: data.street ?? ''
    });

    this.educations.clear();
    (data.educations || []).forEach(edu => {
      this.educations.push(this.fb.group({
        candidateEducationId: [edu.candidateEducationId ?? null],
        candidateQualificationId: [edu.candidateQualificationId ?? null],
        qualification: [edu.qualification, Validators.required],
        qualificationTypeId: [edu.qualificationTypeId ?? null],
        major: [edu.major],
        institution: [edu.institution],
        graduationYear: [edu.graduationYear],
        gradeOrGPA: [edu.gradeOrGPA],
        country: [edu.country],
        degreeId: [edu.degreeId ?? null],
        certificateId: [edu.certificateId ?? null],
        majorFieldOfStudyId: [edu.majorFieldOfStudyId ?? null],
        institutionId: [edu.institutionId ?? null],
        countryId: [edu.countryId ?? null]
      }));
    });

    this.experiences.clear();
    (data.experiences || []).forEach(exp => {
      this.experiences.push(this.fb.group({
        candidateExperienceId: [exp.candidateExperienceId ?? null],
        jobTitle: [exp.jobTitle],
        employer: [exp.employer],
        startDate: [exp.startDate ? new Date(exp.startDate) : null],
        endDate: [exp.endDate ? new Date(exp.endDate) : null],
        isCurrent: [exp.isCurrent],
        salary: [exp.salary],
        currency: [exp.currency || 'SAR'],
        currencyId: [exp.currencyId ?? null],
        country: [exp.country],
        countryId: [exp.countryId ?? null],
        description: [exp.description]
      }));
    });

    this.syncAllLookupNames();
  }

  private loadLookups(): void {
    forkJoin({
      nationalities: this.lookupService.getNationalities(),
      countries: this.lookupService.getCountries(),
      regions: this.lookupService.getRegions(),
      cities: this.lookupService.getCities(),
      districts: this.lookupService.getDistricts(),
      districtCodes: this.lookupService.getDistrictCodes(),
      qualificationTypes: this.lookupService.getQualificationTypes(),
      degrees: this.lookupService.getDegrees(),
      certificates: this.lookupService.getCertificates(),
      majors: this.lookupService.getMajors(),
      institutions: this.lookupService.getInstitutions(),
      universities: this.lookupService.getUniversities(),
      currencies: this.lookupService.getCurrencies()
    }).subscribe({
      next: (res) => {
        this.nationalities = this.withOtherOption(res.nationalities.data || []);
        this.countries = this.withOtherOption(res.countries.data || []);
        this.allRegions = this.withOtherOption(res.regions.data || []);
        this.allCities = this.withOtherOption(res.cities.data || []);
        this.allDistricts = this.withOtherOption(res.districts.data || []);
        this.allDistrictCodes = this.withOtherOption(res.districtCodes.data || []);
        this.qualificationTypes = this.withOtherOption(res.qualificationTypes.data || []);
        this.degrees = this.withOtherOption(res.degrees.data || []);
        this.certificates = this.withOtherOption(res.certificates.data || []);
        this.majors = this.withOtherOption(res.majors.data || []);
        this.institutions = this.withOtherOption(res.institutions.data || []);
        this.universities = (res.universities.data || []).length > 0
          ? this.withOtherOption(res.universities.data || [])
          : this.institutions;
        this.currencies = res.currencies.data || [];
        this.lookupsLoaded = true;

        if (this.candidate) {
          this.loadLocationLookupsFromForm();
          this.syncAllLookupNames();
        }
      },
      error: () => {}
    });
  }

  private loadLocationLookupsFromForm(): void {
    const countryId = this.personalForm.get('residenceCountryId')?.value as number | null;
    const cityId = this.personalForm.get('residenceCityId')?.value as number | null;
    const districtId = this.personalForm.get('residenceDistrictId')?.value as number | null;

    this.loadRegionsForCountry(countryId);
    this.loadCitiesForCountry(countryId);
    this.loadDistrictsForCity(cityId);
    this.loadDistrictCodesForDistrict(districtId);
  }

  private normalizeId(value: number | string | null | undefined): number | null {
    if (value === null || value === undefined || value === '') return null;
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  private filterWithParent(source: LookupItemDto[], parentId: number | null): LookupItemDto[] {
    if (!parentId) {
      return this.withOtherOption(source);
    }

    const filtered = source.filter(item => this.isOtherLookupItem(item) || !item.parentId || item.parentId === parentId);
    return this.withOtherOption(filtered);
  }

  private loadRegionsForCountry(countryId: number | null): void {
    if (!countryId) {
      this.regions = this.withOtherOption([]);
      return;
    }

    this.lookupService.getRegions(countryId).subscribe({
      next: (res) => {
        const fromApi = this.withOtherOption(res.data || []);
        this.regions = fromApi.length > 0 ? fromApi : this.filterWithParent(this.allRegions, countryId);
      },
      error: () => {
        this.regions = this.filterWithParent(this.allRegions, countryId);
      }
    });
  }

  private loadCitiesForCountry(countryId: number | null): void {
    if (!countryId) {
      this.cities = this.withOtherOption([]);
      return;
    }

    this.lookupService.getCities(countryId).subscribe({
      next: (res) => {
        const fromApi = this.withOtherOption(res.data || []);
        this.cities = fromApi.length > 0 ? fromApi : this.filterWithParent(this.allCities, countryId);
        this.syncPersonalLookupNames();
      },
      error: () => {
        this.cities = this.filterWithParent(this.allCities, countryId);
      }
    });
  }

  private loadDistrictsForCity(cityId: number | null): void {
    if (!cityId) {
      this.districts = this.withOtherOption([]);
      return;
    }

    this.lookupService.getDistricts(cityId).subscribe({
      next: (res) => {
        const fromApi = this.withOtherOption(res.data || []);
        this.districts = fromApi.length > 0 ? fromApi : this.filterWithParent(this.allDistricts, cityId);
      },
      error: () => {
        this.districts = this.filterWithParent(this.allDistricts, cityId);
      }
    });
  }

  private loadDistrictCodesForDistrict(districtId: number | null): void {
    if (!districtId) {
      this.districtCodes = this.withOtherOption([]);
      return;
    }

    this.lookupService.getDistrictCodes(districtId).subscribe({
      next: (res) => {
        const fromApi = this.withOtherOption(res.data || []);
        this.districtCodes = fromApi.length > 0 ? fromApi : this.filterWithParent(this.allDistrictCodes, districtId);
      },
      error: () => {
        this.districtCodes = this.filterWithParent(this.allDistrictCodes, districtId);
      }
    });
  }

  private withOtherOption(items: LookupItemDto[]): LookupItemDto[] {
    const list = items ?? [];
    const other = list.find(item => this.isOtherLookupItem(item));
    if (!other) {
      return [...list, { id: -1, name: 'Other', nameAr: 'أخرى', parentId: null }];
    }
    const nonOther = list.filter(item => item.id !== other.id);
    return [...nonOther, other];
  }

  private isOtherLookupItem(item: LookupItemDto | null | undefined): boolean {
    if (!item) return false;
    const en = (item.name ?? '').trim().toLowerCase();
    const ar = (item.nameAr ?? '').trim().toLowerCase();
    return en === 'other' || ar === 'أخرى';
  }

  private isParsedOtherValue(value: string | null | undefined): boolean {
    const normalized = this.normalizeLookupValue(value);
    return normalized === 'other' || normalized === 'اخرى';
  }

  // --- CV Upload Logic ---

  chooseUploadCv(): void {
    this.step = 'cv-upload';
    this.cvFile = null;
    this.cvAnalyzed = false;
    this.parsing = false;
    this.hasNewCvParsed = false;
    this.parsedLookupRawValues = {
      nationalityId: '',
      residenceCountryId: '',
      residenceRegionId: '',
      residenceCityId: '',
      residenceDistrictId: '',
      residenceDistrictCodeId: ''
    };
    this.parsedMandatoryRawValues = {
      fullName: '',
      email: '',
      mobileNumber: '',
      residenceCountryId: '',
      residenceCityId: ''
    };
    this.parsedEducationLookupRawValues = {};
    this.parsedExperienceLookupRawValues = {};
  }

  chooseEditProfile(): void {
    if (this.lookupsLoaded) {
      this.loadLocationLookupsFromForm();
    }
    this.step = 'edit';
  }

  onNationalitySelected(nationalityId: number | null): void {
    this.clearControlError('nationalityId', 'parsedNotMatched');
    this.clearControlError('nationalityId', 'parsedMissing');
    const selected = this.nationalities.find(item => item.id === nationalityId);
    this.personalForm.patchValue({
      nationalityId: nationalityId ?? null,
      nationality: selected?.name ?? ''
    });
  }

  onResidenceCountryChange(countryId: number | null): void {
    const normalizedCountryId = this.normalizeId(countryId);
    this.clearControlError('residenceCountryId', 'parsedNotMatched');
    this.clearControlError('residenceCountryId', 'parsedMissing');
    this.clearControlError('residenceCityId', 'parsedNotMatched');
    this.clearControlError('residenceCityId', 'parsedMissing');
    this.clearControlError('residenceRegionId', 'parsedNotMatched');
    this.clearControlError('residenceRegionId', 'parsedMissing');
    this.clearControlError('residenceDistrictId', 'parsedNotMatched');
    this.clearControlError('residenceDistrictId', 'parsedMissing');
    this.clearControlError('residenceDistrictCodeId', 'parsedNotMatched');
    this.clearControlError('residenceDistrictCodeId', 'parsedMissing');

    this.personalForm.patchValue({
      residenceCountryId: normalizedCountryId,
      residenceRegionId: null,
      residenceRegion: '',
      residenceCityId: null,
      residenceCity: '',
      residenceDistrictId: null,
      residenceDistrictCodeId: null
    });

    this.regions = this.withOtherOption([]);
    this.cities = this.withOtherOption([]);
    this.districts = this.withOtherOption([]);
    this.districtCodes = this.withOtherOption([]);

    this.loadRegionsForCountry(normalizedCountryId);
    this.loadCitiesForCountry(normalizedCountryId);
  }

  onRegionSelected(regionId: number | null): void {
    this.clearControlError('residenceRegionId', 'parsedNotMatched');
    this.clearControlError('residenceRegionId', 'parsedMissing');
    const selected = this.regions.find(item => item.id === regionId);
    this.personalForm.patchValue({
      residenceRegionId: regionId ?? null,
      residenceRegion: selected?.name ?? ''
    });
  }

  onCitySelected(cityId: number | null): void {
    const selected = this.cities.find(item => item.id === cityId);
    this.clearControlError('residenceCityId', 'parsedNotMatched');
    this.clearControlError('residenceCityId', 'parsedMissing');
    this.clearControlError('residenceDistrictId', 'parsedNotMatched');
    this.clearControlError('residenceDistrictId', 'parsedMissing');
    this.clearControlError('residenceDistrictCodeId', 'parsedNotMatched');
    this.clearControlError('residenceDistrictCodeId', 'parsedMissing');
    this.personalForm.patchValue({
      residenceCityId: cityId ?? null,
      residenceCity: selected?.name ?? '',
      residenceDistrictId: null,
      residenceDistrictCodeId: null
    });

    this.districtCodes = this.withOtherOption([]);
    this.loadDistrictsForCity(cityId);
  }

  onResidenceDistrictChange(districtId: number | null): void {
    const normalizedDistrictId = this.normalizeId(districtId);
    this.clearControlError('residenceDistrictId', 'parsedNotMatched');
    this.clearControlError('residenceDistrictId', 'parsedMissing');
    this.clearControlError('residenceDistrictCodeId', 'parsedNotMatched');
    this.clearControlError('residenceDistrictCodeId', 'parsedMissing');
    this.personalForm.patchValue({
      residenceDistrictId: normalizedDistrictId,
      residenceDistrictCodeId: null
    });
    this.loadDistrictCodesForDistrict(normalizedDistrictId);
  }

  onDistrictCodeChange(districtCodeId: number | null): void {
    this.clearControlError('residenceDistrictCodeId', 'parsedNotMatched');
    this.clearControlError('residenceDistrictCodeId', 'parsedMissing');
    this.personalForm.patchValue({ residenceDistrictCodeId: this.normalizeId(districtCodeId) });
  }

  onCvFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.cvFile = input.files[0];
      this.cvAnalyzed = false;
    }
  }

  analyzeCv(): void {
    if (!this.cvFile) return;
    this.parsing = true;
    this.candidateService.parseCvPreview(this.cvFile).subscribe({
      next: (res) => {
        this.parsing = false;
        if (res?.data) {
          this.applyParseResult(res.data);
          this.cvAnalyzed = true;
          this.snackBar.open('CV analyzed! Form has been updated.', 'Close', { duration: 4000 });
          this.step = 'edit';
        }
      },
      error: (err) => {
        this.parsing = false;
        const msg = err?.error?.message || 'CV analysis failed. Please try again.';
        this.snackBar.open(msg, 'Close', { duration: 4000 });
      }
    });
  }

  private applyParseResult(result: CvParseResult): void {
    const info = result.personalInfo;

    this.hasNewCvParsed = true;
    this.updateLookupDisplayLanguage(
      info?.fullName,
      info?.nationality,
      info?.address,
      info?.city,
      ...(result.education || []).flatMap(edu => [edu.qualification, edu.major, edu.institution, edu.country]),
      ...(result.experience || []).flatMap(exp => [exp.jobTitle, exp.employer, exp.country, exp.description])
    );
    this.parsedLookupRawValues = {
      nationalityId: (info?.nationality ?? '').trim(),
      residenceCountryId: (this.findCountryFromAddress(info?.address)?.name ?? '').trim(),
      residenceRegionId: '',
      residenceCityId: (info?.city ?? '').trim(),
      residenceDistrictId: '',
      residenceDistrictCodeId: ''
    };
    this.parsedMandatoryRawValues = {
      fullName: (info?.fullName ?? '').trim(),
      email: (info?.email ?? '').trim(),
      mobileNumber: (info?.phone ?? '').trim(),
      residenceCountryId: (this.findCountryFromAddress(info?.address)?.name ?? (info?.address ?? '')).trim(),
      residenceCityId: (info?.city ?? '').trim()
    };
    this.parsedEducationLookupRawValues = {};
    this.parsedExperienceLookupRawValues = {};

    // New CV should fully replace prior profile values on this screen.
    this.personalForm.reset({
      fullName: '',
      email: '',
      mobileNumber: '',
      nationalId: '',
      idType: 0,
      gender: null,
      dateOfBirth: null,
      nationality: '',
      nationalityId: null,
      nationalAddress: '',
      residenceCountryId: null,
      residenceRegionId: null,
      residenceRegion: '',
      residenceCity: '',
      residenceCityId: null,
      residenceDistrictId: null,
      residenceDistrictCodeId: null,
      postalCode: '',
      buildingNumber: '',
      street: ''
    });
    this.educations.clear();
    this.experiences.clear();

    this.regions = this.withOtherOption([]);
    this.cities = this.withOtherOption([]);
    this.districts = this.withOtherOption([]);
    this.districtCodes = this.withOtherOption([]);

    if (info) {
      const matchedNationality = this.findLookupMatch(this.nationalities, info.nationality);

      this.personalForm.patchValue({
        fullName: info.fullName || '',
        email: info.email || '',
        mobileNumber: info.phone || '',
        nationalId: info.nationalId || '',
        nationality: info.nationality || '',
        nationalityId: matchedNationality?.id ?? null,
        dateOfBirth: info.dateOfBirth ? new Date(info.dateOfBirth) : null,
        nationalAddress: info.address || ''
      });

      if ((info.nationality ?? '').trim().length > 0 && !matchedNationality?.id) {
        this.setParsedLookupMismatch('nationalityId', info.nationality ?? '');
      }

      this.resolveParsedPersonalLocation(info);
      this.markParsedLookupControlsAsTouched();
      this.markParsedMissingRequiredFields();
    }

    (result.education || []).forEach(edu => {
      const qualificationText = (edu.qualification ?? '').trim();
      const majorText = (edu.major ?? '').trim();
      const institutionText = (edu.institution ?? '').trim();
      const countryText = (edu.country ?? '').trim();
      const matchedMajor = this.findLookupMatch(this.majors, edu.major);
      const matchedCountry = this.findLookupMatch(this.countries, edu.country);
      const matchedInstitution = this.findLookupMatch(this.getInstitutionsForCountry(matchedCountry?.id ?? null), edu.institution)
        ?? this.findLookupMatch(this.institutions, edu.institution);
      const matchedDegree = this.findLookupMatch(this.degrees, edu.qualification);
      const matchedCertificate = matchedDegree ? null : this.findLookupMatch(this.certificates, edu.qualification);
      const inferredQualificationKind = this.inferQualificationKind(qualificationText, !!matchedDegree, !!matchedCertificate);
      const matchedQualificationType = inferredQualificationKind === 'certificate'
        ? this.findLookupMatch(this.qualificationTypes, 'Certificate')
        : inferredQualificationKind === 'degree'
          ? this.findLookupMatch(this.qualificationTypes, 'Degree')
          : null;

      const group = this.fb.group({
        candidateEducationId: [null],
        candidateQualificationId: [null],
        qualification: [qualificationText, Validators.required],
        qualificationTypeId: [matchedQualificationType?.id ?? null],
        major: [edu.major ?? ''],
        institution: [edu.institution ?? ''],
        graduationYear: [edu.graduationYear],
        gradeOrGPA: [edu.grade ?? ''],
        country: [edu.country ?? ''],
        degreeId: [matchedDegree?.id ?? null],
        certificateId: [matchedCertificate?.id ?? null],
        majorFieldOfStudyId: [matchedMajor?.id ?? null],
        institutionId: [matchedInstitution?.id ?? null],
        countryId: [matchedCountry?.id ?? null]
      });

      this.educations.push(group);
      const index = this.educations.length - 1;

      if (qualificationText) {
        if (!matchedQualificationType?.id) {
          this.setParsedSectionLookupMismatch('education', index, group, 'qualificationTypeId', qualificationText);
        } else if (inferredQualificationKind === 'certificate' && !matchedCertificate?.id) {
          this.setParsedSectionLookupMismatch('education', index, group, 'certificateId', qualificationText);
        } else if (inferredQualificationKind === 'degree' && !matchedDegree?.id) {
          this.setParsedSectionLookupMismatch('education', index, group, 'degreeId', qualificationText);
        }
      }

      if (majorText) {
        if (!matchedMajor?.id) {
          this.setParsedSectionLookupMismatch('education', index, group, 'majorFieldOfStudyId', majorText);
        }
      }

      if (institutionText) {
        if (!matchedInstitution?.id) {
          this.setParsedSectionLookupMismatch('education', index, group, 'institutionId', institutionText);
        }
      }

      if (countryText) {
        if (!matchedCountry?.id) {
          this.setParsedSectionLookupMismatch('education', index, group, 'countryId', countryText);
        }
      }
    });

    (result.experience || []).forEach(exp => {
      const currencyText = (exp.currency ?? '').trim();
      const countryText = (exp.country ?? '').trim();
      const matchedCountry = this.findLookupMatch(this.countries, exp.country);
      const matchedCurrency = this.findLookupMatch(this.currencies, exp.currency);
      const group = this.fb.group({
        candidateExperienceId: [null],
        jobTitle: [exp.jobTitle ?? ''],
        employer: [exp.employer ?? ''],
        startDate: [exp.startDate ? new Date(exp.startDate) : null],
        endDate: [exp.endDate ? new Date(exp.endDate) : null],
        isCurrent: [exp.isCurrent || false],
        salary: [exp.salary],
        currency: [exp.currency || 'SAR'],
        currencyId: [matchedCurrency?.id ?? null],
        country: [exp.country ?? ''],
        countryId: [matchedCountry?.id ?? null],
        description: [exp.description ?? '']
      });

      this.experiences.push(group);
      const index = this.experiences.length - 1;

      if (currencyText) {
        if (!matchedCurrency?.id) {
          this.setParsedSectionLookupMismatch('experience', index, group, 'currencyId', currencyText);
        }
      }

      if (countryText) {
        if (!matchedCountry?.id) {
          this.setParsedSectionLookupMismatch('experience', index, group, 'countryId', countryText);
        }
      }
    });

    this.syncAllLookupNames();
    this.markLookupControlsAsTouched();
  }

  private markParsedLookupControlsAsTouched(): void {
    ['nationalityId', 'residenceCountryId', 'residenceRegionId', 'residenceCityId', 'residenceDistrictId', 'residenceDistrictCodeId']
      .forEach(name => this.personalForm.get(name)?.markAsTouched());
  }

  private containsArabicText(value: string | null | undefined): boolean {
    return /[\u0600-\u06FF]/.test(value ?? '');
  }

  private updateLookupDisplayLanguage(...values: Array<string | null | undefined>): void {
    const nonEmptyValues = values.filter(value => (value ?? '').toString().trim().length > 0);
    if (nonEmptyValues.length === 0) return;

    this.lookupDisplayLanguage = nonEmptyValues.some(value => this.containsArabicText(value)) ? 'ar' : 'en';
  }

  private markParsedMissingRequiredFields(): void {
    ['fullName', 'email', 'mobileNumber', 'residenceCountryId', 'residenceCityId'].forEach(name => {
      const control = this.personalForm.get(name);
      if (control && !control.value) {
        control.markAsTouched();
      }
    });
  }

  private normalizeLookupValue(value: string | null | undefined): string {
    const normalized = (value ?? '')
      .normalize('NFKC')
      .toLowerCase()
      .replace(/[\u200B-\u200F\u202A-\u202E\u2066-\u2069]/g, '')
      .replace(/[\u064B-\u065F\u0670\u06D6-\u06ED]/g, '')
      .replace(/\u0640/g, '')
      .replace(/[أإآٱ]/g, 'ا')
      .replace(/ى/g, 'ي')
      .replace(/ؤ/g, 'و')
      .replace(/ئ/g, 'ي')
      .replace(/ة/g, 'ه')
      .replace(/ک/g, 'ك')
      .replace(/[^a-z0-9\u0621-\u063A\u0641-\u064A\s]/g, ' ')
      .replace(/\bmecca\b/g, 'makkah')
      .replace(/\bmakka\b/g, 'makkah')
      .replace(/مكة المكرمة/g, 'مكة')
      .replace(/مكة/g, 'makkah')
      .replace(/\bksa\b/g, 'saudi arabia')
      .replace(/\bkingdom of saudi arabia\b/g, 'saudi arabia')
      .replace(/المملكة العربية السعودية/g, 'السعودية')
      .replace(/المملكة العربية/g, '')
      .replace(/الجمهورية العربية/g, '')
      .replace(/\s+/g, ' ')
      .trim();

    return normalized;
  }

  private findLookupMatch(list: LookupItemDto[], value: string | null | undefined): LookupItemDto | null {
    const normalized = this.normalizeLookupValue(value);
    if (!normalized) return null;

    const compact = normalized.replace(/\s+/g, '');
    const candidates = this.isParsedOtherValue(value)
      ? list
      : list.filter(item => !this.isOtherLookupItem(item));

    const byExact = candidates.find(item => this.normalizeLookupValue(item.name) === normalized)
      ?? candidates.find(item => this.normalizeLookupValue(item.nameAr ?? '') === normalized);
    if (byExact) return byExact;

    const byCompactExact = candidates.find(item => this.normalizeLookupValue(item.name).replace(/\s+/g, '') === compact)
      ?? candidates.find(item => this.normalizeLookupValue(item.nameAr ?? '').replace(/\s+/g, '') === compact);
    if (byCompactExact) return byCompactExact;

    const canUseContains = normalized.length >= 3;
    if (!canUseContains) return null;

    return candidates.find(item => this.normalizeLookupValue(item.name).includes(normalized) || normalized.includes(this.normalizeLookupValue(item.name)))
      ?? candidates.find(item => this.normalizeLookupValue(item.nameAr ?? '').includes(normalized) || normalized.includes(this.normalizeLookupValue(item.nameAr ?? '')))
      ?? candidates.find(item => this.normalizeLookupValue(item.name).replace(/\s+/g, '').includes(compact) || compact.includes(this.normalizeLookupValue(item.name).replace(/\s+/g, '')))
      ?? candidates.find(item => this.normalizeLookupValue(item.nameAr ?? '').replace(/\s+/g, '').includes(compact) || compact.includes(this.normalizeLookupValue(item.nameAr ?? '').replace(/\s+/g, '')))
      ?? null;
  }

  private findCountryFromAddress(address: string | null | undefined): LookupItemDto | null {
    const raw = (address ?? '').trim();
    if (!raw) return null;

    const chunks = raw.split(/[,:;\-|/]/).map(part => part.trim()).filter(Boolean);
    for (const chunk of chunks) {
      const matched = this.findLookupMatch(this.countries, chunk);
      if (matched) return matched;
    }

    return this.findLookupMatch(this.countries, raw);
  }

  private getAddressChunks(address: string | null | undefined): string[] {
    return (address ?? '')
      .split(/[,:;\-|/]/)
      .map(part => part.trim())
      .filter(Boolean);
  }

  private tryResolveOptionalAddressLookup(
    controlName: 'residenceRegionId' | 'residenceDistrictId' | 'residenceDistrictCodeId',
    list: LookupItemDto[],
    address: string | null | undefined,
    excluded: Set<string>
  ): number | null {
    const chunks = this.getAddressChunks(address);
    const fullAddress = (address ?? '').trim();
    if (!chunks.length) return null;

    let candidateRaw: string | null = null;

    for (const chunk of chunks) {
      const normalized = this.normalizeLookupValue(chunk);
      if (!normalized || excluded.has(normalized)) continue;

      if (!candidateRaw) {
        candidateRaw = chunk;
      }

      const matched = this.findLookupMatch(list, chunk);
      if (matched?.id) {
        this.parsedLookupRawValues[controlName] = chunk;
        return matched.id;
      }
    }

    if (fullAddress) {
      const matchedFromFullAddress = this.findLookupMatch(list, fullAddress);
      if (matchedFromFullAddress?.id) {
        this.parsedLookupRawValues[controlName] = fullAddress;
        return matchedFromFullAddress.id;
      }
    }

    if (controlName === 'residenceDistrictCodeId') {
      const numericTokens = fullAddress.match(/\d{2,}/g) ?? [];
      if (numericTokens.length > 0) {
        const byNumericToken = list.find(item => {
          const normalizedName = this.normalizeLookupValue(item.name);
          const normalizedNameAr = this.normalizeLookupValue(item.nameAr ?? '');
          return numericTokens.some(token => normalizedName === token || normalizedNameAr === token);
        });

        if (byNumericToken?.id) {
          this.parsedLookupRawValues[controlName] = numericTokens[0] ?? '';
          return byNumericToken.id;
        }
      }
    }

    if (candidateRaw) {
      this.setParsedLookupMismatch(controlName, candidateRaw);
    }

    return null;
  }

  private resolveOptionalParsedAddressLookups(
    info: CvParseResult['personalInfo'],
    countryId: number,
    cityId: number,
    countryName: string,
    cityName: string
  ): void {
    const address = info?.address ?? '';
    if (!address.trim()) return;

    const baseExcluded = new Set<string>();
    const addExcluded = (value: string | null | undefined): void => {
      const normalized = this.normalizeLookupValue(value);
      if (normalized) baseExcluded.add(normalized);
    };

    addExcluded(countryName);

    const resolveDistrictAndCode = (): void => {
      if (!cityId) return;

      this.lookupService.getDistricts(cityId).subscribe({
        next: (districtRes) => {
          const fromApi = this.withOtherOption(districtRes.data || []);
          this.districts = fromApi.length > 0 ? fromApi : this.filterWithParent(this.allDistricts, cityId);

          const districtExcluded = new Set(baseExcluded);
          const districtId = this.tryResolveOptionalAddressLookup('residenceDistrictId', this.districts, address, districtExcluded);
          if (!districtId) return;

          this.onResidenceDistrictChange(districtId);
          const districtName = this.districts.find(item => item.id === districtId)?.name ?? '';
          addExcluded(districtName);

          this.lookupService.getDistrictCodes(districtId).subscribe({
            next: (districtCodeRes) => {
              const districtCodeFromApi = this.withOtherOption(districtCodeRes.data || []);
              this.districtCodes = districtCodeFromApi.length > 0
                ? districtCodeFromApi
                : this.filterWithParent(this.allDistrictCodes, districtId);

              const districtCodeExcluded = new Set(baseExcluded);
              const districtCodeId = this.tryResolveOptionalAddressLookup('residenceDistrictCodeId', this.districtCodes, address, districtCodeExcluded);
              if (districtCodeId) {
                this.onDistrictCodeChange(districtCodeId);
              }
            },
            error: () => {
              this.districtCodes = this.filterWithParent(this.allDistrictCodes, districtId);
              const districtCodeExcluded = new Set(baseExcluded);
              const districtCodeId = this.tryResolveOptionalAddressLookup('residenceDistrictCodeId', this.districtCodes, address, districtCodeExcluded);
              if (districtCodeId) {
                this.onDistrictCodeChange(districtCodeId);
              }
            }
          });
        },
        error: () => {
          this.districts = this.filterWithParent(this.allDistricts, cityId);
          const districtExcluded = new Set(baseExcluded);
          const districtId = this.tryResolveOptionalAddressLookup('residenceDistrictId', this.districts, address, districtExcluded);
          if (!districtId) return;

          this.onResidenceDistrictChange(districtId);
          const districtName = this.districts.find(item => item.id === districtId)?.name ?? '';
          addExcluded(districtName);

          this.districtCodes = this.filterWithParent(this.allDistrictCodes, districtId);
          const districtCodeExcluded = new Set(baseExcluded);
          const districtCodeId = this.tryResolveOptionalAddressLookup('residenceDistrictCodeId', this.districtCodes, address, districtCodeExcluded);
          if (districtCodeId) {
            this.onDistrictCodeChange(districtCodeId);
          }
        }
      });
    };

    if (!countryId) {
      resolveDistrictAndCode();
      return;
    }

    this.lookupService.getRegions(countryId).subscribe({
      next: (regionRes) => {
        const fromApi = this.withOtherOption(regionRes.data || []);
        this.regions = fromApi.length > 0 ? fromApi : this.filterWithParent(this.allRegions, countryId);

        const regionExcluded = new Set(baseExcluded);
        const regionId = this.tryResolveOptionalAddressLookup('residenceRegionId', this.regions, address, regionExcluded);
        if (regionId) {
          this.onRegionSelected(regionId);
          const regionName = this.regions.find(item => item.id === regionId)?.name ?? '';
          addExcluded(regionName);
        }

        resolveDistrictAndCode();
      },
      error: () => {
        this.regions = this.filterWithParent(this.allRegions, countryId);

        const regionExcluded = new Set(baseExcluded);
        const regionId = this.tryResolveOptionalAddressLookup('residenceRegionId', this.regions, address, regionExcluded);
        if (regionId) {
          this.onRegionSelected(regionId);
          const regionName = this.regions.find(item => item.id === regionId)?.name ?? '';
          addExcluded(regionName);
        }

        resolveDistrictAndCode();
      }
    });
  }

  private resolveParsedPersonalLocation(info: CvParseResult['personalInfo']): void {
    if (!info) return;

    const parsedCountry = this.findCountryFromAddress(info.address) ?? this.findLookupMatch(this.countries, info.city);
    const parsedCityText = (info.city ?? '').trim();

    if ((info.address ?? '').trim().length > 0 && !parsedCountry) {
      this.setParsedLookupMismatch('residenceCountryId', info.address ?? '');
    } else if ((info.address ?? '').trim().length === 0) {
      this.setParsedLookupMissing('residenceCountryId');
    }

    if (parsedCountry?.id) {
      this.onResidenceCountryChange(parsedCountry.id);

      if (!parsedCityText) {
        this.setParsedLookupMissing('residenceCityId');
        return;
      }

      this.lookupService.getCities(parsedCountry.id).subscribe({
        next: (res) => {
          this.cities = this.withOtherOption(res.data || []);
          const matchedCity = this.findLookupMatch(this.cities, parsedCityText);
          if (matchedCity?.id) {
            this.onCitySelected(matchedCity.id);
            this.resolveOptionalParsedAddressLookups(
              info,
              parsedCountry.id,
              matchedCity.id,
              parsedCountry.name,
              matchedCity.name
            );
          } else {
            this.setParsedLookupMismatch('residenceCityId', parsedCityText);
          }
        },
        error: () => {
          this.cities = this.filterWithParent(this.allCities, parsedCountry.id);
          const matchedCity = this.findLookupMatch(this.cities, parsedCityText);
          if (matchedCity?.id) {
            this.onCitySelected(matchedCity.id);
            this.resolveOptionalParsedAddressLookups(
              info,
              parsedCountry.id,
              matchedCity.id,
              parsedCountry.name,
              matchedCity.name
            );
          } else {
            this.setParsedLookupMismatch('residenceCityId', parsedCityText);
          }
        }
      });
      return;
    }

    if (!parsedCityText) {
      this.setParsedLookupMissing('residenceCityId');
      return;
    }

    this.lookupService.getCities().subscribe({
      next: (res) => {
        const globalCities = this.withOtherOption(res.data || []);
        const matchedCity = this.findLookupMatch(globalCities, parsedCityText);

        if (!matchedCity?.id) {
          this.setParsedLookupMismatch('residenceCityId', parsedCityText);
          return;
        }

        const parentCountryId = matchedCity.parentId ?? null;
        if (!parentCountryId) {
          this.cities = globalCities;
          this.onCitySelected(matchedCity.id);
          const matchedCountry = this.countries.find(item => item.id === parentCountryId) ?? null;
          this.resolveOptionalParsedAddressLookups(
            info,
            parentCountryId || 0,
            matchedCity.id,
            matchedCountry?.name ?? '',
            matchedCity.name
          );
          return;
        }

        this.onResidenceCountryChange(parentCountryId);
        this.lookupService.getCities(parentCountryId).subscribe({
          next: (countryCitiesRes) => {
            this.cities = this.withOtherOption(countryCitiesRes.data || []);
            this.onCitySelected(matchedCity.id);
            const matchedCountry = this.countries.find(item => item.id === parentCountryId) ?? null;
            this.resolveOptionalParsedAddressLookups(
              info,
              parentCountryId,
              matchedCity.id,
              matchedCountry?.name ?? '',
              matchedCity.name
            );
          },
          error: () => {
            this.cities = this.filterWithParent(this.allCities, parentCountryId);
            this.onCitySelected(matchedCity.id);
            const matchedCountry = this.countries.find(item => item.id === parentCountryId) ?? null;
            this.resolveOptionalParsedAddressLookups(
              info,
              parentCountryId,
              matchedCity.id,
              matchedCountry?.name ?? '',
              matchedCity.name
            );
          }
        });
      },
      error: () => {
        const globalCities = this.withOtherOption(this.allCities);
        const matchedCity = this.findLookupMatch(globalCities, parsedCityText);
        if (!matchedCity?.id) {
          this.setParsedLookupMismatch('residenceCityId', parsedCityText);
          return;
        }
        this.onCitySelected(matchedCity.id);
        const matchedCountry = this.countries.find(item => item.id === matchedCity.parentId) ?? null;
        if (matchedCity.parentId) {
          this.resolveOptionalParsedAddressLookups(
            info,
            matchedCity.parentId,
            matchedCity.id,
            matchedCountry?.name ?? '',
            matchedCity.name
          );
        }
      }
    });
  }

  private getLookupName(list: LookupItemDto[], id: number | null | undefined): string {
    if (!id) return '';
    return list.find(item => item.id === id)?.name ?? '';
  }

  private syncPersonalLookupNames(): void {
    const nationalityId = this.personalForm.get('nationalityId')?.value as number | null;
    const regionId = this.personalForm.get('residenceRegionId')?.value as number | null;
    const cityId = this.personalForm.get('residenceCityId')?.value as number | null;

    this.personalForm.patchValue({
      nationality: this.getLookupName(this.nationalities, nationalityId) || this.personalForm.get('nationality')?.value || '',
      residenceRegion: this.getLookupName(this.regions, regionId) || this.personalForm.get('residenceRegion')?.value || '',
      residenceCity: this.getLookupName(this.cities, cityId) || this.personalForm.get('residenceCity')?.value || ''
    }, { emitEvent: false });
  }

  private syncEducationLookupNames(index: number): void {
    const group = this.getEducationGroup(index);
    const raw = group.getRawValue();
    const qualificationName = this.isCertificateType(raw.qualificationTypeId)
      ? this.getLookupName(this.certificates, raw.certificateId)
      : this.getLookupName(this.degrees, raw.degreeId);

    group.patchValue({
      qualification: qualificationName || raw.qualification || '',
      major: this.getLookupName(this.majors, raw.majorFieldOfStudyId) || raw.major || '',
      institution: this.getLookupName(this.getInstitutionsForCountry(raw.countryId), raw.institutionId)
        || this.getLookupName(this.institutions, raw.institutionId)
        || raw.institution
        || '',
      country: this.getLookupName(this.countries, raw.countryId) || raw.country || ''
    }, { emitEvent: false });
  }

  private syncExperienceLookupNames(index: number): void {
    const group = this.getExperienceGroup(index);
    const raw = group.getRawValue();
    group.patchValue({
      currency: this.getLookupName(this.currencies, raw.currencyId) || raw.currency || 'SAR',
      country: this.getLookupName(this.countries, raw.countryId) || raw.country || ''
    }, { emitEvent: false });
  }

  private syncAllLookupNames(): void {
    this.syncPersonalLookupNames();
    this.educations.controls.forEach((_, index) => this.syncEducationLookupNames(index));
    this.experiences.controls.forEach((_, index) => this.syncExperienceLookupNames(index));
  }

  private getSectionLookupKey(section: 'education' | 'experience', index: number, controlName: string): string {
    return `${section}:${index}:${controlName}`;
  }

  private clearControlError(controlName: string, errorKey: string): void {
    const control = this.personalForm.get(controlName);
    if (!control || !control.errors?.[errorKey]) return;
    const { [errorKey]: _, ...rest } = control.errors;
    control.setErrors(Object.keys(rest).length ? rest : null);
  }

  private setParsedLookupMismatch(controlName: 'residenceCountryId' | 'residenceRegionId' | 'residenceCityId' | 'residenceDistrictId' | 'residenceDistrictCodeId' | 'nationalityId', parsedValue?: string): void {
    const control = this.personalForm.get(controlName);
    if (!control) return;
    this.parsedLookupRawValues[controlName] = (parsedValue ?? this.parsedLookupRawValues[controlName] ?? '').trim();
    control.setErrors({ ...(control.errors ?? {}), parsedNotMatched: true });
    control.markAsTouched();
  }

  private setParsedLookupMissing(controlName: 'residenceCountryId' | 'residenceRegionId' | 'residenceCityId' | 'residenceDistrictId' | 'residenceDistrictCodeId' | 'nationalityId'): void {
    const control = this.personalForm.get(controlName);
    if (!control || !this.hasNewCvParsed) return;
    control.setErrors({ ...(control.errors ?? {}), parsedMissing: true });
    control.markAsTouched();
  }

  private clearSectionControlError(group: FormGroup, controlName: string, errorKey: string): void {
    const control = group.get(controlName);
    if (!control || !control.errors?.[errorKey]) return;
    const { [errorKey]: _, ...rest } = control.errors;
    control.setErrors(Object.keys(rest).length ? rest : null);
  }

  private setParsedSectionLookupMismatch(
    section: 'education' | 'experience',
    index: number,
    group: FormGroup,
    controlName: string,
    parsedValue?: string
  ): void {
    const control = group.get(controlName);
    if (!control) return;
    const key = this.getSectionLookupKey(section, index, controlName);
    const store = section === 'education' ? this.parsedEducationLookupRawValues : this.parsedExperienceLookupRawValues;
    store[key] = (parsedValue ?? store[key] ?? '').trim();
    control.setErrors({ ...(control.errors ?? {}), parsedNotMatched: true });
    control.markAsTouched();
  }

  private setParsedSectionLookupMissing(
    section: 'education' | 'experience',
    index: number,
    group: FormGroup,
    controlName: string
  ): void {
    if (!this.hasNewCvParsed) return;
    const control = group.get(controlName);
    if (!control) return;
    control.setErrors({ ...(control.errors ?? {}), parsedMissing: true });
    control.markAsTouched();
  }

  private getSectionParsedRawValue(section: 'education' | 'experience', index: number, controlName: string): string {
    const key = this.getSectionLookupKey(section, index, controlName);
    const store = section === 'education' ? this.parsedEducationLookupRawValues : this.parsedExperienceLookupRawValues;
    const value = (store[key] ?? '').trim();
    return value.length > 0 ? value : 'N/A';
  }

  private getParsedRawValue(controlName: 'residenceCountryId' | 'residenceRegionId' | 'residenceCityId' | 'residenceDistrictId' | 'residenceDistrictCodeId' | 'nationalityId'): string {
    const value = (this.parsedLookupRawValues[controlName] ?? '').trim();
    return value.length > 0 ? value : 'N/A';
  }

  private getParsedMandatoryRawValue(controlName: 'fullName' | 'email' | 'mobileNumber' | 'residenceCountryId' | 'residenceCityId'): string {
    const value = (this.parsedMandatoryRawValues[controlName] ?? '').trim();
    return value.length > 0 ? value : 'N/A';
  }

  getMandatoryFieldError(controlName: 'fullName' | 'email' | 'mobileNumber' | 'residenceCountryId' | 'residenceCityId'): string {
    const control = this.personalForm.get(controlName);
    if (!control) return '';

    if (controlName === 'email' && control.hasError('email') && (control.touched || control.dirty)) {
      if (this.hasNewCvParsed) {
        return `Parsed email is invalid. Parsed value: "${this.getParsedMandatoryRawValue('email')}". Please enter a valid email.`;
      }
      return 'Invalid email format';
    }

    if (!control.hasError('required') || !(control.touched || control.dirty)) {
      return '';
    }

    if (controlName === 'residenceCountryId') {
      return this.getPersonalLookupError('residenceCountryId');
    }

    if (controlName === 'residenceCityId') {
      return this.getPersonalLookupError('residenceCityId');
    }

    if (this.hasNewCvParsed) {
      return `Parsed value is empty or unusable. Parsed value: "${this.getParsedMandatoryRawValue(controlName)}". Please enter/select manually.`;
    }

    if (controlName === 'fullName') return 'Full name is required';
    if (controlName === 'email') return 'Email is required';
    return 'Mobile number is required';
  }

  getPersonalLookupError(controlName: 'residenceCountryId' | 'residenceRegionId' | 'residenceCityId' | 'residenceDistrictId' | 'residenceDistrictCodeId' | 'nationalityId'): string {
    const control = this.personalForm.get(controlName);
    if (!control) return '';

    if (control.hasError('parsedNotMatched')) {
      if (controlName === 'residenceCountryId') {
        return `Parsed country not found in DB. Parsed value: "${this.getParsedRawValue('residenceCountryId')}". Please select from dropdown.`;
      }
      if (controlName === 'residenceRegionId') {
        return `Parsed region not found in DB. Parsed value: "${this.getParsedRawValue('residenceRegionId')}". Please select from dropdown.`;
      }
      if (controlName === 'residenceCityId') {
        return `Parsed city not found in DB. Parsed value: "${this.getParsedRawValue('residenceCityId')}". Please select from dropdown.`;
      }
      if (controlName === 'residenceDistrictId') {
        return `Parsed district not found in DB. Parsed value: "${this.getParsedRawValue('residenceDistrictId')}". Please select from dropdown.`;
      }
      if (controlName === 'residenceDistrictCodeId') {
        return `Parsed additional number not found in DB. Parsed value: "${this.getParsedRawValue('residenceDistrictCodeId')}". Please select from dropdown.`;
      }
      return `Parsed nationality not found in DB. Parsed value: "${this.getParsedRawValue('nationalityId')}". Please select from dropdown.`;
    }

    if (control.hasError('parsedMissing')) {
      if (controlName === 'residenceCountryId') return 'Parsed country value not found in CV. Please select from dropdown.';
      if (controlName === 'residenceRegionId') return 'Parsed region value not found in CV. Please select from dropdown.';
      if (controlName === 'residenceCityId') return 'Parsed city value not found in CV. Please select from dropdown.';
      if (controlName === 'residenceDistrictId') return 'Parsed district value not found in CV. Please select from dropdown.';
      if (controlName === 'residenceDistrictCodeId') return 'Parsed additional number not found in CV. Please select from dropdown.';
      return 'Parsed nationality value not found in CV. Please select from dropdown.';
    }

    if (controlName === 'residenceCountryId' && control.hasError('required')) return 'Residence country is required.';
    if (controlName === 'residenceCityId' && control.hasError('required')) return 'Residence city is required.';
    return '';
  }

  private inferQualificationKind(qualification: string | null | undefined, matchedDegree: boolean, matchedCertificate: boolean): 'degree' | 'certificate' | null {
    if (matchedDegree) return 'degree';
    if (matchedCertificate) return 'certificate';

    const normalized = this.normalizeLookupValue(qualification);
    if (!normalized) return null;

    if (/(certificate|certification|license|licence|cert|شهاده|شهادة مهنية|رخصه|رخصة)/.test(normalized)) {
      return 'certificate';
    }

    if (/(degree|diploma|bachelor|master|phd|doctorate|associate|mba|بكالوريوس|ماجستير|دكتوراه|دبلوم|جامعي)/.test(normalized)) {
      return 'degree';
    }

    return null;
  }

  hasEducationLookupIssue(index: number, controlName: 'qualificationTypeId' | 'degreeId' | 'certificateId' | 'majorFieldOfStudyId' | 'institutionId' | 'countryId'): boolean {
    const control = this.getEducationGroup(index).get(controlName);
    return !!control && (control.touched || control.dirty) && !!this.getEducationLookupError(index, controlName);
  }

  hasExperienceLookupIssue(index: number, controlName: 'currencyId' | 'countryId'): boolean {
    const control = this.getExperienceGroup(index).get(controlName);
    return !!control && (control.touched || control.dirty) && !!this.getExperienceLookupError(index, controlName);
  }

  getEducationLookupError(index: number, controlName: 'qualificationTypeId' | 'degreeId' | 'certificateId' | 'majorFieldOfStudyId' | 'institutionId' | 'countryId'): string {
    const group = this.getEducationGroup(index);
    const control = group.get(controlName);
    if (!control) return '';
    const raw = group.getRawValue();

    if (control.hasError('parsedNotMatched')) {
      if (controlName === 'qualificationTypeId') return `Parsed qualification not found in DB. Parsed value: "${this.getSectionParsedRawValue('education', index, 'qualificationTypeId')}". Please select qualification type carefully.`;
      if (controlName === 'degreeId') return `Parsed degree not found in DB. Parsed value: "${this.getSectionParsedRawValue('education', index, 'degreeId')}". Please select degree from dropdown.`;
      if (controlName === 'certificateId') return `Parsed certificate not found in DB. Parsed value: "${this.getSectionParsedRawValue('education', index, 'certificateId')}". Please select certificate from dropdown.`;
      if (controlName === 'majorFieldOfStudyId') return `Parsed major not found in DB. Parsed value: "${this.getSectionParsedRawValue('education', index, 'majorFieldOfStudyId')}". Please select major from dropdown.`;
      if (controlName === 'institutionId') return `Parsed institution not found in DB. Parsed value: "${this.getSectionParsedRawValue('education', index, 'institutionId')}". Please select institution from dropdown.`;
      return `Parsed education country not found in DB. Parsed value: "${this.getSectionParsedRawValue('education', index, 'countryId')}". Please select country from dropdown.`;
    }

    if (control.hasError('parsedMissing')) {
      if (controlName === 'qualificationTypeId') return 'Parsed qualification value not found in CV. Please select qualification type.';
      if (controlName === 'degreeId') return 'Parsed degree value not found in CV. Please select degree.';
      if (controlName === 'certificateId') return 'Parsed certificate value not found in CV. Please select certificate.';
      if (controlName === 'majorFieldOfStudyId') return 'Parsed major value not found in CV. Please select major.';
      if (controlName === 'institutionId') return 'Parsed institution value not found in CV. Please select institution.';
      return 'Parsed education country value not found in CV. Please select country.';
    }

    if (controlName === 'qualificationTypeId' && (raw.qualification ?? '').toString().trim() && !raw.qualificationTypeId) {
      return 'Please select qualification type from dropdown.';
    }
    if (controlName === 'degreeId' && !this.isCertificateType(raw.qualificationTypeId) && (raw.qualification ?? '').toString().trim() && !raw.degreeId) {
      return 'Please select degree from dropdown.';
    }
    if (controlName === 'certificateId' && this.isCertificateType(raw.qualificationTypeId) && (raw.qualification ?? '').toString().trim() && !raw.certificateId) {
      return 'Please select certificate from dropdown.';
    }
    if (controlName === 'majorFieldOfStudyId' && (raw.major ?? '').toString().trim() && !raw.majorFieldOfStudyId) {
      return 'Please select major from dropdown.';
    }
    if (controlName === 'institutionId' && (raw.institution ?? '').toString().trim() && !raw.institutionId) {
      return 'Please select institution from dropdown.';
    }
    if (controlName === 'countryId' && (raw.country ?? '').toString().trim() && !raw.countryId) {
      return 'Please select country from dropdown.';
    }

    return '';
  }

  getExperienceLookupError(index: number, controlName: 'currencyId' | 'countryId'): string {
    const group = this.getExperienceGroup(index);
    const control = group.get(controlName);
    if (!control) return '';
    const raw = group.getRawValue();

    if (control.hasError('parsedNotMatched')) {
      if (controlName === 'currencyId') return `Parsed currency not found in DB. Parsed value: "${this.getSectionParsedRawValue('experience', index, 'currencyId')}". Please select currency from dropdown.`;
      return `Parsed experience country not found in DB. Parsed value: "${this.getSectionParsedRawValue('experience', index, 'countryId')}". Please select country from dropdown.`;
    }

    if (control.hasError('parsedMissing')) {
      if (controlName === 'currencyId') return 'Parsed currency value not found in CV. Please select currency.';
      return 'Parsed experience country value not found in CV. Please select country.';
    }

    if (controlName === 'currencyId' && (raw.currency ?? '').toString().trim() && !raw.currencyId) {
      return 'Please select currency from dropdown.';
    }
    if (controlName === 'countryId' && (raw.country ?? '').toString().trim() && !raw.countryId) {
      return 'Please select country from dropdown.';
    }

    return '';
  }

  get hasPendingLookupSelections(): boolean {
    const pv = this.personalForm.getRawValue();
    if (!pv.residenceCountryId || !pv.residenceCityId) return true;
    if ((pv.nationality ?? '').toString().trim() && !pv.nationalityId) return true;

    for (const control of this.educations.controls) {
      const e = (control as FormGroup).getRawValue();
      const hasContent = !!(e.qualification || e.major || e.institution || e.country || e.graduationYear || e.gradeOrGPA);
      if (!hasContent) continue;

      if (!e.qualificationTypeId) return true;
      if (this.isCertificateType(e.qualificationTypeId)) {
        if ((e.qualification ?? '').toString().trim() && !e.certificateId) return true;
      } else {
        if ((e.qualification ?? '').toString().trim() && !e.degreeId) return true;
      }
      if ((e.major ?? '').toString().trim() && !e.majorFieldOfStudyId) return true;
      if ((e.institution ?? '').toString().trim() && !e.institutionId) return true;
      if ((e.country ?? '').toString().trim() && !e.countryId) return true;
    }

    for (const control of this.experiences.controls) {
      const e = (control as FormGroup).getRawValue();
      const hasContent = !!(e.jobTitle || e.employer || e.country || e.description || e.salary || e.startDate || e.endDate);
      if (!hasContent) continue;
      if ((e.currency ?? '').toString().trim() && !e.currencyId) return true;
      if ((e.country ?? '').toString().trim() && !e.countryId) return true;
    }

    return false;
  }

  private markLookupControlsAsTouched(): void {
    this.personalForm.get('nationalityId')?.markAsTouched();
    this.personalForm.get('residenceCountryId')?.markAsTouched();
    this.personalForm.get('residenceRegionId')?.markAsTouched();
    this.personalForm.get('residenceCityId')?.markAsTouched();
    this.personalForm.get('residenceDistrictId')?.markAsTouched();
    this.personalForm.get('residenceDistrictCodeId')?.markAsTouched();
    this.educations.controls.forEach(control => {
      const group = control as FormGroup;
      ['qualificationTypeId', 'degreeId', 'certificateId', 'majorFieldOfStudyId', 'institutionId', 'countryId']
        .forEach(name => group.get(name)?.markAsTouched());
    });
    this.experiences.controls.forEach(control => {
      const group = control as FormGroup;
      ['currencyId', 'countryId'].forEach(name => group.get(name)?.markAsTouched());
    });
  }

  proceedToEdit(): void {
    this.step = 'edit';
  }

  goBack(): void {
    if (this.step === 'edit') {
      this.step = 'choice';
    } else if (this.step === 'cv-upload') {
      this.step = 'choice';
    } else {
      this.router.navigate(['/profile-details']);
    }
  }

  cancelUpdate(): void {
    this.router.navigate(['/profile-details']);
  }

  private clearOptionalLocationErrorsBeforeSave(): void {
    this.clearControlError('residenceRegionId', 'parsedNotMatched');
    this.clearControlError('residenceRegionId', 'parsedMissing');
    this.clearControlError('residenceDistrictId', 'parsedNotMatched');
    this.clearControlError('residenceDistrictId', 'parsedMissing');
  }

  saveUpdate(): void {
    this.clearOptionalLocationErrorsBeforeSave();

    if (this.personalForm.invalid || this.educationForm.invalid || this.experienceForm.invalid) {
      this.personalForm.markAllAsTouched();
      this.educationForm.markAllAsTouched();
      this.experienceForm.markAllAsTouched();
      this.markLookupControlsAsTouched();
      this.snackBar.open('Please fill in all required fields before saving.', 'Close', { duration: 4000 });
      return;
    }

    if (!this.candidate) return;
    this.saving = true;

    const payload = this.buildCandidatePayload();
    this.candidateService.createCandidate(payload).subscribe({
      next: () => {
        this.saving = false;
        this.snackBar.open('Profile updated successfully!', 'Close', { duration: 4000 });
        
        // If CV was uploaded, run lookup resolution
        if (this.cvFile && this.cvAnalyzed) {
          this.candidateService.uploadCvDocument(this.candidate!.candidateId, this.cvFile).subscribe({
            next: () => {
              this.candidateService.resolveAllLookups(this.candidate!.candidateId).subscribe({
                next: () => {
                  this.router.navigate(['/profile-details']);
                },
                error: () => {
                  this.router.navigate(['/profile-details']);
                }
              });
            },
            error: () => {
              this.router.navigate(['/profile-details']);
            }
          });
        } else {
          this.router.navigate(['/profile-details']);
        }
      },
      error: (err) => {
        this.saving = false;
        const msg = err?.error?.message || 'Failed to update profile';
        this.snackBar.open(msg, 'Close', { duration: 4000 });
      }
    });
  }

  private buildCandidatePayload(): CreateCandidateDto {
    const pv = this.personalForm.getRawValue();

    const nationality = this.getLookupName(this.nationalities, pv.nationalityId) || pv.nationality || undefined;
    const residenceCity = this.getLookupName(this.cities, pv.residenceCityId) || pv.residenceCity || undefined;
    const eduVal: CreateEducationDto[] = this.educations.getRawValue()
      .map((e: any) => ({
        qualification: (this.isCertificateType(e.qualificationTypeId)
          ? this.getLookupName(this.certificates, e.certificateId)
          : this.getLookupName(this.degrees, e.degreeId)) || this.getLookupName(this.qualificationTypes, e.qualificationTypeId) || e.qualification || '',
        qualificationTypeId: e.qualificationTypeId || undefined,
        major: this.getLookupName(this.majors, e.majorFieldOfStudyId) || e.major || undefined,
        institution: this.getLookupName(this.getInstitutionsForCountry(e.countryId ?? null), e.institutionId)
          || this.getLookupName(this.institutions, e.institutionId)
          || e.institution
          || undefined,
        graduationYear: e.graduationYear || undefined,
        gradeOrGPA: e.gradeOrGPA || undefined,
        country: this.getLookupName(this.countries, e.countryId) || e.country || undefined,
        degreeId: e.degreeId || undefined,
        certificateId: e.certificateId || undefined,
        majorFieldOfStudyId: e.majorFieldOfStudyId || undefined,
        institutionId: e.institutionId || undefined,
        countryId: e.countryId || undefined
      }))
      .filter((e: CreateEducationDto) => !!(e.qualification || e.institution || e.major || e.country));

    const expVal: CreateExperienceDto[] = this.experiences.getRawValue()
      .map((e: any) => ({
        employer: e.employer || undefined,
        jobTitle: e.jobTitle || undefined,
        startDate: e.startDate ? new Date(e.startDate).toISOString().split('T')[0] : undefined,
        endDate: e.endDate ? new Date(e.endDate).toISOString().split('T')[0] : undefined,
        isCurrent: !!e.isCurrent,
        salary: e.salary || undefined,
        currency: this.getLookupName(this.currencies, e.currencyId) || e.currency || undefined,
        currencyId: e.currencyId || undefined,
        description: e.description || undefined,
        country: this.getLookupName(this.countries, e.countryId) || e.country || undefined,
        countryId: e.countryId || undefined
      }))
      .filter((e: CreateExperienceDto) => !!(e.employer || e.jobTitle || e.country || e.currency || e.description));

    return {
      fullName: pv.fullName,
      email: pv.email,
      mobileNumber: pv.mobileNumber,
      nationalId: pv.nationalId || undefined,
      idType: pv.idType,
      gender: pv.gender,
      dateOfBirth: pv.dateOfBirth ? new Date(pv.dateOfBirth).toISOString().split('T')[0] : undefined,
      nationality,
      nationalityId: pv.nationalityId || undefined,
      nationalAddress: pv.nationalAddress || undefined,
      residenceCity,
      residenceCityId: pv.residenceCityId || undefined,
      residenceCountryId: pv.residenceCountryId || undefined,
      residenceRegionId: pv.residenceRegionId || undefined,
      residenceDistrictId: pv.residenceDistrictId || undefined,
      residenceDistrictCodeId: pv.residenceDistrictCodeId || undefined,
      postalCode: pv.postalCode || undefined,
      buildingNumber: pv.buildingNumber || undefined,
      street: pv.street || undefined,
      educations: eduVal,
      experiences: expVal
    };
  }

  isCertificateType(qualificationTypeId: number | null | undefined): boolean {
    if (!qualificationTypeId) return false;
    const type = this.qualificationTypes.find(q => q.id === qualificationTypeId);
    return (type?.name ?? '').toLowerCase().includes('certificate') || qualificationTypeId === 2;
  }

  getInstitutionsForCountry(countryId: number | string | null): LookupItemDto[] {
    const source = this.universities.length > 0 ? this.universities : this.institutions;
    const normalizedCountryId = this.normalizeId(countryId);
    if (!normalizedCountryId) return source;
    return source.filter(inst => this.isOtherLookupItem(inst) || !inst.parentId || inst.parentId === normalizedCountryId);
  }

  addEducation(): void {
    this.educations.push(this.fb.group({
      candidateEducationId: [null],
      candidateQualificationId: [null],
      qualification: ['', Validators.required],
      qualificationTypeId: [null],
      major: [''],
      institution: [''],
      graduationYear: [null],
      gradeOrGPA: [''],
      country: [''],
      degreeId: [null],
      certificateId: [null],
      majorFieldOfStudyId: [null],
      institutionId: [null],
      countryId: [null]
    }));
  }

  removeEducation(i: number): void { this.educations.removeAt(i); }

  addExperience(): void {
    this.experiences.push(this.fb.group({
      candidateExperienceId: [null],
      jobTitle: [''],
      employer: [''],
      startDate: [null],
      endDate: [null],
      isCurrent: [false],
      salary: [null],
      currency: ['SAR'],
      currencyId: [null],
      country: [''],
      countryId: [null],
      description: ['']
    }));
  }

  removeExperience(i: number): void { this.experiences.removeAt(i); }

  getEducationGroup(i: number): FormGroup { return this.educations.at(i) as FormGroup; }
  getExperienceGroup(i: number): FormGroup { return this.experiences.at(i) as FormGroup; }

  onEducationQualificationTypeChange(qualificationTypeId: number | null, i: number): void {
    const group = this.getEducationGroup(i);
    this.clearSectionControlError(group, 'qualificationTypeId', 'parsedNotMatched');
    this.clearSectionControlError(group, 'qualificationTypeId', 'parsedMissing');
    group.patchValue({ qualificationTypeId: qualificationTypeId ?? null });
    if (this.isCertificateType(qualificationTypeId)) {
      group.patchValue({ degreeId: null });
      this.clearSectionControlError(group, 'degreeId', 'parsedNotMatched');
      this.clearSectionControlError(group, 'degreeId', 'parsedMissing');
    } else {
      group.patchValue({ certificateId: null });
      this.clearSectionControlError(group, 'certificateId', 'parsedNotMatched');
      this.clearSectionControlError(group, 'certificateId', 'parsedMissing');
    }
  }

  onEducationDegreeChange(degreeId: number | null, i: number): void {
    const group = this.getEducationGroup(i);
    this.clearSectionControlError(group, 'degreeId', 'parsedNotMatched');
    this.clearSectionControlError(group, 'degreeId', 'parsedMissing');
    const selected = this.degrees.find(item => item.id === degreeId);
    group.patchValue({ degreeId: degreeId ?? null, qualification: selected?.name ?? group.get('qualification')?.value ?? '' });
  }

  onEducationCertificateChange(certificateId: number | null, i: number): void {
    const group = this.getEducationGroup(i);
    this.clearSectionControlError(group, 'certificateId', 'parsedNotMatched');
    this.clearSectionControlError(group, 'certificateId', 'parsedMissing');
    const selected = this.certificates.find(item => item.id === certificateId);
    group.patchValue({ certificateId: certificateId ?? null, qualification: selected?.name ?? group.get('qualification')?.value ?? '' });
  }

  onEducationMajorSelected(item: number | null, i: number): void {
    const group = this.getEducationGroup(i);
    this.clearSectionControlError(group, 'majorFieldOfStudyId', 'parsedNotMatched');
    this.clearSectionControlError(group, 'majorFieldOfStudyId', 'parsedMissing');
    const selected = this.majors.find(x => x.id === item);
    group.patchValue({ major: selected?.name ?? '', majorFieldOfStudyId: selected?.id ?? null });
  }

  onEducationInstitutionSelected(item: number | null, i: number): void {
    const group = this.getEducationGroup(i);
    this.clearSectionControlError(group, 'institutionId', 'parsedNotMatched');
    this.clearSectionControlError(group, 'institutionId', 'parsedMissing');
    const countryId = group.get('countryId')?.value as number | null;
    const selected = this.getInstitutionsForCountry(countryId).find(x => x.id === item)
      ?? this.institutions.find(x => x.id === item);
    group.patchValue({ institution: selected?.name ?? '', institutionId: selected?.id ?? null });
  }

  onEducationCountrySelected(item: number | null, i: number): void {
    const group = this.getEducationGroup(i);
    this.clearSectionControlError(group, 'countryId', 'parsedNotMatched');
    this.clearSectionControlError(group, 'countryId', 'parsedMissing');
    const selected = this.countries.find(x => x.id === item);
    group.patchValue({ country: selected?.name ?? '', countryId: selected?.id ?? null });

    const currentInstitutionId = group.get('institutionId')?.value as number | null;
    const allowedInstitutions = this.getInstitutionsForCountry(selected?.id ?? null);
    if (currentInstitutionId && !allowedInstitutions.some(inst => inst.id === currentInstitutionId)) {
      group.patchValue({ institutionId: null, institution: '' });
    }
  }

  onExperienceCurrencyChange(currencyId: number | null, i: number): void {
    const group = this.getExperienceGroup(i);
    this.clearSectionControlError(group, 'currencyId', 'parsedNotMatched');
    this.clearSectionControlError(group, 'currencyId', 'parsedMissing');
    const selected = this.currencies.find(x => x.id === currencyId);
    group.patchValue({ currency: selected?.name ?? 'SAR', currencyId: selected?.id ?? null });
  }

  onExperienceCountrySelected(item: number | null, i: number): void {
    const group = this.getExperienceGroup(i);
    this.clearSectionControlError(group, 'countryId', 'parsedNotMatched');
    this.clearSectionControlError(group, 'countryId', 'parsedMissing');
    const selected = this.countries.find(x => x.id === item);
    group.patchValue({ country: selected?.name ?? '', countryId: selected?.id ?? null });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile = input.files[0];
      this.showUploadForm = true;
    }
  }

  uploadDocument(): void {
    if (!this.selectedFile || !this.candidate) return;
    this.uploading = true;
    this.candidateService.uploadDocument(this.candidate.candidateId, this.selectedFile, this.selectedDocType).subscribe({
      next: () => {
        this.uploading = false;
        this.showUploadForm = false;
        this.selectedFile = null;
        this.snackBar.open('Document uploaded successfully!', 'Close', { duration: 4000 });
        this.loadProfile();
      },
      error: (err) => {
        this.uploading = false;
        this.snackBar.open(err?.error?.message || 'Failed to upload document', 'Close', { duration: 4000 });
      }
    });
  }

  cancelUpload(): void {
    this.showUploadForm = false;
    this.selectedFile = null;
  }

  deleteDocument(docId: number): void {
    if (!this.candidate) return;
    this.candidateService.deleteDocument(this.candidate.candidateId, docId).subscribe({
      next: () => {
        this.snackBar.open('Document deleted.', 'Close', { duration: 3000 });
        this.loadProfile();
      },
      error: (err) => {
        this.snackBar.open(err?.error?.message || 'Failed to delete document', 'Close', { duration: 4000 });
      }
    });
  }

  getDocTypeName(type: number): string {
    return DocumentTypeLabels[type] || 'Other';
  }

  getLookupDisplayName(item: LookupItemDto | string | null | undefined): string {
    if (!item) return '';
    if (typeof item === 'string') return item;
    return this.lookupDisplayLanguage === 'ar' && (item.nameAr ?? '').trim().length > 0
      ? item.nameAr!
      : item.name;
  }

  isMandatoryFieldInvalid(controlName: 'fullName' | 'email' | 'mobileNumber' | 'residenceCountryId' | 'residenceCityId'): boolean {
    const control = this.personalForm.get(controlName);
    if (!control) return false;
    return control.invalid && (control.touched || control.dirty);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1048576).toFixed(1) + ' MB';
  }
}

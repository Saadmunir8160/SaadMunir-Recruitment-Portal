import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CandidateService } from '../../../core/services/candidate.service';
import { LookupService } from '../../../core/services/lookup.service';
import { CandidateDetailDto, LookupItemDto } from '../../../shared/interfaces/models';

@Component({
  selector: 'app-profile-details',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './profile-details.component.html',
  styleUrl: './profile-details.component.scss'
})
export class ProfileDetailsComponent implements OnInit {
  candidate: CandidateDetailDto | null = null;
  loading = false;
  profileMissing = false;
  lookupDisplayLanguage: 'en' | 'ar' = 'en';
  private userSelectedLanguage = false;

  countries: LookupItemDto[] = [];
  regions: LookupItemDto[] = [];
  cities: LookupItemDto[] = [];
  districts: LookupItemDto[] = [];
  districtCodes: LookupItemDto[] = [];
  nationalities: LookupItemDto[] = [];

  private readonly labels: Record<'en' | 'ar', Record<string, string>> = {
    en: {
      profileDetails: 'Profile Details',
      back: 'Back',
      personalInfo: 'Personal Information',
      residenceInfo: 'Residence Information',
      fullName: 'Full Name',
      email: 'Email',
      mobileNumber: 'Mobile Number',
      gender: 'Gender',
      dateOfBirth: 'Date of Birth',
      nationalId: 'National ID',
      idType: 'ID Type',
      nationality: 'Nationality',
      nationalAddress: 'National Address',
      residenceCountry: 'Residence Country',
      region: 'Region',
      city: 'City',
      district: 'District',
      additionalNumber: 'Additional Number',
      postalCode: 'Postal Code',
      buildingNumber: 'Building Number',
      street: 'Street',
      education: 'Education',
      institution: 'Institution',
      major: 'Major',
      graduationYear: 'Graduation Year',
      gradeGpa: 'Grade/GPA',
      country: 'Country',
      workExperience: 'Work Experience',
      startDate: 'Start Date',
      endDate: 'End Date',
      salary: 'Salary',
      description: 'Description',
      documents: 'Documents',
      updateProfile: 'Update Profile',
      backToJobs: 'Back to Jobs',
      currentlyWorking: 'Currently working',
      noProfileFound: 'No profile found',
      createProfilePrompt: 'Create your profile to continue.',
      createProfile: 'Create Profile',
      male: 'Male',
      female: 'Female',
      nationalIdType: 'National ID',
      iqama: 'Iqama',
      passport: 'Passport',
      borderNumber: 'Border Number'
    },
    ar: {
      profileDetails: 'تفاصيل الملف الشخصي',
      back: 'رجوع',
      personalInfo: 'المعلومات الشخصية',
      residenceInfo: 'معلومات السكن',
      fullName: 'الاسم الكامل',
      email: 'البريد الإلكتروني',
      mobileNumber: 'رقم الجوال',
      gender: 'الجنس',
      dateOfBirth: 'تاريخ الميلاد',
      nationalId: 'رقم الهوية',
      idType: 'نوع الهوية',
      nationality: 'الجنسية',
      nationalAddress: 'العنوان الوطني',
      residenceCountry: 'دولة الإقامة',
      region: 'المنطقة',
      city: 'المدينة',
      district: 'الحي',
      additionalNumber: 'الرقم الإضافي',
      postalCode: 'الرمز البريدي',
      buildingNumber: 'رقم المبنى',
      street: 'الشارع',
      education: 'المؤهلات العلمية',
      institution: 'المؤسسة التعليمية',
      major: 'التخصص',
      graduationYear: 'سنة التخرج',
      gradeGpa: 'المعدل',
      country: 'الدولة',
      workExperience: 'الخبرات العملية',
      startDate: 'تاريخ البداية',
      endDate: 'تاريخ النهاية',
      salary: 'الراتب',
      description: 'الوصف',
      documents: 'المستندات',
      updateProfile: 'تحديث الملف الشخصي',
      backToJobs: 'العودة للوظائف',
      currentlyWorking: 'يعمل حاليا',
      noProfileFound: 'لا يوجد ملف شخصي',
      createProfilePrompt: 'قم بإنشاء ملفك الشخصي للمتابعة.',
      createProfile: 'إنشاء ملف شخصي',
      male: 'ذكر',
      female: 'أنثى',
      nationalIdType: 'هوية وطنية',
      iqama: 'إقامة',
      passport: 'جواز سفر',
      borderNumber: 'رقم الحدود'
    }
  };

  constructor(
    private candidateService: CandidateService,
    private lookupService: LookupService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.loadProfile();
  }

  private loadLookups(): void {
    this.lookupService.getCountries().subscribe({ next: (res) => { this.countries = res.data || []; } });
    this.lookupService.getRegions().subscribe({ next: (res) => { this.regions = res.data || []; } });
    this.lookupService.getCities().subscribe({ next: (res) => { this.cities = res.data || []; } });
    this.lookupService.getDistricts().subscribe({ next: (res) => { this.districts = res.data || []; } });
    this.lookupService.getDistrictCodes().subscribe({ next: (res) => { this.districtCodes = res.data || []; } });
    this.lookupService.getNationalities().subscribe({ next: (res) => { this.nationalities = res.data || []; } });
  }

  loadProfile(): void {
    this.loading = true;
    this.profileMissing = false;
    this.candidateService.getProfile().subscribe({
      next: (res) => {
        this.candidate = res.data;
        if (!this.userSelectedLanguage) {
          this.lookupDisplayLanguage = this.detectPreferredLanguage();
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.candidate = null;
        if (err?.status === 404) {
          this.profileMissing = true;
          return;
        }
        this.snackBar.open('Failed to load profile', 'Close', { duration: 4000 });
      }
    });
  }

  setLookupLanguage(lang: 'en' | 'ar'): void {
    this.lookupDisplayLanguage = lang;
    this.userSelectedLanguage = true;
  }

  t(key: string): string {
    return this.labels[this.lookupDisplayLanguage][key] ?? key;
  }

  private containsArabicText(value: string | null | undefined): boolean {
    return /[\u0600-\u06FF]/.test(value ?? '');
  }

  private detectPreferredLanguage(): 'en' | 'ar' {
    const c = this.candidate;
    if (!c) return 'en';

    const values: Array<string | null | undefined> = [
      c.fullName,
      c.nationality,
      c.nationalAddress,
      c.residenceCity,
      c.street,
      c.postalCode
    ];

    return values.some(value => this.containsArabicText(value)) ? 'ar' : 'en';
  }

  private getLookupDisplayName(item: LookupItemDto | null | undefined): string {
    if (!item) return '-';
    if (this.lookupDisplayLanguage === 'ar' && (item.nameAr ?? '').trim().length > 0) {
      return item.nameAr!;
    }
    return item.name;
  }

  private resolveLookupById(list: LookupItemDto[], id: number | null | undefined): string {
    if (!id) return '-';
    const item = list.find(x => x.id === id);
    return this.getLookupDisplayName(item);
  }

  getResidenceCountryDisplay(): string {
    return this.resolveLookupById(this.countries, this.candidate?.residenceCountryId ?? null);
  }

  getRegionDisplay(): string {
    return this.resolveLookupById(this.regions, this.candidate?.residenceRegionId ?? null);
  }

  getDistrictDisplay(): string {
    return this.resolveLookupById(this.districts, this.candidate?.residenceDistrictId ?? null);
  }

  getDistrictCodeDisplay(): string {
    return this.resolveLookupById(this.districtCodes, this.candidate?.residenceDistrictCodeId ?? null);
  }

  getNationalityDisplay(): string {
    const c = this.candidate;
    if (!c) return '-';

    if (c.nationalityId) {
      const resolved = this.resolveLookupById(this.nationalities, c.nationalityId);
      if (resolved !== '-') return resolved;
    }

    return c.nationality || '-';
  }

  goToCreate(): void {
    this.router.navigate(['/profile']);
  }

  goToUpdate(): void {
    if (!this.candidate) return;
    this.router.navigate(['/profile-update']);
  }

  goBack(): void {
    this.router.navigate(['/jobs']);
  }

  getGenderLabel(gender: number | null): string {
    switch (gender) {
      case 0: return this.t('male');
      case 1: return this.t('female');
      default: return '-';
    }
  }

  getIdTypeLabel(idType: number | null): string {
    switch (idType) {
      case 0: return this.t('nationalIdType');
      case 1: return this.t('iqama');
      case 2: return this.t('passport');
      case 3: return this.t('borderNumber');
      default: return '-';
    }
  }

  formatDate(date: string | null): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1048576).toFixed(1) + ' MB';
  }
}

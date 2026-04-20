import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { ProfileComponent } from './profile.component';
import { CandidateService } from '../../../core/services/candidate.service';
import { LookupService } from '../../../core/services/lookup.service';
import { LookupItemDto, ApiResponse } from '../../../shared/interfaces/models';

describe('ProfileComponent - Dropdown Flow', () => {
  let component: ProfileComponent;
  let fixture: ComponentFixture<ProfileComponent>;
  let lookupService: jasmine.SpyObj<LookupService>;
  let candidateService: jasmine.SpyObj<CandidateService>;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  const mockCountries: LookupItemDto[] = [
    { id: 1, name: 'Saudi Arabia', nameAr: 'المملكة العربية السعودية', parentId: null },
    { id: 2, name: 'UAE', nameAr: 'الامارات', parentId: null }
  ];

  const mockRegions: LookupItemDto[] = [
    { id: 1, name: 'Riyadh Region', nameAr: 'منطقة الرياض', parentId: 1 },
    { id: 2, name: 'Mecca Region', nameAr: 'منطقة مكة', parentId: 1 },
    { id: 3, name: 'Abu Dhabi Region', nameAr: 'منطقة أبو ظبي', parentId: 2 }
  ];

  const mockCities: LookupItemDto[] = [
    { id: 1, name: 'Riyadh', nameAr: 'الرياض', parentId: 1 },
    { id: 2, name: 'Jeddah', nameAr: 'جدة', parentId: 1 },
    { id: 3, name: 'Abu Dhabi', nameAr: 'أبو ظبي', parentId: 2 }
  ];

  const mockDistricts: LookupItemDto[] = [
    { id: 1, name: 'Al-Olaya', nameAr: 'العليا', parentId: 1 },
    { id: 2, name: 'Al-Malaz', nameAr: 'الملز', parentId: 1 },
    { id: 3, name: 'Al-Nuzha', nameAr: 'النزهة', parentId: 2 }
  ];

  const mockApiResponse = <T,>(data: T): ApiResponse<T> => ({
    success: true,
    message: '',
    data
  });

  beforeEach(async () => {
    const lookupServiceSpy = jasmine.createSpyObj('LookupService', [
      'getNationalities',
      'getCountries',
      'getRegions',
      'getCities',
      'getDistricts',
      'getDistrictCodes',
      'getQualificationTypes',
      'getDegrees',
      'getCertificates',
      'getMajors',
      'getInstitutions',
      'getUniversities',
      'getCurrencies'
    ]);

    const candidateServiceSpy = jasmine.createSpyObj('CandidateService', ['getProfile']);
    const snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    // Set default return values
    lookupServiceSpy.getNationalities.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getCountries.and.returnValue(of(mockApiResponse(mockCountries)));
    lookupServiceSpy.getRegions.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getCities.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getDistricts.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getDistrictCodes.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getQualificationTypes.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getDegrees.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getCertificates.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getMajors.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getInstitutions.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getUniversities.and.returnValue(of(mockApiResponse([])));
    lookupServiceSpy.getCurrencies.and.returnValue(of(mockApiResponse([])));

    candidateServiceSpy.getProfile.and.returnValue(of(mockApiResponse({
      candidateId: 0,
      fullName: '',
      email: '',
      mobileNumber: '',
      profileStatus: 0,
      isProfileLocked: false,
      documents: []
    })));

    await TestBed.configureTestingModule({
      imports: [ProfileComponent, ReactiveFormsModule],
      providers: [
        { provide: LookupService, useValue: lookupServiceSpy },
        { provide: CandidateService, useValue: candidateServiceSpy },
        { provide: MatSnackBar, useValue: snackBarSpy }
      ]
    }).compileComponents();

    lookupService = TestBed.inject(LookupService) as jasmine.SpyObj<LookupService>;
    candidateService = TestBed.inject(CandidateService) as jasmine.SpyObj<CandidateService>;
    snackBar = TestBed.inject(MatSnackBar) as jasmine.SpyObj<MatSnackBar>;

    fixture = TestBed.createComponent(ProfileComponent);
    component = fixture.componentInstance;
  });

  describe('Country Selection Flow', () => {
    it('should load cities and regions when country is selected', (done) => {
      lookupService.getCities.and.returnValue(of(mockApiResponse(mockCities)));
      lookupService.getRegions.and.returnValue(of(mockApiResponse(mockRegions)));

      fixture.detectChanges();

      component.onResidenceCountryChange(1);

      setTimeout(() => {
        expect(lookupService.getCities).toHaveBeenCalledWith(1);
        expect(lookupService.getRegions).toHaveBeenCalledWith(1);
        expect(component.cities.length).toBeGreaterThan(0);
        expect(component.regions.length).toBeGreaterThan(0);
        done();
      }, 100);
    });

    it('should reset dependent fields when country is changed', (done) => {
      fixture.detectChanges();

      // Set some initial values
      component.personalForm.patchValue({
        residenceCityId: 5,
        residenceCity: 'Some City',
        residenceRegionId: 3,
        residenceRegion: 'Some Region',
        residenceDistrictId: 7
      });

      component.onResidenceCountryChange(1);

      setTimeout(() => {
        expect(component.personalForm.get('residenceCityId')?.value).toBe(null);
        expect(component.personalForm.get('residenceCity')?.value).toBe('');
        expect(component.personalForm.get('residenceRegionId')?.value).toBe(null);
        expect(component.personalForm.get('residenceRegion')?.value).toBe('');
        expect(component.personalForm.get('residenceDistrictId')?.value).toBe(null);
        done();
      }, 100);
    });

    it('should clear cities, districts, and districtCodes on country change', (done) => {
      fixture.detectChanges();

      component.cities = mockCities;
      component.districts = mockDistricts;
      component.districtCodes = [{ id: 1, name: 'Code 1', nameAr: 'كود 1', parentId: null }];

      component.onResidenceCountryChange(1);

      setTimeout(() => {
        expect(component.cities.length).toBe(0);
        expect(component.districts.length).toBe(0);
        expect(component.districtCodes.length).toBe(0);
        done();
      }, 100);
    });
  });

  describe('City Selection Flow', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.cities = mockCities;
      component.regions = mockRegions;
    });

    it('should load districts when city is selected', (done) => {
      lookupService.getDistricts.and.returnValue(of(mockApiResponse(mockDistricts)));

      component.onCitySelected(1);

      setTimeout(() => {
        expect(lookupService.getDistricts).toHaveBeenCalledWith(1);
        expect(component.districts.length).toBeGreaterThan(0);
        done();
      }, 100);
    });

    it('should update city and residenceCity form fields', () => {
      component.onCitySelected(1);

      expect(component.personalForm.get('residenceCityId')?.value).toBe(1);
      expect(component.personalForm.get('residenceCity')?.value).toBe('Riyadh');
    });

    it('should reset district fields when city is selected', () => {
      component.personalForm.patchValue({
        residenceDistrictId: 5,
        residenceCountryId: 1
      });

      component.onCitySelected(1);

      expect(component.personalForm.get('residenceDistrictId')?.value).toBe(null);
    });
  });

  describe('Region Selection Flow', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.regions = mockRegions;
    });

    it('should update region form fields when region is selected', () => {
      component.onRegionSelected(1);

      expect(component.personalForm.get('residenceRegionId')?.value).toBe(1);
      expect(component.personalForm.get('residenceRegion')?.value).toBe('Riyadh Region');
    });

    it('should handle null region selection', () => {
      component.onRegionSelected(null);

      expect(component.personalForm.get('residenceRegionId')?.value).toBe(null);
      expect(component.personalForm.get('residenceRegion')?.value).toBe('');
    });

    it('should maintain region as optional', () => {
      fixture.detectChanges();

      const regionControl = component.personalForm.get('residenceRegionId');
      expect(regionControl?.validator).toBeNull();
    });
  });

  describe('District Selection Flow', () => {
    it('should load district codes when district is selected', (done) => {
      lookupService.getDistrictCodes.and.returnValue(of(mockApiResponse([
        { id: 1, name: 'Code 1', nameAr: 'كود 1', parentId: null }
      ])));

      fixture.detectChanges();
      component.onResidenceDistrictChange(1);

      setTimeout(() => {
        expect(lookupService.getDistrictCodes).toHaveBeenCalledWith(1);
        expect(component.districtCodes.length).toBeGreaterThan(0);
        done();
      }, 100);
    });

    it('should clear district codes when no district is selected', () => {
      fixture.detectChanges();
      component.districtCodes = [{ id: 1, name: 'Code 1', nameAr: 'كود 1', parentId: null }];

      component.onResidenceDistrictChange(null);

      expect(component.districtCodes.length).toBe(0);
    });
  });

  describe('API Integration', () => {
    it('should call getCities with countryId parameter', () => {
      fixture.detectChanges();

      lookupService.getCities.calls.reset();
      lookupService.getCities.and.returnValue(of(mockApiResponse([])));

      component.onResidenceCountryChange(1);

      expect(lookupService.getCities).toHaveBeenCalledWith(1);
    });

    it('should call getDistricts with cityId parameter', () => {
      fixture.detectChanges();

      lookupService.getDistricts.calls.reset();
      lookupService.getDistricts.and.returnValue(of(mockApiResponse([])));

      component.cities = mockCities;
      component.onCitySelected(1);

      expect(lookupService.getDistricts).toHaveBeenCalledWith(1);
    });

    it('should call getRegions with countryId parameter', () => {
      fixture.detectChanges();

      lookupService.getRegions.calls.reset();
      lookupService.getRegions.and.returnValue(of(mockApiResponse([])));

      component.onResidenceCountryChange(1);

      expect(lookupService.getRegions).toHaveBeenCalledWith(1);
    });

    it('should handle API errors gracefully', (done) => {
      lookupService.getCities.and.returnValue(throwError(() => new Error('API Error')));

      fixture.detectChanges();
      component.onResidenceCountryChange(1);

      setTimeout(() => {
        expect(component.cities.length).toBe(0);
        expect(component.filteredCities.length).toBe(0);
        done();
      }, 100);
    });
  });

  describe('Lookup Display Name', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display English name by default', () => {
      component.lookupDisplayLanguage = 'en';
      const item: LookupItemDto = { id: 1, name: 'Riyadh', nameAr: 'الرياض', parentId: null };

      const displayName = component.getLookupDisplayName(item);

      expect(displayName).toBe('Riyadh');
    });

    it('should display Arabic name when language is set to Arabic', () => {
      component.lookupDisplayLanguage = 'ar';
      const item: LookupItemDto = { id: 1, name: 'Riyadh', nameAr: 'الرياض', parentId: null };

      const displayName = component.getLookupDisplayName(item);

      expect(displayName).toBe('الرياض');
    });
  });
});

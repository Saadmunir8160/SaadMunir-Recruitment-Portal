import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductSearchPopupComponent } from './product-search-popup.component';

describe('ProductSearchPopupComponent', () => {
  let component: ProductSearchPopupComponent;
  let fixture: ComponentFixture<ProductSearchPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductSearchPopupComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductSearchPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddressCreateComponent } from './address-create.component';

describe('AddressCreateComponent', () => {
  let component: AddressCreateComponent;
  let fixture: ComponentFixture<AddressCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddressCreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddressCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

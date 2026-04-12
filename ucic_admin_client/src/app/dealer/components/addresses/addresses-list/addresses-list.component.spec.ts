import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddressesListComponent } from './addresses-list.component';

describe('AddressesListComponent', () => {
  let component: AddressesListComponent;
  let fixture: ComponentFixture<AddressesListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddressesListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddressesListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

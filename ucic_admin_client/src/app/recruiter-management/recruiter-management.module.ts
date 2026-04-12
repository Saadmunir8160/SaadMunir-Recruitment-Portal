import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RecruiterManagementRoutingModule } from './recruiter-management-routing.module';
import { RecruiterManagementComponent } from './recruiter-management.component';

@NgModule({
  imports: [
    CommonModule,
    RecruiterManagementRoutingModule,
    RecruiterManagementComponent
  ]
})
export class RecruiterManagementModule { }

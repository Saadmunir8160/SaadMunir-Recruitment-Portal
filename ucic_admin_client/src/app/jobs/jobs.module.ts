import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobsRoutingModule } from './jobs-routing.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { JobsListComponent } from './jobs-list/jobs-list.component';
import { JobsCreateComponent } from './jobs-create/jobs-create.component';
import { JobsEditComponent } from './jobs-edit/jobs-edit.component';
import { RouterModule } from '@angular/router';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    JobsRoutingModule,
    HttpClientModule,
    RouterModule,
    JobsListComponent,
    JobsCreateComponent,
    JobsEditComponent
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class JobsModule { }

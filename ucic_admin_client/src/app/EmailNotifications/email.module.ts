import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { EmailListComponent } from './email-list/email-list.component';
import { RouterModule } from '@angular/router';
import { EMAIL_ROUTES } from './email.routes';
import { EmailCreateComponent } from './email-create/email-create.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    RouterModule.forChild(EMAIL_ROUTES),
    EmailListComponent,
    EmailCreateComponent
  ]
})

export class EmailModule { }

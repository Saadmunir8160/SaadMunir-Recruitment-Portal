import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmailListComponent } from './email-list/email-list.component';
import { EmailCreateComponent } from './email-create/email-create.component';

const routes: Routes = [
    { path: '', component: EmailListComponent },
        { path: 'create', component: EmailCreateComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmailRoutingModule { }

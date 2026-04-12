import { Routes } from '@angular/router';
import { RoleGuard } from '../guards/role.guard';
import { UserRole } from '../services/role.service';
import { EmailListComponent } from './email-list/email-list.component';
import { EmailCreateComponent } from './email-create/email-create.component';

export const EMAIL_ROUTES: Routes = [
    {
        path: '',
        component: EmailListComponent,
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN] }
      },
        {
          path: 'create',
          component: EmailCreateComponent,
          canActivate: [RoleGuard],
          data: { roles: [UserRole.ADMIN] }
        },
]; 
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router'; 
import { EmailService } from '../services/email.service';

@Component({
  selector: 'app-email-list',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    FormsModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './email-list.component.html',
  styleUrl: './email-list.component.scss'
})
export class EmailListComponent implements OnInit {
  emailList: any[] = [];

  constructor(
    private emailService: EmailService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.getAll();
  }

    getAll(): void {
    this.emailService.getall().subscribe({
      next: (res: any) => {
        this.emailList = res.data;
      },
      error: (err) => {
      }
    });
  }
  create(): void {
    this.router.navigate(['/admin/email/create']);
  }

  deleteItem(id: number): void {
    if (confirm('Are you sure you want to delete this recipient?')) {
      this.emailService.delete(id).subscribe({
        next: () => {
          this.emailList = this.emailList.filter(item => item.id !== id);
        },
        error: (err) => {
        }
      });
    }
  }
}

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class PaginationComponent {
  @Input() currentPage!: number;
  @Input() totalPages!: number;
  @Input() hasPreviousPage!: boolean;
  @Input() hasNextPage!: boolean;
  @Input() pageSize!: number;
  @Input() totalCount!: number;

  @Output() pageChange = new EventEmitter<number>();

  getPageNumbers(): number[] {
    if (this.pageSize === 0) {
      return [];
    }
    const maxVisiblePages = 5;
    const pages: number[] = [];
    if (this.totalPages <= maxVisiblePages) {
      for (let i = 1; i <= this.totalPages; i++) pages.push(i);
    } else {
      let start = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
      let end = Math.min(this.totalPages, start + maxVisiblePages - 1);
      if (end - start + 1 < maxVisiblePages) start = Math.max(1, end - maxVisiblePages + 1);
      for (let i = start; i <= end; i++) pages.push(i);
    }
    return pages;
  }

  getShowingText(): string {
    const start = (this.currentPage - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage * this.pageSize, this.totalCount);
    return `Showing ${start} to ${end} of ${this.totalCount} results`;
  }
} 
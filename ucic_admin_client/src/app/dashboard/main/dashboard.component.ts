import { Component, OnInit } from '@angular/core';
import { OrdersService } from '../../orders/services/orders.service';
import { Router } from '@angular/router';
import { VendorsService } from '../../vendors/services/vendors.service';
import { JobApplicationsService } from '../../services/job-applications.service';
import { UserService } from '../../services/user.service';
import { RecruiterManagementService } from '../../recruiter-management/services/recruiter-management.service';
import { VacancyPublishStatus } from '../../recruiter-management/models/recruitment.models';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  orders: any[] = [];
  displayedOrders: any[] = [];
  currentPage: number = 1;
  pageSize: number = 5;
  totalOrders: number = 0;
  totalVendors: number = 0;
  totalJobApplications: number = 0;
  totalCustomers: number = 0;
  /** Track T4 — vacancies in PendingApproval (Recruitment API). */
  pendingVacancyApprovals = 0;
  chartData: any[] = [];
  pieChartData: any[] = [];
  dateRanges = [
    { label: 'Weekly', value: 'weekly' },
    { label: 'Monthly', value: 'monthly' },
    { label: 'Yearly', value: 'yearly' },
    { label: 'All', value: 'all' }
  ];
  selectedRange = 'monthly';

  // Status color mapping
  private statusColors: { [key: string]: string } = {
    'Pending': '#edcbbd',
    'Confirmed': '#edcbbd',
    'PaymentSubmitted': '#edcbbd',
    'PaymentConfirmed': '#edcbbd'
  };

  public colorScheme: any = {
    domain: ['#E67E22', '#2980B9', '#8E44AD', '#27AE60','#F44336','#FF9800','#4CAF50','#9C27B0']
  };

  constructor(
    private ordersService: OrdersService,
    private vendorsService: VendorsService,
    private jobApplicationsService: JobApplicationsService,
    private userService: UserService,
    private router: Router,
    private recruiterManagementService: RecruiterManagementService
  ) {}

  ngOnInit(): void {
    this.fetchOrders();
    this.fetchVendors();
    this.fetchJobApplications();
    this.fetchCustomers();
    this.fetchPendingVacancyApprovals();
  }

  fetchPendingVacancyApprovals(): void {
    this.recruiterManagementService.getVacancies(1, 1, VacancyPublishStatus.PendingApproval).subscribe({
      next: (res) => {
        this.pendingVacancyApprovals = res.metadata?.totalCount ?? 0;
      },
      error: () => {
        this.pendingVacancyApprovals = 0;
      }
    });
  }

  getStatusColor(status: string): string {
    return this.statusColors[status] || '#edcbbd';
  }

  fetchOrders(): void {
    this.ordersService.getOrders().subscribe({
      next: (res: any) => {
        this.setOrders(res.data || []);
      },
      error: (err) => {
      }
    });
  }

  fetchVendors(): void {
    this.vendorsService.getVendors(this.currentPage,1000000).subscribe({
      next: (res: any) => {
        this.totalVendors = Array.isArray(res.data) ? res.data.length : 0;
      },
      error: (err) => {
      }
    });
  }

  fetchJobApplications(): void {
    this.jobApplicationsService.getAll(this.currentPage, 1000000).subscribe({
      next: (res: any) => {
        // FIX: Check for res.data
        const jobs = Array.isArray(res?.data) ? res.data : Array.isArray(res) ? res : [];
        this.totalJobApplications = jobs.length;
      },
      error: (err) => {
      }
    });
  }

  fetchCustomers(): void {
    this.userService.getAllUsers(this.currentPage, 1000000).subscribe({
      next: (res: any) => {
        // FIX: Check for res.data
        const users = Array.isArray(res?.data) ? res.data : Array.isArray(res) ? res : [];
        this.totalCustomers = users.filter((user: any) => user.roles?.includes('User')).length;
      },
      error: (error: Error) => {
      }
    });
  }

  processSummary(): void {
    this.totalOrders = this.orders.length;
  }

  setDateRange(range: string) {
    this.selectedRange = range;
    this.processChartData();
  }

  filterOrdersByRange(): any[] {
    if (this.selectedRange === 'all') return this.orders;
    const now = new Date();
    let fromDate: Date;
    if (this.selectedRange === 'weekly') {
      fromDate = new Date(now);
      fromDate.setDate(now.getDate() - 7);
    } else if (this.selectedRange === 'monthly') {
      fromDate = new Date(now);
      fromDate.setMonth(now.getMonth() - 1);
    } else if (this.selectedRange === 'yearly') {
      fromDate = new Date(now);
      fromDate.setFullYear(now.getFullYear() - 1);
    } else {
      return this.orders;
    }
    return this.orders.filter(order => order.createdDate && new Date(order.createdDate) >= fromDate);
  }

  processChartData(): void {
    const filteredOrders = this.filterOrdersByRange();
    const statusMap: { [key: string]: number } = {};
    filteredOrders.forEach(order => {
      const status = order.status || 'Unknown';
      statusMap[status] = (statusMap[status] || 0) + 1;
    });
    this.chartData = Object.keys(statusMap).map(status => ({
      name: status,
      value: statusMap[status]
    }));
    // Pie chart uses the same data as bar chart
    this.pieChartData = [...this.chartData];
  }

  formatYAxisTick(value: number): string {
    return Number.isInteger(value) ? value.toString() : '';
  }

  showOrderDetails(order: any): void {
    this.router.navigate(['/admin/orders/details', order.orderId]);
  }

  setOrders(orders: any[]): void {
    this.orders = orders;
    this.totalOrders = orders.length;
    this.currentPage = 1;
    this.updateDisplayedOrders();
    this.processChartData();
  }

  updateDisplayedOrders(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedOrders = this.orders.slice(startIndex, endIndex);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages.length) return;
    this.currentPage = page;
    this.updateDisplayedOrders();
  }

  get totalPages(): number[] {
    return Array(Math.ceil(this.totalOrders / this.pageSize))
      .fill(0)
      .map((x, i) => i + 1);
  }

  min(a: number, b: number): number {
    return Math.min(a, b);
  }
}

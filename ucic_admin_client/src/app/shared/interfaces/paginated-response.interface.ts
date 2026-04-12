export interface PaginatedResponse<T> {
  data: T[];
  message?: string;
  metadata: {
    currentPage: number;
    hasNext: boolean;
    hasPrevious: boolean;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
  success?: boolean;
} 
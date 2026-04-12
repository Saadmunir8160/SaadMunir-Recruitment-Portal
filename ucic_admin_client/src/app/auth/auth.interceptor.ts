import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
  HttpHeaders
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Skip adding auth headers for login request
    if (request.url.includes('/Auth/Login')) {
      return next.handle(request);
    }

    const token = this.authService.getToken();
    
    if (token) {

      let headersConfig: { [name: string]: string } = {
        'Authorization': `Bearer ${token.trim()}`
      };
      // Only set Content-Type and Accept if not FormData
      if (!(request.body instanceof FormData)) {
        headersConfig['Content-Type'] = 'application/json';
        headersConfig['Accept'] = 'application/json';
      }
      const headers = new HttpHeaders(headersConfig);
      const authReq = request.clone({
        headers,
        withCredentials: false
      });
      
      return next.handle(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401 || error.status === 0) {
            this.authService.clearAuthData();
            this.router.navigate(['/login']);
            console.log('HTTP Interceptor caught 401/0 error, NOT redirecting to login for testing');
            // Temporarily disabled: this.authService.clearAuthData();
            // Temporarily disabled: this.router.navigate(['/login']);
          }
          return throwError(() => error);
        })
      );
    }
    
    return next.handle(request);
  }
} 

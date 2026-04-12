import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Skip auth header for login/register requests
    if (req.url.includes('/Auth/Login') || req.url.includes('/Auth/RegisterCandidate')) {
      return next.handle(req);
    }

    let authReq = req;

    if (isPlatformBrowser(this.platformId)) {
      const token = sessionStorage.getItem('token');
      if (token) {
        // Don't set Content-Type for FormData (file uploads)
        if (req.body instanceof FormData) {
          authReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${token}`,
              Accept: 'application/json'
            }
          });
        } else {
          authReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${token}`,
              'Content-Type': 'application/json',
              Accept: 'application/json'
            }
          });
        }
      }
    }

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 || error.status === 0) {
          if (isPlatformBrowser(this.platformId)) {
            sessionStorage.removeItem('token');
            sessionStorage.removeItem('FullName');
            sessionStorage.removeItem('user');
            sessionStorage.removeItem('userRole');
          }
          this.router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
}

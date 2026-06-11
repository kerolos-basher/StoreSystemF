import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { extractHttpErrorMessage } from '../utils/http-error.util';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError(error => {
      const message = extractHttpErrorMessage(error);
      if (message) {
        snackBar.open(message, 'إغلاق', {
          duration: 4500,
          horizontalPosition: 'center',
          verticalPosition: 'top',
          panelClass: ['app-error-snackbar']
        });
      }

      return throwError(() => error);
    })
  );
};

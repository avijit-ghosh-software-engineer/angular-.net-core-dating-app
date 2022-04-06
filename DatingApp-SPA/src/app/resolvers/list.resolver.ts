import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { IUser } from '../models/IUser';
import { AlertifyService } from '../services/alertify.service';
import { UserService } from '../services/user.service';

@Injectable()
export class ListsResolver implements Resolve<IUser[]> {
  pageNumber = 1;
  pageSize = 12;
  likesParams = 'Likers';
  constructor(
    private userService: UserService,
    private router: Router,
    private alertify: AlertifyService
  ) {}

  resolve(route: ActivatedRouteSnapshot): Observable<IUser[]> {
    return this.userService
      .getUsers(this.pageNumber, this.pageSize, null, this.likesParams)
      .pipe(
        catchError((error) => {
          this.alertify.error('Some exception taken place.');
          this.router.navigate(['/home']);
          return of(null);
        })
      );
  }
}

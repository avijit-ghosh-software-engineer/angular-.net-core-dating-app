import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { IUser } from '../models/IUser';
import { AlertifyService } from '../services/alertify.service';
import { UserService } from '../services/user.service';

@Injectable()
export class MemberDetailResolver implements Resolve<IUser> {
  constructor(
    private userService: UserService,
    private router: Router,
    private alertify: AlertifyService
  ) {}

  resolve(route: ActivatedRouteSnapshot): Observable<IUser> {
    return this.userService.getUser(route.params['id']).pipe(
      catchError((error) => {
        this.alertify.error('Some exception taken place.');
        this.router.navigate(['/members']);
        return of(null);
      })
    );
  }
}

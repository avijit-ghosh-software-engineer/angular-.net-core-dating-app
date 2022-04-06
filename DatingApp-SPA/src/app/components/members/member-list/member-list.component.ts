import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { IPagination, PaginatedResult } from 'src/app/models/IPagination';
import { IUser } from 'src/app/models/IUser';
import { AlertifyService } from 'src/app/services/alertify.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css'],
})
export class MemberListComponent implements OnInit {
  users: IUser[];
  pagination: IPagination;
  user: IUser = JSON.parse(localStorage.getItem('user'));
  genderList = [
    { value: 'male', text: 'Males' },
    { value: 'female', text: 'Females' },
  ];
  userParams: any = {};
  constructor(
    private route: ActivatedRoute,
    private alertify: AlertifyService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.users = data.users.result;
      this.pagination = data.users.pagination;
    });
    this.filterDefaultValue();
  }

  resetFilters() {
    this.filterDefaultValue();
    this.loadUsers();
  }

  filterDefaultValue() {
    this.userParams.gender = this.user.gender === 'male' ? 'female' : 'male';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }

  loadUsers() {
    this.userService
      .getUsers(
        this.pagination.currentPage,
        this.pagination.itemsPerPage,
        this.userParams
      )
      .subscribe(
        (result: PaginatedResult<IUser[]>) => {
          this.users = result.result;
          this.pagination = result.pagination;
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }
}

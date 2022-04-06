import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { IPagination, PaginatedResult } from 'src/app/models/IPagination';
import { IUser } from 'src/app/models/IUser';
import { AlertifyService } from 'src/app/services/alertify.service';
import { AuthService } from 'src/app/services/auth.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css'],
})
export class ListsComponent implements OnInit {
  users: IUser[];
  pagination: IPagination;
  likesParam: string;
  constructor(
    private authService: AuthService,
    private userService: UserService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.users = data.users.result;
      this.pagination = data.users.pagination;
    });
    this.likesParam = 'Likers';
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
        null,
        this.likesParam
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

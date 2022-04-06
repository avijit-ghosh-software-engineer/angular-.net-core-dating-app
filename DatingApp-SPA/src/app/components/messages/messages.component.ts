import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { IMessage } from 'src/app/models/IMessage';
import { IPagination, PaginatedResult } from 'src/app/models/IPagination';
import { AlertifyService } from 'src/app/services/alertify.service';
import { AuthService } from 'src/app/services/auth.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
})
export class MessagesComponent implements OnInit {
  messages: IMessage[];
  pagination: IPagination;
  messageContainer = 'Unread';
  constructor(
    private userService: UserService,
    private authService: AuthService,
    private alertifyService: AlertifyService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.messages = data.messages.result;
      this.pagination = data.messages.pagination;
    });
  }

  loadMessages() {
    this.userService
      .getMessages(
        this.authService.decodedToken.nameid,
        this.pagination.currentPage,
        this.pagination.itemsPerPage,
        this.messageContainer
      )
      .subscribe(
        (res: PaginatedResult<IMessage[]>) => {
          this.messages = res.result;
          this.pagination = res.pagination;
        },
        (error) => {
          this.alertifyService.error(error);
        }
      );
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadMessages();
  }

  deleteMessage(id: number) {
    this.alertifyService.confirm(
      'Are you sure you want to delete this message?',
      () => {
        this.userService
          .deleteMessage(id, this.authService.decodedToken.nameid)
          .subscribe(
            () => {
              this.messages.splice(
                this.messages.findIndex((x) => x.id === id),
                1
              );
              this.alertifyService.success('Message has been deleted.');
              this.loadMessages();
            },
            (error) => {
              this.alertifyService.error('Failed to delete the message.');
            }
          );
      }
    );
  }
}

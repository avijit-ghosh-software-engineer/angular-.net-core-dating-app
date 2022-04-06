import { Component, Input, OnInit } from '@angular/core';
import { IMessage } from 'src/app/models/IMessage';
import { AlertifyService } from 'src/app/services/alertify.service';
import { AuthService } from 'src/app/services/auth.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit {
  @Input() recipientId: number;
  messages: IMessage[];
  newMessage: any = {};
  constructor(
    private userService: UserService,
    private authService: AuthService,
    private alertify: AlertifyService
  ) {}

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.userService
      .getMessageThread(this.authService.decodedToken.nameid, this.recipientId)
      .subscribe(
        (messages) => {
          this.messages = messages;
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }

  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService
      .sendMessage(this.authService.decodedToken.nameid, this.newMessage)
      .subscribe(
        (message: IMessage) => {
          this.messages.unshift(message);
          this.newMessage.content = '';
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }

  markAsRead(id: number) {
    const currentUserId = +this.authService.decodedToken.nameid;
    this.userService.markAsRead(currentUserId, id).subscribe(
      () => {
        this.loadMessages();
      },
      (error) => {
        this.alertify.error(error);
      }
    );
  }
}

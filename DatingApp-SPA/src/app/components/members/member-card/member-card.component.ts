import { Component, Input, OnInit } from '@angular/core';
import { IUser } from 'src/app/models/IUser';
import { AlertifyService } from 'src/app/services/alertify.service';
import { AuthService } from 'src/app/services/auth.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
})
export class MemberCardComponent implements OnInit {
  @Input() user: IUser;
  constructor(
    private authService: AuthService,
    private userService: UserService,
    private alertify: AlertifyService
  ) {}

  ngOnInit(): void {}

  sendLike(id: number) {
    this.userService
      .sendLike(this.authService.decodedToken.nameid, id)
      .subscribe(
        (date) => {
          this.alertify.success('You have liked : ' + this.user.knownAs);
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }
}

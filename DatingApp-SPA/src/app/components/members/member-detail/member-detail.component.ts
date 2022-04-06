import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TabsetComponent } from 'ngx-bootstrap/tabs';
import {
  NgxGalleryAnimation,
  NgxGalleryImage,
  NgxGalleryOptions,
} from 'ngx-gallery-9';
import { IUser } from 'src/app/models/IUser';
import { AlertifyService } from 'src/app/services/alertify.service';
import { AuthService } from 'src/app/services/auth.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs') memberTabs: TabsetComponent;
  user: IUser;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  defaultText =
    "There are many variations of passages of Lorem Ipsum available, but the majority have suffered alteration in some form, by injected humour, or randomised words which don't look even slightly believable. If you are going to use a passage of Lorem Ipsum, you need to be sure there isn't anything embarrassing hidden in the middle of text. All the Lorem Ipsum generators on the Internet tend to repeat predefined chunks as necessary, making this the first true generator on the Internet. ";

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private userService: UserService,
    private alertify: AlertifyService
  ) {}

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.user = data.user;
    });

    this.route.queryParams.subscribe((params) => {
      const selectedTab = params.tab;
      if (typeof this.memberTabs !== 'undefined') {
        this.memberTabs.tabs[selectedTab > 0 ? selectedTab : 0].active = true;
      } else {
        this.onLoadSelectTab(selectedTab);
      }
    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false,
      },
    ];

    this.galleryImages = this.getImages();
  }

  getImages() {
    const imageUrls = [];
    // tslint:disable-next-line: prefer-for-of
    for (let i = 0; i < this.user.photos.length; i++) {
      imageUrls.push({
        small: this.user.photos[i].url,
        medium: this.user.photos[i].url,
        big: this.user.photos[i].url,
        description: this.user.photos[i].description,
      });
    }
    return imageUrls;
  }

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

  onLoadSelectTab(selectedTab: number) {
    if (typeof this.memberTabs === 'undefined') {
      setTimeout(() => {
        this.onLoadSelectTab(selectedTab);
      }, 100);
    } else {
      this.memberTabs.tabs[selectedTab > 0 ? selectedTab : 0].active = true;
    }
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }
  // loadUser() {
  //   const id = +this.route.snapshot.params['id'];
  //   this.userService.getUser(id).subscribe(
  //     (user: IUser) => {
  //       this.user = user;
  //     },
  //     (error) => {
  //       this.alertify.error(error);
  //     }
  //   );
  // }
}

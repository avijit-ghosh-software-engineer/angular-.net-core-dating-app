import { Input } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { IPhoto } from 'src/app/models/IPhoto';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { AuthService } from 'src/app/services/auth.service';
import { UserService } from 'src/app/services/user.service';
import { AlertifyService } from 'src/app/services/alertify.service';

@Component({
  selector: 'app-member-photo-editor',
  templateUrl: './member-photo-editor.component.html',
  styleUrls: ['./member-photo-editor.component.css'],
})
export class MemberPhotoEditorComponent implements OnInit {
  @Input() photos: IPhoto[];

  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  currentMainPhoto: IPhoto;
  constructor(
    private authService: AuthService,
    private userService: UserService,
    private alertify: AlertifyService
  ) {}

  ngOnInit(): void {
    this.initializedUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializedUploader() {
    this.uploader = new FileUploader({
      url:
        this.baseUrl +
        'users/' +
        this.authService.decodedToken.nameid +
        '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, header) => {
      if (response) {
        const res: IPhoto = JSON.parse(response);
        const photo = {
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain,
        };
        this.photos.push(photo);
        if (photo.isMain) {
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem(
            'user',
            JSON.stringify(this.authService.currentUser)
          );
        }
      }
    };
  }

  setmainPhoto(photo: IPhoto) {
    this.userService
      .setMainPhoto(this.authService.decodedToken.nameid, photo.id)
      .subscribe(
        (next) => {
          this.currentMainPhoto = this.photos.filter(
            (x) => x.isMain === true
          )[0];
          this.currentMainPhoto.isMain = false;
          photo.isMain = true;
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem(
            'user',
            JSON.stringify(this.authService.currentUser)
          );
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }

  deletePhoto(id: number) {
    this.alertify.confirm('Are you sure you want to delete this photo?', () => {
      this.userService
        .deletePhoto(this.authService.decodedToken.nameid, id)
        .subscribe(
          () => {
            this.photos.splice(
              this.photos.findIndex((x) => x.id === id),
              1
            );
            this.alertify.success('Photo has been deleted.');
          },
          (error) => {
            this.alertify.error(error);
          }
        );
    });
  }
}

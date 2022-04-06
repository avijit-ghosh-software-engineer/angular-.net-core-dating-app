import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberPhotoEditorComponent } from './member-photo-editor.component';

describe('MemberPhotoEditorComponent', () => {
  let component: MemberPhotoEditorComponent;
  let fixture: ComponentFixture<MemberPhotoEditorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MemberPhotoEditorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MemberPhotoEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

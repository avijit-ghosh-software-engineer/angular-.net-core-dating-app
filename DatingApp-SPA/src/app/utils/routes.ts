import { Routes } from '@angular/router';
import { HomeComponent } from '../components/home/home.component';
import { ListsComponent } from '../components/lists/lists.component';
import { MemberDetailComponent } from '../components/members/member-detail/member-detail.component';
import { MemberEditComponent } from '../components/members/member-edit/member-edit.component';
import { MemberListComponent } from '../components/members/member-list/member-list.component';
import { MessagesComponent } from '../components/messages/messages.component';
import { AuthGuard } from '../guard/auth.guard';
import { PreventUnsavedChange } from '../guard/prevent-unsaved-changes.guard';
import { ListsResolver } from '../resolvers/list.resolver';
import { MemberDetailResolver } from '../resolvers/member-detail.resolver';
import { MemberEditResolver } from '../resolvers/member-edit.resolver';
import { MemberListResolver } from '../resolvers/member-list.resolver';
import { MessagesResolver } from '../resolvers/messages.resolver';

export const appRoots: Routes = [
  { path: '', component: HomeComponent },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      {
        path: 'members',
        component: MemberListComponent,
        resolve: { users: MemberListResolver },
      },
      {
        path: 'members/:id',
        component: MemberDetailComponent,
        resolve: { user: MemberDetailResolver },
      },
      {
        path: 'member/edit',
        component: MemberEditComponent,
        resolve: { user: MemberEditResolver },
        canDeactivate: [PreventUnsavedChange],
      },
      {
        path: 'messages',
        component: MessagesComponent,
        resolve: { messages: MessagesResolver },
      },
      {
        path: 'lists',
        component: ListsComponent,
        resolve: { users: ListsResolver },
      },
    ],
  },
  { path: '**', redirectTo: '', pathMatch: 'full' },
];

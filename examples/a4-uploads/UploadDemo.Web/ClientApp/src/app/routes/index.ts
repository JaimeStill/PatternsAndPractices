import { Route } from '@angular/router';
import { FoldersComponent } from './folder/folders.component';
import { FolderComponent } from './folder/folder.component';
import { UploadsComponent } from './upload/uploads.component';
import { UploadComponent } from './upload/upload.component';

export const RouteComponents = [
  FoldersComponent,
  FolderComponent,
  UploadsComponent,
  UploadComponent
];

export const Routes: Route[] = [
  { path: 'folders', component: FoldersComponent },
  { path: 'folder/:name', component: FolderComponent },
  { path: 'uploads', component: UploadsComponent },
  { path: 'upload/:file', component: UploadComponent },
  { path: '', redirectTo: 'uploads', pathMatch: 'full' },
  { path: '**', redirectTo: 'uploads', pathMatch: 'full' }
];

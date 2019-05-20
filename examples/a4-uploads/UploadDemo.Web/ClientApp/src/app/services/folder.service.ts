import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { SnackerService } from './snacker.service';

import {
  Folder,
  FolderUpload,
  Upload
} from '../models';

@Injectable()
export class FolderService {
  private folders = new Subject<Folder[]>();
  private folder = new Subject<Folder>();
  private uploads = new Subject<Upload[]>();

  folders$ = this.folders.asObservable();
  folder$ = this.folder.asObservable();
  uploads$ = this.uploads.asObservable();

  constructor(
    private http: HttpClient,
    private snacker: SnackerService
  ) { }

  getFolders = () => this.http.get<Folder[]>('/api/folder/getFolders')
    .subscribe(
      data => this.folders.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  getDeletedFolders = () => this.http.get<Folder[]>('/api/folder/getDeletedFolders')
    .subscribe(
      data => this.folders.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  searchFolders = (search: string) => this.http.get<Folder[]>(`/api/folder/searchFolders/${search}`)
    .subscribe(
      data => this.folders.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  searchDeletedFolders = (search: string) => this.http.get<Folder[]>(`/api/folder/searchDeletedFolders/${search}`)
    .subscribe(
      data => this.folders.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  getFolder = (id: number): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.get<Folder>(`/api/folder/getFolder/${id}`)
        .subscribe(
          data => {
            this.folder.next(data);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

  getFolderByName = (name: string): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.get<Folder>(`/api/folder/getFolderByName/${name}`)
        .subscribe(
          data => {
            this.folder.next(data);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

  getFolderUploads = (name: string) => this.http.get<Upload[]>(`/api/folder/getFolderUploads/${name}`)
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  getExcludedFolders = (file: string) => this.http.get<Folder[]>(`/api/folder/getExcludedFolders/${file}`)
    .subscribe(
      data => this.folders.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  addFolder = (folder: Folder): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/folder/addFolder', folder)
        .subscribe(
          () => {
            this.snacker.sendSuccessMessage(`${folder.name} successfully added`);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

  updateFolder = (folder: Folder): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/folder/updateFolder', folder)
        .subscribe(
          () => {
            this.snacker.sendSuccessMessage(`${folder.name} successfully updated`);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

  toggleFolderDeleted = (folder: Folder): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/folder/toggleFolderDeleted', folder)
        .subscribe(
          () => {
            const message = folder.isDeleted ?
              `${folder.name} successfully restored` :
              `${folder.name} successfully deleted`;

            this.snacker.sendSuccessMessage(message);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

  removeFolder = (folder: Folder): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/folder/removeFolder', folder)
        .subscribe(
          () => {
            this.snacker.sendSuccessMessage(`${folder.name} permanently deleted`);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

    addFolderUploads = (folderUploads: FolderUpload[]): Promise<boolean> =>
      new Promise((resolve) => {
        this.http.post('/api/folder/addFolderUploads', folderUploads)
          .subscribe(
            () => {
              this.snacker.sendSuccessMessage('Uploads successfully added to folder');
              resolve(true);
            },
            err => {
              this.snacker.sendErrorMessage(err.error);
              resolve(false);
            }
          )
      });

    updateFolderUpload = (folderUpload: FolderUpload): Promise<boolean> =>
      new Promise((resolve) => {
        this.http.post('/api/folder/updateFolderUpload', folderUpload)
          .subscribe(
            () => {
              this.snacker.sendSuccessMessage('Folder upload successfully updated');
              resolve(true);
            },
            err => {
              this.snacker.sendErrorMessage(err.error);
              resolve(false);
            }
          )
      });

    removeFolderUpload = (name: string, upload: Upload): Promise<boolean> =>
      new Promise((resolve) => {
        this.http.post(`/api/folder/removeFolderUpload/${name}`, upload)
          .subscribe(
            () => {
              this.snacker.sendSuccessMessage('Upload successfully removed from folder');
              resolve(true);
            },
            err => {
              this.snacker.sendErrorMessage(err.error);
              resolve(false);
            }
          )
      });
}

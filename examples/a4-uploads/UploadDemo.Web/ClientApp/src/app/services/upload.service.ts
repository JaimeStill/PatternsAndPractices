import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { CoreService } from './core.service';
import { SnackerService } from './snacker.service';

import {
  Folder,
  FolderUpload,
  Upload
} from '../models';

@Injectable()
export class UploadService {
  private uploads = new Subject<Upload[]>();
  private upload = new Subject<Upload>();
  private folders = new Subject<Folder[]>();

  uploads$ = this.uploads.asObservable();
  upload$ = this.upload.asObservable();
  folders$ = this.folders.asObservable();

  constructor(
    private core: CoreService,
    private http: HttpClient,
    private snacker: SnackerService
  ) { }

  getUploads = () => this.http.get<Upload[]>('/api/upload/getUploads')
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  getDeletedUploads = () => this.http.get<Upload[]>('/api/upload/getDeletedUploads')
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  searchUploads = (search: string) => this.http.get<Upload[]>(`/api/upload/searchUploads/${search}`)
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  searchDeletedUploads = (search: string) => this.http.get<Upload[]>(`/api/upload/getDeletedUploads/${search}`)
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  getUpload = (id: number): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.get<Upload>(`/api/upload/getUpload/${id}`)
      .subscribe(
        data => {
          this.upload.next(data);
          resolve(true);
        },
        err => {
          this.snacker.sendErrorMessage(err.error);
          resolve(false);
        }
      )
    });

  getUploadByName = (file: string): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.get<Upload>(`/api/upload/getUploadByName/${file}`)
        .subscribe(
        data => {
          this.upload.next(data);
          resolve(true);
        },
        err => {
          this.snacker.sendErrorMessage(err.error);
          resolve(false);
        }
      )
    });

  getExcludedUploads = (name: string) => this.http.get<Upload[]>(`/api/upload/getExcludedUploads/${name}`)
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err)
    );

  getUploadFolders = (id: number) => this.http.get<Folder[]>(`/api/upload/getUploadFolders/${id}`)
    .subscribe(
      data => this.folders.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  uploadFiles = (formData: FormData): Promise<Upload[]> =>
    new Promise((resolve) => {
      this.http.post<Upload[]>('/api/upload/uploadFiles', formData, { headers: this.core.getUploadOptions() })
        .subscribe(
          res => {
            this.snacker.sendSuccessMessage('Uploads successfully processed');
            resolve(res);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(null);
          }
        )
    });

  toggleUploadDeleted = (upload: Upload): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/upload/toggleUploadDeleted', upload)
        .subscribe(
          () => {
            const message = upload.isDeleted ?
              `${upload.file} successfully restored` :
              `${upload.file} successfully deleted`;

            this.snacker.sendSuccessMessage(message);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

  removeUpload = (upload: Upload): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/upload/removeUpload', upload)
        .subscribe(
          () => {
            this.snacker.sendSuccessMessage(`${upload.file} permanently deleted`);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

  addFolderUpload = (folderUpload: FolderUpload): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/folder/addFolderUpload', folderUpload)
        .subscribe(
          () => {
            this.snacker.sendSuccessMessage('Upload successfully added to folder');
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

  removeFolderUpload = (folderUpload: FolderUpload): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/folder/removeFolderUpload', folderUpload)
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

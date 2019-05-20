import { ConfirmDialog } from './confirm.dialog';

import { AddUploadDialog } from './folder/add-upload.dialog';
import { FolderDialog } from './folder/folder.dialog';
import { FolderBinDialog } from './folder/folder-bin.dialog';

import { AddFolderDialog } from './upload/add-folder.dialog';
import { UploadBinDialog } from './upload/upload-bin.dialog';

export const Dialogs = [
  ConfirmDialog,
  AddUploadDialog,
  FolderDialog,
  FolderBinDialog,
  AddFolderDialog,
  UploadBinDialog
];

export * from './confirm.dialog';
export * from './folder/add-upload.dialog';
export * from './folder/folder.dialog';
export * from './folder/folder-bin.dialog';
export * from './upload/add-folder.dialog';
export * from './upload/upload-bin.dialog';

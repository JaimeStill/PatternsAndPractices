import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HubConnectionBuilder } from '@aspnet/signalr';
import { SnackerService } from '../snacker.service';

@Injectable()
export class GroupSocketService {
  private connection = new HubConnectionBuilder()
    .withUrl('/group-socket')
    .build();

  private connected = new BehaviorSubject<boolean>(null);
  private error = new BehaviorSubject<any>(null);
  private trigger = new BehaviorSubject<boolean>(false);

  connected$ = this.connected.asObservable();
  error$ = this.error.asObservable();
  trigger$ = this.trigger.asObservable();

  constructor(
    private snacker: SnackerService
  ) {
    this.connection.on(
      'groupAlert',
      (message: string) => this.snacker.sendColorMessage(message, ['snacker-teal'])
    );

    this.connection.on('groupMessage', () => this.trigger.next(true));

    this.connection
      .start()
      .then(() => this.connected.next(true))
      .catch((err) => {
        this.connected.next(false);
        this.error.next(err);
        this.snacker.sendErrorMessage(err.error);
      });
  }

  triggerJoinGroup = async (group: string) => {
    if (this.connected.value) {
      await this.connection
        .invoke('triggerJoinGroup', group);
    }
  }

  triggerLeaveGroup = async (group: string) => {
    if (this.connected.value) {
      await this.connection
        .invoke('triggerLeaveGroup', group);
    }
  }

  triggerGroupMessage = async (group: string) => {
    if (this.connected.value) {
      await this.connection
        .invoke('triggerGroupMessage', group);
    }
  }
}

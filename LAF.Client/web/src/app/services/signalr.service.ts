import { Injectable, NgZone } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection;
  private _isConnected = new BehaviorSubject<boolean>(false);
  isConnected$ = this._isConnected.asObservable();

  constructor(
    private ngZone: NgZone,
    private toastService: ToastService
  ) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/laf`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Debug)
      .build();

    this.registerCommonHandlers();
  }

  private registerCommonHandlers(): void {
    this.hubConnection.onreconnecting((error) => {
      console.log('Reconnecting to SignalR hub...', error);
      this._isConnected.next(false);
      this.ngZone.run(() => {
        this.toastService.showWarning('Reconnecting to server...');
      });
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('Reconnected to SignalR hub:', connectionId);
      this._isConnected.next(true);
      this.ngZone.run(() => {
        this.toastService.showSuccess('Reconnected to server');
      });
    });

    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this._isConnected.next(false);
      this.ngZone.run(() => {
        this.toastService.showError('Connection lost');
      });
    });
  }

  async connect(): Promise<void> {
    try {
      if (this.hubConnection.state === 'Disconnected') {
        await this.hubConnection.start();
        console.log('Connected to SignalR hub');
        this._isConnected.next(true);
        this.ngZone.run(() => {
          this.toastService.showSuccess('Connected to real-time updates');
        });
      }
    } catch (error) {
      console.error('Error connecting to SignalR hub:', error);
      this._isConnected.next(false);
      this.ngZone.run(() => {
        this.toastService.showError('Failed to connect to server');
      });
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    try {
      await this.hubConnection.stop();
      console.log('Disconnected from SignalR hub');
      this._isConnected.next(false);
    } catch (error) {
      console.error('Error disconnecting from SignalR hub:', error);
      throw error;
    }
  }

  // Method to subscribe to a specific message type
  on<T>(methodName: string, callback: (data: T) => void): void {
    this.hubConnection.on(methodName, (data: T) => {
      this.ngZone.run(() => callback(data));
    });
  }

  // Method to unsubscribe from a specific message type
  off(methodName: string): void {
    this.hubConnection.off(methodName);
  }

  // Method to invoke a hub method and get response
  async invoke<T>(methodName: string, ...args: any[]): Promise<T> {
    try {
      return await this.hubConnection.invoke<T>(methodName, ...args);
    } catch (error) {
      console.error(`Error invoking ${methodName}:`, error);
      throw error;
    }
  }
}

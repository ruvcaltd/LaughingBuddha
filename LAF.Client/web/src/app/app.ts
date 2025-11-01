import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SignalRService } from './services/signalr.service';
import { ThemeService } from './services/theme.service';
import { Navbar } from './shared/navbar/navbar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('web');
  private themeService = inject(ThemeService);
  private signalRService = inject(SignalRService);

  async ngOnInit(): Promise<void> {
    // Initialize theme service
    this.themeService;

    // Connect to SignalR hub
    try {
      await this.signalRService.connect();
    } catch (error) {
      console.error('Failed to connect to SignalR hub:', error);
    }
  }

  async ngOnDestroy(): Promise<void> {
    await this.signalRService.disconnect();
  }
}

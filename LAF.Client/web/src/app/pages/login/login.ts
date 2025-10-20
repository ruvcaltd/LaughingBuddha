import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { ThemeToggleComponent } from '../../shared/theme-toggle/theme-toggle';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule, ThemeToggleComponent],
  templateUrl: './login.html'
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  loading: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  async onSubmit() {
    this.errorMessage = '';
    
    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter both email and password';
      return;
    }

    this.loading = true;

    try {
      const result = await this.authService.login(this.email, this.password);
      if (result.success) {
        this.router.navigate(['/repo-rates']);
      } else {
        this.errorMessage = result.error || 'Login failed';
      }
    } catch (error) {
      this.errorMessage = 'An error occurred during login';
      console.error('Login error:', error);
    } finally {
      this.loading = false;
    }
  }
}
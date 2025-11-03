import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthConfigService } from '../../services/auth-config.service';
import { AuthService } from '../../services/auth.service';
import { EntraAuthService } from '../../services/entra-auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  loading: boolean = false;
  showEntraLogin: boolean = true;
  isEntraAuthEnabled: boolean = false;
  isJwtFallbackEnabled: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private entraAuthService: EntraAuthService,
    private authConfigService: AuthConfigService,
  ) {
    this.isEntraAuthEnabled = this.authConfigService.isEntraAuthEnabled();
    this.isJwtFallbackEnabled = this.authConfigService.isJwtFallbackEnabled();
    this.showEntraLogin = this.isEntraAuthEnabled;

    console.log('Login Component Debug:', {
      isEntraAuthEnabled: this.isEntraAuthEnabled,
      isJwtFallbackEnabled: this.isJwtFallbackEnabled,
      showEntraLogin: this.showEntraLogin,
    });
  }

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

  async loginWithMicrosoft() {
    this.errorMessage = '';
    this.loading = true;

    try {
      console.log('Starting Microsoft login redirect process...');
      // Use redirect instead of popup for Microsoft login

      // For redirect flow, we don't use Observable pattern as the page will redirect
      await this.authService.loginWithEntraRedirect();

      console.log('Microsoft login redirect should have been initiated');
      // This line should not be reached as the browser should redirect
    } catch (error) {
      this.errorMessage = 'Failed to initiate Microsoft login redirect';
      console.error('Microsoft login redirect error:', error);
      this.loading = false;
    }
  }

  toggleAuthMethod() {
    this.showEntraLogin = !this.showEntraLogin;
    this.errorMessage = '';
  }
}

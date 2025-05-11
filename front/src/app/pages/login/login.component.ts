import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  username: string = '';
  email: string = '';
  password: string = '';
  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    const user = {
      username: this.username,
      email: this.email,
      password: this.password
    };

    this.authService.login(user).subscribe({
      next: (res: any) => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = err.error?.message || 'Erreur de connexion';
      }
    });
  }
}

import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  imports: [FormsModule] // ← virgule ici corrigée
})
export class LoginComponent {
  username: string = '';
  email: string = '';
  password: string = '';
  errorMessage: string = '';

  constructor(private authService: AuthService) {}

  onSubmit() {
    const user = {
      username: this.username,
      email: this.email,
      password: this.password
    };

    this.authService.login(user).subscribe({
      next: (res) => {
        console.log('Connecté :', res);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = err.error?.message || 'Erreur de connexion';
      }
    });
  }
}

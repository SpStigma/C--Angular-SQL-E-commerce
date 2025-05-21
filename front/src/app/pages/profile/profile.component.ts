import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { ProfileService } from '../../services/profile.service';
import { Router } from '@angular/router';
import { CommonModule }        from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-profile',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profileForm: FormGroup;
  message = '';
  error = '';
  // **nouvelles** variables pour stocker les placeholders
  placeholderUsername = '';
  placeholderEmail    = '';

  constructor(
    private fb: FormBuilder,
    private profileService: ProfileService,
    private router: Router
  ) {
    this.profileForm = this.fb.group({
      username: [''],
      email: ['', [Validators.email]],
      currentPassword: [''],
      newPassword: [''],
      confirmPassword: ['']
    });
  }

  ngOnInit(): void {
    this.profileService.getProfile().subscribe(profile => {
      // on stocke dans placeholder*, on NE patch plus la valeur
      this.placeholderUsername = profile.username;
      this.placeholderEmail    = profile.email;
    }, err => {
      this.error = 'Impossible de récupérer le profil';
    });
  }

    onSave(): void {
    this.message = '';
    this.error   = '';

    // vérif mot de passe
    if (
      this.profileForm.value.newPassword &&
      this.profileForm.value.newPassword !== this.profileForm.value.confirmPassword
    ) {
      this.error = 'Les nouveaux mots de passe ne correspondent pas';
      return;
    }

    // on ne crée que les champs “dirty” (modifiés)
    const dto: any = {};

    const uCtrl = this.profileForm.get('username')!;
    if (uCtrl.dirty && uCtrl.value) {
      dto.username = uCtrl.value;
    }

    const eCtrl = this.profileForm.get('email')!;
    if (eCtrl.dirty && eCtrl.value) {
      dto.email = eCtrl.value;
    }

    // password partiel
    if (this.profileForm.value.currentPassword && this.profileForm.value.newPassword) {
      dto.currentPassword = this.profileForm.value.currentPassword;
      dto.newPassword     = this.profileForm.value.newPassword;
    }

    // si rien n’a changé
    if (Object.keys(dto).length === 0) {
      this.error = 'Aucun changement détecté';
      return;
    }

    // appel partiel
    this.profileService.updateProfile(dto).subscribe(
      () => {
        this.message = 'Profil mis à jour avec succès';
        // on reset juste les champs password
        this.profileForm.patchValue({
          currentPassword: '',
          newPassword: '',
          confirmPassword: ''
        });
        // et on marque tout “pristine”
        this.profileForm.markAsPristine();
      },
      (err: any) => {
        this.error = err.error.message || 'Erreur lors de la mise à jour du profil';
      }
    );
  }

   onDelete(): void {
    if (!confirm('Êtes-vous sûr de vouloir supprimer votre compte ? Cette action est irréversible.')) {
      return;
    }

    this.profileService.deleteAccount().subscribe(
      () => {
        // une fois le compte supprimé, redirige vers login
        this.router.navigate(['/login']);
      },
      (err: any) => {
        this.error = err.error?.message || 'Erreur lors de la suppression du compte';
      }
    );
  }
}

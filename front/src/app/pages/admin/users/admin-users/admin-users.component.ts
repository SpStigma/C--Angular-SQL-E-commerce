import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../../services/user.service';
import { User } from '../../../../models/user.model';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.css']
})
export class AdminUsersComponent implements OnInit {
  users: User[] = [];
  filteredUsers: User[] = [];
  searchTerm: string = '';
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.fetchUsers();
  }

  private fetchUsers(): void {
    this.loading = true;
    this.userService.getUsers().subscribe({
      next: (data: User[]) => {
        this.users = data;
        this.applyFilter();
        this.loading = false;
      },
      error: (err: any) => {
        this.errorMessage = err.message || 'Erreur lors du chargement';
        this.loading = false;
      }
    });
  }

  onSearchChange(): void {
    this.applyFilter();
  }

  private applyFilter(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredUsers = this.users.filter(u =>
      u.username.toLowerCase().includes(term)
    );
  }

  onRoleChange(user: User): void {
    if (user.id == null) {
      console.error('User ID is missing');
      return;
    }
    const previousRole = user.role;
    this.errorMessage = null;
    this.successMessage = null;
    this.userService.updateRole(user.id, user.role).subscribe({
      next: () => {
        this.successMessage = `Rôle de ${user.username} mis à jour en ${user.role}`;
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err: any) => {
        user.role = previousRole;
        this.errorMessage = err.error?.message || 'Erreur lors de la mise à jour du rôle';
      }
    });
  }
}
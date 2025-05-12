import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface Product {
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl: string;
}

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './add-product.component.html',
})
export class AddProductComponent {
  product: Product = {
    name: '',
    description: '',
    price: 0,
    stock: 0,
    imageUrl: ''
  };

  imageFile: File | null = null;
  uploadError = '';
  createError = '';

  constructor(
    private http: HttpClient,
    public router: Router
  ) {}

  // Récupère le fichier sélectionné et l'upload pour récupérer l'URL
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.imageFile = input.files[0];
      const formData = new FormData();
      formData.append('file', this.imageFile, this.imageFile.name);

      this.http.post<{ imageUrl: string }>(
        'http://localhost:5292/api/products/upload',
        formData
      ).subscribe({
        next: (res) => {
          this.product.imageUrl = res.imageUrl;
        },
        error: (err) => {
          console.error('Erreur lors de l’upload de l’image', err);
          this.uploadError = 'Échec de l’upload de l’image.';
        }
      });
    }
  }

  // Soumet le produit complet au backend
  onSubmit(): void {
    if (this.product.imageUrl === '') {
      this.createError = 'Veuillez uploader une image avant de créer le produit.';
      return;
    }

    this.http.post(
      'http://localhost:5292/api/products',
      this.product
    ).subscribe({
      next: () => {
        this.router.navigate(['/admin/products']);
      },
      error: (err) => {
        console.error('Erreur lors de la création du produit', err);
        this.createError = 'Échec de la création du produit.';
      }
    });
  }
}

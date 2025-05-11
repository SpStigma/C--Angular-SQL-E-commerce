import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.css']
})
export class AddProductComponent {
  product = {
    name: '',
    description: '',
    price: 0,
    stock: 0,
    image: ''
  };

  imageFile: File | null = null;
  uploadError = '';
  createError = '';

  constructor(private http: HttpClient, private router: Router) {}

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      const formData = new FormData();
      formData.append('file', file);

      this.http.post<{ imageUrl: string }>(
        'http://localhost:5292/api/products/upload',
        formData
      ).subscribe({
        next: (res) => {
          this.product.image = res.imageUrl;
        },
        error: (err) => {
          console.error('Erreur lors de l\'upload de l\'image', err);
          this.uploadError = 'Échec de l\'upload de l\'image.';
        }
      });
    }
  }

  onSubmit() {
    this.http.post(
      'http://localhost:5292/api/products',
      this.product
    ).subscribe({
      next: () => {
        console.log('Produit ajouté avec succès');
        this.router.navigate(['/admin/products']);
      },
      error: (err) => {
        console.error('Erreur lors de l\'ajout du produit', err);
        this.createError = 'Échec de la création du produit.';
      }
    });
  }
}

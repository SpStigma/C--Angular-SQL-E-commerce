import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';

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
    this.imageFile = event.target.files[0];
  }

  onSubmit() {
    const token = localStorage.getItem('token');
    if (!token) {
      this.uploadError = 'Non autorisé.';
      return;
    }

    if (this.imageFile) {
      const formData = new FormData();
      formData.append('file', this.imageFile);

      const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

      this.http.post<any>('http://localhost:5292/api/products/upload', formData, { headers })
        .subscribe({
          next: (res) => {
            this.product.image = res.imageUrl;
            this.createProduct(token);
          },
          error: (err) => {
            this.uploadError = err.error?.message || 'Erreur lors de l\'upload';
          }
        });
    } else {
      this.createError = 'Aucune image sélectionnée';
    }
  }

  createProduct(token: string) {
    const headers = new HttpHeaders()
      .set('Authorization', `Bearer ${token}`)
      .set('Content-Type', 'application/json');

    this.http.post('http://localhost:5292/api/products', this.product, { headers })
      .subscribe({
        next: () => this.router.navigate(['/products']),
        error: (err) => {
          this.createError = err.error?.message || 'Erreur lors de la création du produit';
        }
      });
  }
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService, Product } from '../../../../services/product.service';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-products.component.html',
})
export class AdminProductsComponent implements OnInit {
  products: Product[] = [];
  stockToAdd: { [productId: number]: number } = {};

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  private loadProducts(): void {
    this.productService.getAll().subscribe(rawProducts => {
      this.products = rawProducts.map(p => {
        // Si l’API renvoie une URL relative, on ajoute le domaine
        if (p.imageUrl && p.imageUrl.startsWith('/')) {
          p.imageUrl = environment.apiUrl + p.imageUrl;
        }

        // Si aucune URL valide, on utilise un placeholder
        if (!p.imageUrl) {
          p.imageUrl = 'assets/placeholder.png';
        }

        return p;
      });

      this.initializeStockToAdd();
    });
  }

  private initializeStockToAdd(): void {
    this.stockToAdd = {};
    this.products.forEach(p => this.stockToAdd[p.id] = 0);
  }

  addStock(productId: number): void {
    const qty = this.stockToAdd[productId];
    if (qty > 0) {
      this.productService.addStock(productId, qty).subscribe({
        next: () => this.loadProducts(),
        error: err => console.error('Erreur ajout de stock', err)
      });
    }
  }

  deleteProduct(productId: number): void {
    if (confirm('Êtes-vous sûr de vouloir supprimer ce produit ?')) {
      this.productService.delete(productId).subscribe({
        next: () => this.loadProducts(),
        error: err => console.error('Erreur suppression', err)
      });
    }
  }
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService, Product } from '../../../../services/product.service';

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

  loadProducts(): void {
    this.productService.getAll().subscribe(products => {
      this.products = products;
      // Initialise stockToAdd pour chaque produit
      this.stockToAdd = {};
      products.forEach(product => {
        this.stockToAdd[product.id] = 0;
      });
    });
  }

  addStock(productId: number): void {
    const quantity = this.stockToAdd[productId];
    if (quantity > 0) {
      this.productService.addStock(productId, quantity).subscribe({
        next: () => this.loadProducts(),
        error: err => console.error('Erreur lors de l’ajout de stock', err)
      });
    }
  }

  deleteProduct(productId: number): void {
    if (confirm('Êtes-vous sûr de vouloir supprimer ce produit ?')) {
      this.productService.delete(productId).subscribe({
        next: () => this.loadProducts(),
        error: err => console.error('Erreur lors de la suppression', err)
      });
    }
  }
}

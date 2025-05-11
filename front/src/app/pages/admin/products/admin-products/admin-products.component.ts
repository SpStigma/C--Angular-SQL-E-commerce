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
      products.forEach(product => {
        this.stockToAdd[product.id] = 0;
      });
    });
  }

  addStock(productId: number): void {
    const amount = this.stockToAdd[productId];
    if (amount <= 0) return;

    this.productService.addStock(productId, amount).subscribe(() => {
      this.loadProducts();
    });
  }

  deleteProduct(productId: number): void {
    if (confirm('Êtes-vous sûr de vouloir supprimer ce produit ?')) {
      this.productService.delete(productId).subscribe(() => {
        this.loadProducts();
      });
    }
  }
}

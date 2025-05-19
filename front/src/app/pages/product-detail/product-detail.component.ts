import { Component, OnInit } from '@angular/core';
import { CommonModule }        from '@angular/common';
import { ActivatedRoute }    from '@angular/router';
import { ProductService }    from '../../services/product.service';
import { CartService }       from '../../services/cart.service';
import { Product }           from '../../models/product.model';

@Component({
  standalone: true,
  imports: [CommonModule],
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css']
})
export class ProductDetailComponent implements OnInit {
  product!: Product;
  errorMessage = '';
  successMessage = '';
  cartItemCount = 0;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private cartService: CartService
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.productService.getById(id).subscribe({
      next: data => this.product = data,
      error: ()   => this.errorMessage = 'Impossible de charger le produit'
    });
  }

  clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }

  addToCart(): void {
    this.clearMessages();
    // on envoie l'id (et la quantité par défaut = 1)
    this.cartService.addToCart(this.product.id).subscribe({
      next: () => {
        this.successMessage = `“${this.product.name}” ajouté au panier !`;
        // rafraîchir le stock
        this.productService.getById(this.product.id).subscribe({
          next: fresh => this.product.stock = fresh.stock,
          error: err => console.error('Erreur rafraîchissement stock', err)
        });
        // mettre à jour le badge
        this.cartItemCount++;
        setTimeout(() => this.clearMessages(), 3000);
      },
      error: err => {
        this.errorMessage = err.error?.message || 'Erreur lors de l’ajout';
        setTimeout(() => this.clearMessages(), 3000);
      }
    });
  }
}

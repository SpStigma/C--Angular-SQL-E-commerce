import { Component, OnInit } from '@angular/core';
import { CommonModule }      from '@angular/common';
import { RouterModule }      from '@angular/router';
import { CartService, CartItem, CartResponse } from '../../services/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cart.component.html'
})
export class CartComponent implements OnInit {
  items: CartItem[] = [];
  total = 0;

  constructor(private cartService: CartService) {}

  ngOnInit(): void {
    this.loadCart();
  }

  private loadCart(): void {
    this.cartService.getCart().subscribe({
      next: (data: CartResponse) => {
        this.items = data.items;
        this.total = data.total;
      },
      error: err => {
        console.error('Erreur chargement du panier', err);
      }
    });
  }

  clearCart(): void {
    if (!confirm('Êtes-vous sûr de vouloir vider votre panier ?')) {
      return;
    }
    this.cartService.clearCart().subscribe({
      next: () => {
        this.items = [];
        this.total = 0;
      },
      error: err => {
        console.error('Erreur lors de la suppression du panier', err);
      }
    });
  }
}

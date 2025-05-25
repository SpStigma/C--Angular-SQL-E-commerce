// front/src/app/pages/cart/cart.component.ts
import { Component, OnInit }                         from '@angular/core';
import { CommonModule }                              from '@angular/common';
import { RouterModule }                              from '@angular/router';
import { firstValueFrom }                            from 'rxjs';

import { CartService, CartItem, CartResponse }       from '../../services/cart.service';
import { OrderService }                              from '../../services/order.service';
import { PaymentService }                            from '../../services/payment.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cart.component.html'
})
export class CartComponent implements OnInit {
  items: CartItem[] = [];
  total = 0;

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  private loadCart(): void {
    this.cartService.getCart().subscribe({
      next: (res: CartResponse) => {
        this.items = res.items;
        this.total = res.total;
      },
      error: err => console.error(err)
    });
  }

  clearCart(): void {
    if (!confirm('Êtes-vous sûr de vouloir vider votre panier ?')) return;
    this.cartService.clearCart().subscribe({
      next: () => {
        this.items = [];
        this.total = 0;
      },
      error: err => console.error(err)
    });
  }

  /** 1) Place une commande, 2) lance Stripe Checkout */
  async onPay(): Promise<void> {
    try {
      const order = await firstValueFrom(this.orderService.placeOrder());
      await this.paymentService.checkout(order.id);
    } catch (error) {
      console.error('Erreur lors du paiement', error);
      alert('Une erreur est survenue. Veuillez réessayer.');
    }
  }
}

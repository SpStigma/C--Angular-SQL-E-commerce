import { Component } from '@angular/core';
import { Router }    from '@angular/router';

@Component({
  selector: 'app-payment-cancel',
  standalone: true,
  template: `
    <div class="mx-auto max-w-xl text-center mt-12">
      <h2 class="text-red-600 text-2xl font-bold mb-4">Paiement annulé</h2>
      <button class="btn btn-outline" (click)="goCart()">Retour au panier</button>
    </div>
  `
})
export class PaymentCancelComponent {
  constructor(private router: Router) {}
  goCart() { this.router.navigateByUrl('/cart'); }
}

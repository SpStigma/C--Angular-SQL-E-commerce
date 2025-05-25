// front/src/app/pages/payment-cancel/payment-cancel.component.ts
import { Component } from '@angular/core';
import { Router }    from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-payment-cancel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-cancel.component.html',
})
export class PaymentCancelComponent {
  constructor(private router: Router) {}

  goCart() {
    this.router.navigateByUrl('/cart');
  }
}

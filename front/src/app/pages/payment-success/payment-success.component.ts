import { Component, OnInit }      from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  template: `
    <div class="mx-auto max-w-xl text-center mt-12">
      <h2 class="text-green-600 text-2xl font-bold mb-4">Paiement réussi 🎉</h2>
      <p>Votre session Stripe est : <code>{{ sessionId }}</code></p>
      <button class="btn btn-primary mt-6" (click)="goHome()">Retour à l’accueil</button>
    </div>
  `
})
export class PaymentSuccessComponent implements OnInit {
  sessionId: string|null = null;
  constructor(private route: ActivatedRoute, private router: Router) {}
  ngOnInit() {
    this.sessionId = this.route.snapshot.queryParamMap.get('session_id');
  }
  goHome() { this.router.navigateByUrl('/'); }
}

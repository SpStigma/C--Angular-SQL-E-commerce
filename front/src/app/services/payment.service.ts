// front/src/app/services/payment.service.ts
import { Injectable }                 from '@angular/core';
import { HttpClient }                 from '@angular/common/http';
import { loadStripe, Stripe }         from '@stripe/stripe-js';
import { firstValueFrom }             from 'rxjs';
import { environment }                from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private stripePromise = loadStripe(environment.stripePublishableKey);

  constructor(private http: HttpClient) {}

  /** Crée une Checkout Session et redirige l'utilisateur */
  async checkout(orderId: number): Promise<void> {
    // 1) appeler l'API pour créer la session
    const { sessionId } = await firstValueFrom(
      this.http.post<{ sessionId: string }>(
        `${environment.apiUrl}/payments/create-checkout-session`,
        { orderId }
      )
    );

    // 2) rediriger vers Stripe
    const stripe = await this.stripePromise;
    if (stripe) {
      await stripe.redirectToCheckout({ sessionId });
    } else {
      console.error('Erreur Stripe : pas de stripe instance');
    }
  }
}

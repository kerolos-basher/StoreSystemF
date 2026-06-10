import { DeferredPaymentListItem } from '../../../shared/models/inventory.models';

export interface RegisterPaymentPayload {
  deferredPaymentId: number;
  amountPaid: number;
  notes: string;
}

export type { DeferredPaymentListItem };

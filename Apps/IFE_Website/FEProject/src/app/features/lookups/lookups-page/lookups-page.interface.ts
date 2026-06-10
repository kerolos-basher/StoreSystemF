import { Category, ReturnReason, Supplier } from '../../../shared/models/inventory.models';

export type LookupTab = 'categories' | 'suppliers' | 'return-reasons';

export interface LookupFormState {
  name: string;
  isReturnToStock: boolean;
}

export interface LookupsPageState {
  activeTab: LookupTab;
  categories: Category[];
  suppliers: Supplier[];
  returnReasons: ReturnReason[];
  editingId: string | number | null;
  saving: boolean;
}

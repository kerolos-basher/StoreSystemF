import { ProductDetails } from '../../../shared/models/inventory.models';

export interface ProductDetailsDialogData {
  details: ProductDetails;
  onChanged?: () => void;
}

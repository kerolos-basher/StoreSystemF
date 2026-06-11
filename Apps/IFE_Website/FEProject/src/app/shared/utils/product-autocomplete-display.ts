import { ProductDetailsAutoComplete } from '../models/inventory.models';

export function displayProductWithSupplier(item: ProductDetailsAutoComplete | string): string {
  if (typeof item === 'string') return item;
  return `${item.productName} — ${item.supplierName}`;
}

export function formatProductAutocompleteOption(
  item: ProductDetailsAutoComplete,
  includePrice = true
): string {
  const label = `${item.productName} — ${item.supplierName}`;
  if (!includePrice) return label;
  return `${label} (${item.suggestedSellingPrice.toFixed(2)} ج.م)`;
}

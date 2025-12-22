/**
 * Shared formatters for consistent number and currency formatting across the app.
 */

export const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
});

export const numberFormatter = new Intl.NumberFormat('en-US', {
  maximumFractionDigits: 0,
});

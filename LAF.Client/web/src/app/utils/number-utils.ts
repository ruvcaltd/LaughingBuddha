/**
 * Utility helpers for number parsing/formatting used across the app.
 */

/**
 * Parse a user input value into a numeric value in base units.
 * Accepts numbers like '3' or strings like '3M' / '3m' and converts to 3,000,000.
 * If the value already contains separators or is a full number, it will be parsed as-is.
 */
export function parseMillionInput(value: any): number {
  if (value == null || value === '') return 0;

  // If already a number, assume user entered full amount or shorthand number
  if (typeof value === 'number') {
    // Heuristic: if number is small treat it as millions shorthand
    if (Math.abs(value) <= 1000000 && Math.abs(value) <= 10000) {
      return value * 1000000;
    }
    return value;
  }

  const raw = String(value).trim();

  // supports 3, 3M, 3m, 3.5, 3.5M
  const mMatch = raw.match(/^(-?[0-9,.]+)\s*[mM]$/);
  if (mMatch) {
    const num = Number(mMatch[1].replace(/,/g, ''));
    return isNaN(num) ? 0 : Math.round(num * 1000000);
  }

  // plain number
  const asNum = Number(raw.replace(/,/g, ''));
  if (!isNaN(asNum)) {
    // Heuristic: if user entered a small number (<= 10_000) treat as millions shorthand.
    if (Math.abs(asNum) <= 10000) {
      return Math.round(asNum * 1000000);
    }
    return asNum;
  }

  return 0;
}

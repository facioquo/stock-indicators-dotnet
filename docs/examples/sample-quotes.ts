import type { Bar } from '@facioquo/indy-charts'

/**
 * Forty synthetic daily OHLCV bars (Jan 2 - Feb 28, 2025).
 *
 * `Bar.timestamp` accepts either an ISO 8601 string or a `Date` instance;
 * `loadStaticQuotes` normalizes string timestamps to `Date` at load time.
 */
export const SAMPLE_QUOTES: Bar[] = [
  { timestamp: '2025-01-02', open: 180.0, high: 182.5, low: 179.2, close: 181.8, volume: 38500000 },
  { timestamp: '2025-01-03', open: 181.8, high: 183.4, low: 180.5, close: 182.6, volume: 32100000 },
  { timestamp: '2025-01-06', open: 182.6, high: 184.2, low: 181.0, close: 183.4, volume: 29800000 },
  { timestamp: '2025-01-07', open: 183.4, high: 185.1, low: 182.3, close: 184.9, volume: 35200000 },
  { timestamp: '2025-01-08', open: 184.9, high: 186.5, low: 183.6, close: 185.8, volume: 41600000 },
  { timestamp: '2025-01-09', open: 185.8, high: 186.2, low: 183.0, close: 183.5, volume: 44900000 },
  { timestamp: '2025-01-10', open: 183.5, high: 185.0, low: 182.4, close: 184.2, volume: 33700000 },
  { timestamp: '2025-01-13', open: 184.2, high: 185.8, low: 183.1, close: 185.4, volume: 38200000 },
  { timestamp: '2025-01-14', open: 185.4, high: 187.6, low: 184.9, close: 187.2, volume: 52300000 },
  { timestamp: '2025-01-15', open: 187.2, high: 188.5, low: 186.0, close: 187.8, volume: 39100000 },
  { timestamp: '2025-01-16', open: 187.8, high: 188.9, low: 185.2, close: 185.9, volume: 48600000 },
  { timestamp: '2025-01-17', open: 185.9, high: 187.1, low: 184.5, close: 186.6, volume: 31400000 },
  { timestamp: '2025-01-21', open: 186.6, high: 188.2, low: 185.8, close: 187.9, volume: 36800000 },
  { timestamp: '2025-01-22', open: 187.9, high: 189.3, low: 186.7, close: 188.5, volume: 43200000 },
  { timestamp: '2025-01-23', open: 188.5, high: 190.0, low: 187.4, close: 189.7, volume: 55600000 },
  { timestamp: '2025-01-24', open: 189.7, high: 191.2, low: 188.6, close: 190.3, volume: 48900000 },
  { timestamp: '2025-01-27', open: 190.3, high: 191.5, low: 188.8, close: 189.1, volume: 37500000 },
  { timestamp: '2025-01-28', open: 189.1, high: 190.4, low: 186.9, close: 187.4, volume: 52100000 },
  { timestamp: '2025-01-29', open: 187.4, high: 188.9, low: 186.2, close: 188.7, volume: 34800000 },
  { timestamp: '2025-01-30', open: 188.7, high: 190.5, low: 187.8, close: 190.2, volume: 41300000 },
  { timestamp: '2025-01-31', open: 190.2, high: 192.8, low: 189.5, close: 192.4, volume: 68900000 },
  { timestamp: '2025-02-03', open: 192.4, high: 193.6, low: 191.3, close: 192.9, volume: 44200000 },
  { timestamp: '2025-02-04', open: 192.9, high: 194.1, low: 191.8, close: 193.6, volume: 39700000 },
  { timestamp: '2025-02-05', open: 193.6, high: 195.2, low: 192.4, close: 194.8, volume: 46500000 },
  { timestamp: '2025-02-06', open: 194.8, high: 196.0, low: 193.2, close: 194.1, volume: 38300000 },
  { timestamp: '2025-02-07', open: 194.1, high: 194.8, low: 191.4, close: 191.9, volume: 53700000 },
  { timestamp: '2025-02-10', open: 191.9, high: 193.4, low: 190.8, close: 193.1, volume: 35200000 },
  { timestamp: '2025-02-11', open: 193.1, high: 194.6, low: 192.0, close: 194.3, volume: 42800000 },
  { timestamp: '2025-02-12', open: 194.3, high: 196.5, low: 193.7, close: 196.1, volume: 59400000 },
  { timestamp: '2025-02-13', open: 196.1, high: 197.8, low: 195.2, close: 197.4, volume: 47600000 },
  { timestamp: '2025-02-14', open: 197.4, high: 198.5, low: 195.8, close: 196.2, volume: 43100000 },
  { timestamp: '2025-02-18', open: 196.2, high: 197.4, low: 194.6, close: 195.8, volume: 38900000 },
  { timestamp: '2025-02-19', open: 195.8, high: 197.2, low: 194.8, close: 196.9, volume: 36700000 },
  { timestamp: '2025-02-20', open: 196.9, high: 198.6, low: 196.0, close: 198.2, volume: 44500000 },
  { timestamp: '2025-02-21', open: 198.2, high: 199.8, low: 197.5, close: 199.4, volume: 51200000 },
  { timestamp: '2025-02-24', open: 199.4, high: 200.5, low: 197.8, close: 198.7, volume: 48700000 },
  { timestamp: '2025-02-25', open: 198.7, high: 200.1, low: 197.2, close: 199.9, volume: 42300000 },
  { timestamp: '2025-02-26', open: 199.9, high: 201.4, low: 199.1, close: 200.8, volume: 55100000 },
  { timestamp: '2025-02-27', open: 200.8, high: 202.5, low: 199.6, close: 201.7, volume: 63800000 },
  { timestamp: '2025-02-28', open: 201.7, high: 203.2, low: 200.5, close: 202.4, volume: 57200000 }
]

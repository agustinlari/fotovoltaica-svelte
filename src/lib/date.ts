export function parseDdMmYyyy(value: string): Date | null {
  const trimmed = (value || '').trim();
  const match = /^([0-3]?\d)\/([0-1]?\d)\/(\d{4})$/.exec(trimmed);
  if (!match) return null;
  const day = Number(match[1]);
  const month = Number(match[2]);
  const year = Number(match[3]);
  if (month < 1 || month > 12 || day < 1 || day > 31) return null;
  const date = new Date(Date.UTC(year, month - 1, day, 0, 0, 0, 0));
  return isNaN(date.getTime()) ? null : date;
}

export function toIsoMidnight(value: string): string | null {
  const d = parseDdMmYyyy(value);
  if (!d) return null;
  return d.toISOString();
}

export function formatDdMmYyyyFromIso(iso?: string | null): string {
  if (!iso) return '';
  const d = new Date(iso);
  if (isNaN(d.getTime())) return '';
  const dd = String(d.getUTCDate()).padStart(2, '0');
  const mm = String(d.getUTCMonth() + 1).padStart(2, '0');
  const yyyy = d.getUTCFullYear();
  return `${dd}/${mm}/${yyyy}`;
}



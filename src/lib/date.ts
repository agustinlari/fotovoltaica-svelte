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

export function toIsoMidnight(value: string | Date | null | undefined): Date | null {
  if (!value) return null;
  
  // Si ya es un objeto Date, devolverlo
  if (value instanceof Date) {
    return isNaN(value.getTime()) ? null : value;
  }
  
  // Convertir a string si no lo es
  const stringValue = String(value);
  
  // Si ya es una fecha ISO (YYYY-MM-DD) del input type="date"
  if (/^\d{4}-\d{2}-\d{2}$/.test(stringValue)) {
    const d = new Date(stringValue + 'T00:00:00.000Z');
    return isNaN(d.getTime()) ? null : d;
  }
  
  // Si es formato DD/MM/YYYY (legacy)
  const d = parseDdMmYyyy(stringValue);
  return d;
}

export function formatDdMmYyyyFromIso(iso?: string | Date | null): string {
  if (!iso) return '';
  
  // Si ya es un objeto Date, usar directamente
  let d: Date;
  if (iso instanceof Date) {
    d = iso;
  } else {
    d = new Date(iso);
  }
  
  if (isNaN(d.getTime())) return '';
  const dd = String(d.getUTCDate()).padStart(2, '0');
  const mm = String(d.getUTCMonth() + 1).padStart(2, '0');
  const yyyy = d.getUTCFullYear();
  return `${dd}/${mm}/${yyyy}`;
}

export function formatYyyyMmDdFromIso(iso?: string | Date | null): string {
  if (!iso) return '';
  
  // Si ya es un objeto Date, usar directamente
  let d: Date;
  if (iso instanceof Date) {
    d = iso;
  } else {
    d = new Date(iso);
  }
  
  if (isNaN(d.getTime())) return '';
  const dd = String(d.getUTCDate()).padStart(2, '0');
  const mm = String(d.getUTCMonth() + 1).padStart(2, '0');
  const yyyy = d.getUTCFullYear();
  return `${yyyy}-${mm}-${dd}`;
}



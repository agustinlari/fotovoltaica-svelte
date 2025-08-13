#!/usr/bin/env bash
set -euo pipefail

# Reads backend/.env DATABASE_URL, creates DB if not exists, and applies schema SQL

ENV_FILE="/home/osmos/proyectos/fotovoltaica/backend/.env"
SCHEMA_SQL="/home/osmos/proyectos/fotovoltaica/scripts/schema_fotovoltaica.sql"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "Missing .env at $ENV_FILE" >&2
  exit 1
fi

DATABASE_URL=$(grep -E '^\s*DATABASE_URL=' "$ENV_FILE" | sed -E 's/^\s*DATABASE_URL=//')
if [[ -z "${DATABASE_URL:-}" ]]; then
  echo "DATABASE_URL not set in $ENV_FILE" >&2
  exit 1
fi

DB_NAME=$(printf '%s' "$DATABASE_URL" | sed -E 's|.*/([^/?]+)(\?.*)?$|\1|')
# Build admin URL by swapping the database name to 'postgres'
ADMIN_URL=$(printf '%s' "$DATABASE_URL" | sed -E 's|/([^/?]+)(\?.*)?$|/postgres\2|')

echo "Creating database '$DB_NAME' if not exists"
psql "$ADMIN_URL" -v ON_ERROR_STOP=1 -Atc "SELECT 1 FROM pg_database WHERE datname = '$DB_NAME'" | grep -q 1 || \
  psql "$ADMIN_URL" -v ON_ERROR_STOP=1 -c "CREATE DATABASE \"$DB_NAME\";"

echo "Applying schema at $SCHEMA_SQL"
psql "$DATABASE_URL" -v ON_ERROR_STOP=1 -f "$SCHEMA_SQL"

echo "Done."



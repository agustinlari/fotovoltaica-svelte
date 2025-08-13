#!/usr/bin/env bash
set -euo pipefail

# Deployment script for Fotovoltaica frontend (Svelte) and backend (Hono)
# - Does not modify existing Nginx server blocks directly
# - Creates a snippet to include inside your existing 4444 SSL server block
# - Sets up a systemd service for the backend

FRONTEND_DIR="/home/osmos/proyectos/fotovoltaica"
BACKEND_DIR="/home/osmos/proyectos/fotovoltaica/backend"
WEB_ROOT="/var/www/fotovoltaica"
NGINX_SNIPPET="/etc/nginx/snippets/fotovoltaica.conf"
SERVICE_FILE="/etc/systemd/system/fotovoltaica-backend.service"

echo "[1/6] Building frontend (Svelte)"
pushd "$FRONTEND_DIR" >/dev/null
VITE_API_BASE=/api/fotovoltaica npm run build
popd >/dev/null

echo "[2/6] Syncing built assets to $WEB_ROOT"
sudo mkdir -p "$WEB_ROOT"
sudo rsync -a --delete "$FRONTEND_DIR/dist/" "$WEB_ROOT/"

echo "[3/6] Writing Nginx snippet to $NGINX_SNIPPET"
sudo mkdir -p /etc/nginx/snippets
sudo tee "$NGINX_SNIPPET" >/dev/null <<'NGINX'
# Fotovoltaica frontend (static)
location /fotovoltaica/ {
    limit_req zone=app burst=40 nodelay;
    alias /var/www/fotovoltaica/;
    index index.html index.htm;
    try_files $uri $uri/ /fotovoltaica/index.html;
}

# Fotovoltaica API (Hono on 8787)
location /api/fotovoltaica/ {
    client_max_body_size 20M;
    limit_req zone=app burst=20 nodelay;

    # CORS (adjust if needed)
    add_header 'Access-Control-Allow-Origin' '*' always;
    add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, PATCH, OPTIONS' always;
    add_header 'Access-Control-Allow-Headers' 'Content-Type, Authorization, X-Requested-With, Cache-Control, Pragma' always;
    if ($request_method = 'OPTIONS') {
        add_header 'Access-Control-Max-Age' 86400 always;
        return 204;
    }

    proxy_pass http://127.0.0.1:8787/;

    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection 'upgrade';
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_cache_bypass $http_upgrade;

    proxy_connect_timeout 5s;
    proxy_send_timeout 10s;
    proxy_read_timeout 10s;
}
NGINX

echo "[4/6] Creating systemd service for backend at $SERVICE_FILE"
sudo tee "$SERVICE_FILE" >/dev/null <<'SERVICE'
[Unit]
Description=Fotovoltaica Hono Backend
After=network.target

[Service]
Type=simple
WorkingDirectory=/home/osmos/proyectos/fotovoltaica/backend
EnvironmentFile=/home/osmos/proyectos/fotovoltaica/backend/.env
ExecStart=/usr/bin/node /home/osmos/proyectos/fotovoltaica/backend/dist/index.js
Restart=always
RestartSec=3
User=osmos
Group=osmos

[Install]
WantedBy=multi-user.target
SERVICE

echo "[5/6] Building backend (TypeScript)"
pushd "$BACKEND_DIR" >/dev/null
npm run build
popd >/dev/null

cat <<INFO

Done.

Next steps (manual):
1) Include the snippet inside your existing 4444 SSL server block and reload Nginx:
   - Edit your server block (e.g., /etc/nginx/sites-enabled/default or your vhost):
     inside 'server { ... }' add:
       include /etc/nginx/snippets/fotovoltaica.conf;

   - Test and reload:
       sudo nginx -t && sudo systemctl reload nginx

2) Configure backend environment variables in /home/osmos/proyectos/fotovoltaica/backend/.env
   Example:
       DATABASE_URL=postgresql://postgres:postgres@localhost:5432/fotovoltaica
       PORT=8787

3) Enable and start backend service:
       sudo systemctl daemon-reload
       sudo systemctl enable --now fotovoltaica-backend

Frontend will be available at:
   https://aplicaciones.osmos.es:4444/fotovoltaica/

API base (production):
   https://aplicaciones.osmos.es:4444/api/fotovoltaica/

INFO

echo "[6/6] Script finished"


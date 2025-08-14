server {
    listen 4444 ssl;
    listen [::]:4444 ssl;
    server_name aplicaciones.osmos.es;

    # Rutas a tu certificado autofirmado y clave privada
    ssl_certificate /etc/nginx/ssl/nginx-selfsigned.crt;
    ssl_certificate_key /etc/nginx/ssl/nginx-selfsigned.key;

    # Opcional pero recomendado: Parámetros de SSL robustos
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;

    # ================== APLICACIONES SVELTE ==================
    
    # Stock Taller
    location /stocktaller/ {
        limit_req zone=app burst=40 nodelay;
        alias /var/www/stocktaller/;
        index index.html index.htm;
        try_files $uri $uri/ /stocktaller/index.html;
    }

    # Almacén
    location /almacen/ {
        limit_req zone=app burst=40 nodelay;
        alias /var/www/almacen/;
        index index.html index.htm;
        try_files $uri $uri/ /almacen/index.html;
    }

    # Kanban
    location /kanban/ {
        limit_req zone=app burst=40 nodelay;
        alias /var/www/kanban/;
        index index.html index.htm;
        try_files $uri $uri/ /kanban/index.html;
    }

    # Fotovoltaica
    location /fotovoltaica/ {
        limit_req zone=app burst=40 nodelay;
        alias /var/www/fotovoltaica/;
        index index.html index.htm;
        try_files $uri $uri/ /fotovoltaica/index.html;
    }

    # ================== ARCHIVOS ESTÁTICOS ==================
    
    # Uploads generales
    location /public/uploads/ {
        alias /home/osmos/hono-api/uploads_data/;
        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' 'Content-Type, Cache-Control, Pragma, Authorization' always;

        if ($request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Access-Control-Allow-Methods' 'GET, OPTIONS' always;
            add_header 'Access-Control-Allow-Headers' 'Content-Type, Cache-Control, Pragma, Authorization' always;
            add_header 'Access-Control-Max-Age' 86400 always;
            return 204;
        }

        expires 30d;
        add_header Cache-Control "public";
        access_log off;
    }

    # Uploads Kanban
    location /public/kanban-uploads/ {
        alias /home/osmos/hono-kanban/uploads_data/;
        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' 'Content-Type, Cache-Control, Pragma, Authorization' always;

        if ($request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Access-Control-Allow-Methods' 'GET, OPTIONS' always;
            add_header 'Access-Control-Allow-Headers' 'Content-Type, Cache-Control, Pragma, Authorization' always;
            add_header 'Access-Control-Max-Age' 86400 always;
            return 204;
        }

        expires 30d;
        add_header Cache-Control "public";
        access_log off;
    }

    # Uploads Fotovoltaica
    location /public/fotovoltaica-uploads/ {
        alias /home/osmos/proyectos/fotovoltaica/backend/uploads_data/;
        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' 'Content-Type, Cache-Control, Pragma, Authorization' always;

        if ($request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Access-Control-Allow-Methods' 'GET, OPTIONS' always;
            add_header 'Access-Control-Allow-Headers' 'Content-Type, Cache-Control, Pragma, Authorization' always;
            add_header 'Access-Control-Max-Age' 86400 always;
            return 204;
        }

        expires 30d;
        add_header Cache-Control "public";
        access_log off;
    }

    # ================== APIS ==================
    
    # API Kanban
    location /api/kanban/ {
        client_max_body_size 20M;
        limit_req zone=app burst=20 nodelay;

        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, PATCH, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' 'Content-Type, Authorization, X-Requested-With, Cache-Control, Pragma' always;

        if ($request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, PATCH, OPTIONS' always;
            add_header 'Access-Control-Allow-Headers' 'Content-Type, Authorization, X-Requested-With, Cache-Control, Pragma' always;
            add_header 'Access-Control-Max-Age' 86400 always;
            return 204;
        }

        # Proxy a puerto 3001 (elimina prefijo /api/kanban)
        proxy_pass http://localhost:3001/;
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

    # API Fotovoltaica
    location /api/fotovoltaica/ {
        client_max_body_size 20M;
        limit_req zone=app burst=20 nodelay;

        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, PATCH, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' 'Content-Type, Authorization, X-Requested-With, Cache-Control, Pragma' always;

        if ($request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, PATCH, OPTIONS' always;
            add_header 'Access-Control-Allow-Headers' 'Content-Type, Authorization, X-Requested-With, Cache-Control, Pragma' always;
            add_header 'Access-Control-Max-Age' 86400 always;
            return 204;
        }

        # Proxy a puerto 8787 (elimina prefijo /api/fotovoltaica)
        proxy_pass http://localhost:8787/;
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
    
    # ================== KEYCLOAK ==================
    
    location /auth/ {
        proxy_pass http://127.0.0.1:8080/auth/;
        
        # Headers esenciales para HTTPS
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        
        # CRÍTICO: Estos headers le dicen a Keycloak que está en HTTPS
        proxy_set_header X-Forwarded-Proto https;
        proxy_set_header X-Forwarded-Host $host:4444;
        proxy_set_header X-Forwarded-Port 4444;
        
        # Header específico para Keycloak
        proxy_set_header X-Forwarded-Prefix /auth;
        
        # Configuración de proxy
        proxy_buffering off;
        proxy_buffer_size 128k;
        proxy_buffers 4 256k;
        proxy_busy_buffers_size 256k;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        # Para WebSockets
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }

    # ================== APLICACIÓN PRINCIPAL ==================
    
    location / {
        client_max_body_size 20M;
        limit_req zone=app burst=20 nodelay;    
        proxy_pass http://localhost:3000;
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
}
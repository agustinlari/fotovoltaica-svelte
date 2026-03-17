#!/bin/bash

# Script de despliegue para Fotovoltaica
# Compila el frontend y lo publica en producción

set -e  # Salir si hay algún error

# Colores para el output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Directorios
PROJECT_DIR="/home/osmos/proyectos/fotovoltaica"
FRONTEND_DIR="$PROJECT_DIR"
PRODUCTION_DIR="/var/www/fotovoltaica"
BACKEND_DIR="$PROJECT_DIR/backend"

echo -e "${BLUE}🚀 Iniciando despliegue de Fotovoltaica...${NC}"

# Verificar que estemos en el directorio correcto
if [ ! -d "$PROJECT_DIR" ]; then
    echo -e "${RED}❌ Error: No se encuentra el directorio del proyecto: $PROJECT_DIR${NC}"
    exit 1
fi

cd "$PROJECT_DIR"

# 1. Compilar el frontend
echo -e "${YELLOW}📦 Compilando el frontend...${NC}"
cd "$FRONTEND_DIR"

if [ ! -f "package.json" ]; then
    echo -e "${RED}❌ Error: No se encuentra package.json en $FRONTEND_DIR${NC}"
    exit 1
fi

# Instalar dependencias si es necesario
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}📥 Instalando dependencias del frontend...${NC}"
    npm install
fi

# Compilar
echo -e "${YELLOW}🔨 Ejecutando build...${NC}"
npm run build

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Error en la compilación del frontend${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Frontend compilado correctamente${NC}"

# 2. Verificar que el directorio dist existe
cd "$PROJECT_DIR"
if [ ! -d "dist" ]; then
    echo -e "${RED}❌ Error: No se encuentra el directorio dist después de la compilación${NC}"
    exit 1
fi

# 3. Hacer backup del directorio de producción actual
echo -e "${YELLOW}💾 Creando backup de la versión actual...${NC}"
BACKUP_DIR="/tmp/fotovoltaica_backup_$(date +%Y%m%d_%H%M%S)"
sudo cp -r "$PRODUCTION_DIR" "$BACKUP_DIR" 2>/dev/null || true
echo -e "${GREEN}✅ Backup creado en: $BACKUP_DIR${NC}"

# 4. Copiar archivos compilados a producción
echo -e "${YELLOW}📋 Copiando archivos a producción...${NC}"
sudo cp -r dist/* "$PRODUCTION_DIR/"

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Error copiando archivos a producción${NC}"
    echo -e "${YELLOW}🔄 Restaurando backup...${NC}"
    sudo rm -rf "$PRODUCTION_DIR"/*
    sudo cp -r "$BACKUP_DIR"/* "$PRODUCTION_DIR/"
    exit 1
fi

# 5. Establecer permisos correctos
echo -e "${YELLOW}🔒 Estableciendo permisos...${NC}"
sudo chown -R www-data:www-data "$PRODUCTION_DIR"
sudo chmod -R 755 "$PRODUCTION_DIR"

# 6. Compilar y reiniciar el backend
echo -e "${YELLOW}⚙️ Compilando backend...${NC}"
cd "$BACKEND_DIR"

if [ ! -f "package.json" ]; then
    echo -e "${RED}❌ Error: No se encuentra package.json en $BACKEND_DIR${NC}"
    exit 1
fi

# Instalar dependencias del backend si es necesario
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}📥 Instalando dependencias del backend...${NC}"
    npm install
fi

# Compilar backend
echo -e "${YELLOW}🔨 Compilando backend...${NC}"
npm run build

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Error en la compilación del backend${NC}"
    exit 1
fi

# 7. Reiniciar el backend con PM2
echo -e "${YELLOW}🔄 Reiniciando backend...${NC}"
pm2 restart fotovoltaica-backend

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Error reiniciando el backend${NC}"
    exit 1
fi

# 8. Verificar que el backend esté funcionando
echo -e "${YELLOW}🔍 Verificando estado del backend...${NC}"
sleep 3
pm2 status fotovoltaica-backend

# 9. Probar que el frontend esté accesible
echo -e "${YELLOW}🌐 Verificando que el frontend esté accesible...${NC}"
if [ -f "$PRODUCTION_DIR/index.html" ]; then
    echo -e "${GREEN}✅ Frontend accesible en $PRODUCTION_DIR${NC}"
else
    echo -e "${RED}❌ Error: No se encuentra index.html en producción${NC}"
    exit 1
fi

# 10. Mostrar información del despliegue
echo -e "${GREEN}🎉 ¡Despliegue completado exitosamente!${NC}"
echo -e "${BLUE}📊 Información del despliegue:${NC}"
echo -e "  📂 Directorio de producción: $PRODUCTION_DIR"
echo -e "  💾 Backup guardado en: $BACKUP_DIR"
echo -e "  🕒 Fecha y hora: $(date)"
echo -e "  📝 Archivos en producción:"
ls -la "$PRODUCTION_DIR" | head -10

echo -e "${GREEN}✅ La aplicación debería estar disponible en el servidor web${NC}"

# Guardar PM2
pm2 save

echo -e "${BLUE}🏁 Script de despliegue finalizado${NC}"
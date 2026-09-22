# Inventario UPS

## Actualización: Id interno de UPS

Antes de iniciar esta versión, siga [las instrucciones de base de datos](backend/Database/README.md).
Ejecute `backend/Database/20260922_UPSIdInterno.sql`; reemplaza la propuesta anterior
de usar inventario como clave primaria. Id es la clave interna automática,
inventario y serie son únicos y editables, y Codigo se elimina de UPS.
La migración admite ambas versiones anteriores y conserva los mantenimientos.

## Ejecutar en desarrollo

Requisitos: .NET 8, Node.js y la base de datos SQL Server configurada.
Las actualizaciones de base de datos se describen en backend/Database/README.md.

Desde la raíz del proyecto, instala las dependencias si todavía no están instaladas:

```powershell
npm --prefix frontend ci
```

En una terminal, inicia la API con el perfil HTTPS:

```powershell
npm run dev:api
```

También puedes iniciarla desde Visual Studio seleccionando el perfil https.
Utiliza solo una de estas opciones para la API.

En otra terminal, inicia el frontend desde la raíz:

```powershell
npm run dev
```

También funciona npm run dev dentro de frontend.

Abre la dirección Local que indique Vite. Intenta usar 5173 y, si está
ocupado, selecciona el siguiente puerto disponible. No detiene otros procesos.
Si tienes otra instancia del frontend abierta, puedes cerrarla con Ctrl+C
en su terminal.

## Conexión con la API

En desarrollo, el navegador solicita /api en el mismo origen del frontend.
Vite reenvía esas solicitudes a https://localhost:7295, que corresponde
al perfil https del backend. Esto evita la redirección HTTP y permite
usar un puerto alternativo del frontend sin cambiar CORS en el backend.

El proxy local acepta el certificado de desarrollo autofirmado.
La validación HTTPS de la API y su configuración de producción no se modifican.
Si el proxy muestra ECONNREFUSED, confirma que la API está ejecutándose
con el perfil https en el puerto 7295.

VITE_API_URL se aplica únicamente al compilar para producción.
Por defecto, la compilación usa /api y requiere que el servidor de despliegue
publique o redirija esa ruta a la API. Para una API en otro origen,
configura VITE_API_URL antes de compilar y autoriza el origen en CORS.
npm run preview dentro de frontend también incluye el proxy local para pruebas.

## Descargar cambios

Desde la raíz:

```powershell
git switch desarrollo
git pull --ff-only origin desarrollo
```

Reinicia la API y el frontend después de actualizar. Si Git informa cambios
locales que impiden actualizar, revísalos antes de continuar.

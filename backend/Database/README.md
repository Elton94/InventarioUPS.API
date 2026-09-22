# Base de datos: Id interno, inventario y serie únicos

## Diseño vigente

- `dbo.UPSs.Id`: clave primaria numérica generada automáticamente (`IDENTITY`).
- `NumeroInventario`: obligatorio y único, visible y editable en la aplicación.
- `NumeroSerie`: obligatorio y único, también editable.
- `Codigo`: eliminado de UPS. Los códigos de agencias y permisos se conservan.
- `dbo.Mantenimientos.UPSId`: referencia al Id estable del UPS.

El usuario no escribe ni ve el Id interno en los formularios o tablas.
Corregir inventario o serie no cambia la identidad del equipo ni sus mantenimientos.

## Actualizar una instalación existente

1. Detenga la API y respalde `InventarioUPSDB` en SSMS.
2. Descargue la rama `desarrollo`.
3. Abra y ejecute completo `backend/Database/20260922_UPSIdInterno.sql`.
4. Si muestra identificadores repetidos, vacíos o demasiado largos, corríjalos con
   sus valores reales y vuelva a ejecutar. El script revierte toda la operación
   cuando encuentra un problema, sin borrar equipos ni mantenimientos.
5. Al terminar mostrará los conteos de UPS y mantenimientos conservados.
6. Compile/reinicie la API y el frontend y recargue la página.

Para retirar los costos de mantenimiento, aplique también el script de la sección siguiente.
NO ejecute el script anterior
`20260922_InventarioClavePrimariaUPS.sql`: esa decisión fue reemplazada.
Tampoco vuelva a ejecutar `20260922_IdentificadoresUnicosUPS.sql`, que contemplaba Codigo.

El script vigente admite:

| Estado anterior | Resultado |
|---|---|
| UPS con Id numérico y Codigo | Conserva los Id y relaciones; elimina Codigo. |
| Inventario como PK, Id ya eliminado | Genera nuevos Id internos y recupera las relaciones usando inventario. |
| Ya actualizado a Id interno sin Codigo | Revalida y conserva los Id y relaciones existentes. |

Requiere las actualizaciones previas de lógica de negocio (20260919) y de inventario
(20260921), que ya utilizaba la aplicación. Los valores `INV-PEND-...` únicos se
conservan y pueden reemplazarse por los números reales desde el formulario de edición.

Se eliminan columnas calculadas auxiliares de las versiones anteriores si existen.
Una relación adicional desconocida hacia UPS detiene el proceso para no romperla.
El script recorta espacios exteriores; no inventa números de inventario o serie.
Las comparaciones no distinguen mayúsculas/minúsculas y conservan acentos.
SQL Server protege contra duplicados incluso en escrituras simultáneas.

Los archivos de migración históricos describen el esquema inicial; la actualización
local se aplica con estos scripts. No use `Add-Migration` ni `Update-Database` para
este cambio. El DbContext contiene el modelo utilizado por la aplicación.

## Eliminar costos de mantenimientos

Si ya aplicó `20260922_UPSIdInterno.sql`, el único paso nuevo es:

1. Detenga la API y respalde `InventarioUPSDB` en SSMS.
2. Descargue la rama `desarrollo` y ejecute completo
   `backend/Database/20260922_MantenimientosSinCostos.sql`.
3. Confirme el mensaje de éxito y el conteo de mantenimientos conservados.
4. Reinicie la API y el frontend. Pruebe crear y editar un mantenimiento y completar
   un cambio de baterías; revise también las proyecciones.

El script elimina `Costo` y `CostoBaterias` de `dbo.Mantenimientos`, incluidos sus
importes históricos. Recuperarlos requiere el respaldo anterior. No elimina filas,
fechas, cantidades de baterías ni relaciones con los UPS. Los costos de Proyecciones
permanecen en su configuración independiente.

Puede ejecutarse de nuevo: omite las columnas que ya no existen. Retira sus valores
predeterminados; si otra dependencia impide eliminarlas, revierte toda la operación
y muestra el error. No vuelva a iniciar una versión anterior de la API que espere
esas columnas. No vuelva a ejecutar scripts históricos para recrearlas.

## Contrato de la API

| Operación | Ruta |
|---|---|
| Listar UPS | `GET /api/UPS` |
| Consultar UPS | `GET /api/UPS/{id}` |
| Crear UPS | `POST /api/UPS`, con inventario y serie, sin Codigo; Id generado por el servidor |
| Editar UPS | `PUT /api/UPS/{id}`, con el mismo Id e inventario/serie corregidos |
| Cambiar estado | `PATCH /api/UPS/{id}/estado?estado=...` |
| Mantenimientos del UPS | `GET /api/Mantenimiento/ups/{upsId}` |

Los mantenimientos envían `upsId` numérico. El DTO muestra `numeroInventarioUPS`
como información del equipo, no como clave de la relación. Presupuesto sigue el
mismo criterio. El frontend usa los Id internamente y muestra el inventario.

## Verificación

- Crear un UPS: no se solicita Codigo ni Id.
- Editar su inventario/serie: debe conservar sus mantenimientos.
- Intentar inventario o serie de otro UPS: debe impedir guardar.
- Guardar un UPS sin cambiar sus identificadores: debe permitirlo.
- Revisar mantenimiento, cambios de estado y presupuesto después de la migración.

```powershell
dotnet test backend/InventarioUPS.Tests
npm --prefix frontend test
npm run build
```

Las pruebas usan SQLite en memoria para los flujos relacionales y el parser T-SQL
de Microsoft para la sintaxis del script. No sustituyen ejecutar la migración en
SQL Server sobre una copia respaldada de la base local.

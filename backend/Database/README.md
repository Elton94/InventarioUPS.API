# Base de datos: inventario como clave primaria de UPS

## Actualización vigente

La versión actual elimina `Codigo` e `Id` de `dbo.UPSs`. Su clave primaria es
`NumeroInventario` (texto obligatorio de hasta 100 caracteres). `NumeroSerie`
continúa siendo único. El código de agencia permanece, pues identifica otra entidad.

`dbo.Mantenimientos.NumeroInventarioUPS` reemplaza a `UPSId` y referencia el
inventario del UPS. El script obtiene esa correspondencia a partir del Id anterior;
no cambia el Id propio del mantenimiento ni sus fechas, costos o historial.

### Aplicar sobre una instalación existente

1. Antes de actualizar, sustituya los inventarios temporales `INV-PEND-...` por los
   números reales y corrija series o inventarios repetidos con la aplicación anterior.
2. Respalde `InventarioUPSDB` en SSMS y detenga la API. Esta actualización elimina
   las columnas antiguas de UPS; el respaldo conserva también sus códigos anteriores.
3. Descargue la rama `desarrollo`.
4. Abra y ejecute completo `backend/Database/20260922_InventarioClavePrimariaUPS.sql`.
5. Si muestra conflictos, no aplicó la migración: corrija los datos indicados antes de
   intentarlo otra vez. No invente identificadores ni elimine equipos para superar el control.
6. Al terminar mostrará la cantidad de UPS y mantenimientos conservados.
7. Compile y reinicie la API y el frontend, y recargue la página.

El script requiere las actualizaciones `20260919_LogicaNegocio.sql` y
`20260921_NumeroInventarioUPS.sql` que ya usaba la aplicación. No necesita el script
anterior de unicidad de tres campos. Retira las columnas calculadas auxiliares si
se llegó a ejecutar esa versión. Si detecta otra relación desconocida hacia UPS,
se detiene para evitar romperla. Ejecutarlo de nuevo sobre el esquema migrado
informa que ya está aplicado.

Los scripts y migraciones anteriores describen el esquema histórico: no deben
volver a ejecutarse después de esta actualización. Este proyecto mantiene las
actualizaciones locales mediante SQL; no use `Add-Migration` ni `Update-Database`
para aplicar este cambio. El modelo actual del DbContext es el que utiliza la API.

### Comportamiento

- El inventario conserva letras, ceros iniciales y caracteres especiales.
- La API limpia espacios exteriores y comprueba inventario y serie antes de guardar.
- SQL Server usa una clave primaria en inventario y un índice único en serie,
  sin distinguir mayúsculas/minúsculas. No agrega columnas auxiliares a UPS.
- El inventario no se modifica al editar un UPS: es su identidad y la referencia
  de mantenimientos. Corrija los números reales antes de la migración.
- Se permite editar marca, modelo, serie y demás datos del equipo conservando la clave.
- No cambia la clasificación Local/Foránea, las reglas de mantenimiento ni los costos
  y ciclos del presupuesto.

### Contrato API actualizado

| Operación | Ruta |
|---|---|
| Listar UPS | `GET /api/UPS` |
| Consultar UPS | `GET /api/UPS/detalle?numeroInventario=...` |
| Crear UPS | `POST /api/UPS` con `numeroInventario`, sin `id` ni `codigo` |
| Editar UPS | `PUT /api/UPS?numeroInventario=...` |
| Cambiar estado | `PATCH /api/UPS/estado?numeroInventario=...&estado=...` |
| Mantenimientos del UPS | `GET /api/Mantenimiento/ups?numeroInventarioUPS=...` |

En mantenimientos, enviar `numeroInventarioUPS` como texto en lugar de `upsId`.
Los endpoints de editar/consultar un mantenimiento conservan su Id numérico.
El frontend utiliza parámetros de Axios para codificar los inventarios en la URL.

### Verificación local

- Crear un UPS con inventario real único, editarlo y recargar la página.
- Intentar inventario o serie repetidos: debe impedir guardar.
- Revisar que los mantenimientos anteriores muestran el inventario correcto.
- Crear y editar un mantenimiento; comprobar los cambios de estado del mismo UPS.
- Consultar presupuesto y comprobar el inventario y los trabajos programados.

Pruebas automatizadas desde la raíz:

```powershell
dotnet test backend/InventarioUPS.Tests
npm --prefix frontend test
npm run build
```

Las pruebas relacionales usan SQLite en memoria y validan el contrato de la API,
las relaciones, la unicidad y la proyección. La prueba de sintaxis SQL usa el parser
T-SQL de Microsoft; no sustituye ejecutar la migración sobre SQL Server con un respaldo.

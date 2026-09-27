# Revisión de campos y catálogos (actualización más reciente)

1. Detenga la API y respalde la base de datos.
2. Descargue la rama `desarrollo` con `git pull --ff-only origin desarrollo`.
3. Si aún faltan, ejecute en este orden `20260927_CatalogoBaterias.sql` y
   `20260927_AdministracionCatalogos.sql` (sus requisitos anteriores se describen abajo).
4. Ejecute `backend/Database/20260927_RevisionCampos.sql` en la base de la aplicación.
5. Reinicie la API y el frontend. En Configuración revise **Responsables del servicio**.

El nuevo script conserva los datos. Importa los técnicos/proveedores anteriores como
opciones; si no había registros, el administrador debe agregar responsables antes de
registrar cambios. UPS requiere tipo de batería y permite modelo pendiente; los cambios
siguen requiriendo modelo. No ejecute Add-Migration/Update-Database para esta actualización.

Consulte [la clasificación completa de campos](CAMPOS_Y_CATALOGOS.md).
La migración se verificó con el analizador T-SQL; su ejecución en SQL Server requiere
la instalación local. Las pruebas de negocio utilizan SQLite.

---

# Administración de catálogos

Esta actualización agrega **Configuración**, con acceso mediante `GESTIONAR_CATALOGOS`.

## Aplicar sobre la versión con catálogo de baterías

1. Detenga la API y respalde la base de datos.
2. Descargue la rama `desarrollo`.
3. Ejecute en SSMS `backend/Database/20260927_AdministracionCatalogos.sql`.
   Requiere haber ejecutado `20260927_CatalogoBaterias.sql`. No use migraciones EF para esta actualización.
4. Reinicie API y frontend y cierre/inicie sesión para renovar los permisos.
5. Entre a **Configuración**. El permiso se asigna inicialmente a los roles
   Administrador y Control Total. Otros roles pueden recibirlo desde Usuarios y Roles.

## Reglas

- Se administran departamentos, ciudades, tipos/modelos de batería y marcas/modelos de UPS.
- Se agregan, corrigen, desactivan y reactivan opciones. No hay eliminación física.
- Los nombres son únicos por catálogo o por su clasificación superior, incluyendo opciones inactivas.
- Desactivar una opción superior impide nuevas selecciones de sus dependientes, sin borrar registros ni desactivar agencias/equipos.
- Los registros existentes pueden conservar opciones inactivas al editar otros campos.
- Un modelo de batería utilizado no puede cambiar de tipo; un modelo de UPS utilizado no puede cambiar de marca. Tampoco se traslada a otro departamento una ciudad utilizada o clasificada como local.
- Corregir nombres actualiza su presentación en los registros relacionados; para cambiar el significado se crea otra opción.
- Las tres ciudades locales se identifican mediante una clasificación persistente, no por su texto. La migración inicializa esa clasificación una sola vez. Renombrarlas no cambia las políticas; nuevas ciudades son foráneas.
- Estados operativos y tipos de agencia son de solo lectura; sus nombres funcionales permanecen protegidos.
- Roles/permisos y políticas/costos mantienen sus módulos y permisos específicos; gestionar catálogos no concede acceso adicional a esos módulos.
- Solo quienes tengan GESTIONAR_CATALOGOS pueden agregar modelos de batería desde Configuración. Las marcas y modelos de UPS se seleccionan desde el catálogo.
- El historial registra usuario, fecha UTC, acción y valores anteriores/nuevos, dentro de la misma transacción. Las ediciones sobre una versión desactualizada se rechazan.
- Al reiniciar no se recrean ciudades renombradas ni se reactivan catálogos desactivados.

El script conserva equipos y cambios, incorpora las marcas/modelos de UPS existentes,
y agrega referencias y auditoría. Si encuentra nombres preexistentes duplicados al
crear índices, revierte la operación para que se corrijan; no fusiona datos.
La clasificación inicial depende de los nombres de las tres ciudades antes de la
actualización. No modifica clasificaciones existentes al ejecutar el script de nuevo.

Pruebas: compilación C# y frontend, servicios y permisos con SQLite, validaciones de
catálogos e historial, y análisis sintáctico T-SQL. La ejecución del script en SQL
Server y la revisión visual requieren la instalación local.

---

# Actualización del 27 de septiembre: catálogo de baterías e Inicio

Si la aplicación ya tiene los cambios anteriores de `desarrollo`:

1. Detenga la API y respalde su base de datos.
2. Descargue `desarrollo` con `git pull --ff-only origin desarrollo`.
3. En SSMS, seleccione la base de Inventario UPS y ejecute completo
   `backend/Database/20260927_CatalogoBaterias.sql`.
4. Reinicie la API y el frontend. No use `Update-Database` para esta actualización.
5. Edite los UPS existentes y seleccione su tipo de batería; el modelo es opcional en la versión actual. Agregue las referencias desde Configuración; no hay página de baterías.
6. En Proyecciones, configure el costo general del año y, cuando disponga de precios
   concretos, use **Costos por modelo**. Genere nuevamente la proyección.

El script es reejecutable y no borra registros. Crea `TiposBateria`, `ModelosBateria`
y `CostosModeloBateria`, con claves foráneas desde UPS y cambios. Los datos antiguos
quedan sin modelo identificado; no se supone que todos usen la misma tecnología.
Los UPS requieren un tipo válido y permiten modelo pendiente. Los cambios requieren modelo. Cuando se selecciona un modelo, debe corresponder al tipo. Los tipos clasifican baterías; el
sistema no certifica su compatibilidad eléctrica con un UPS.

Al completar un cambio se actualiza el modelo instalado. Cancelarlo o reprogramarlo
restaura el modelo correspondiente al historial válido y a la ficha base. Cuando ya
hay cambios completados con modelo, las sustituciones se registran desde ese módulo;
no se sobrescribe el modelo desde la ficha del UPS.

Inicio consulta vencimientos sin necesitar costos. Solo muestra cambios completados
en los recientes (30 días). Mantiene alertas para vencimientos y trabajos programados
o en proceso que quedaron con fechas pasadas. Las órdenes pendientes no se cierran
automáticamente al completar otro registro; deben conciliarse desde Cambios de baterías.

Verificación realizada: compilación C# y frontend, pruebas de servicios con SQLite,
pruebas de distribución mensual/exportación y análisis sintáctico del script T-SQL.
La ejecución del script contra su SQL Server local y la revisión visual en su navegador
requieren la instalación local.

---

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

## Cambios de baterías (20260923)

Después de aplicar `20260922_MantenimientosSinCostos.sql`:

1. Detenga la API y respalde `InventarioUPSDB`.
2. Descargue `desarrollo` y ejecute `backend/Database/20260923_CambiosBaterias.sql` en SSMS.
3. Confirme el mensaje de éxito. Recompile y reinicie la API y el frontend.

El script retira `TipoMantenimientoId` y su catálogo. Conserva los registros y
relaciones con los UPS. Es transaccional y puede repetirse; dependencias adicionales
que impidan eliminar el tipo provocan una reversión, no una eliminación en cascada.

La pantalla y la API operativa muestran únicamente cambios de baterías. Los registros
antiguos sin reemplazo permanecen en la base, excluidos del módulo y las proyecciones.
`IncluyeCambioBaterias` se conserva internamente para distinguirlos, pero ya no forma
parte del formulario ni del contrato de escritura. El servidor asigna siempre `true`
y determina la cantidad según el banco completo del UPS. No se convierten pruebas
antiguas en reemplazos ficticios.

Solo un cambio Completado actualiza la última fecha de reemplazo. Programado,
En proceso y Cancelado no la adelantan. Si se cancela o reprograma uno completado,
se recupera la fecha anterior usando los demás cambios completados y la fecha base
del UPS. La proyección utiliza esa fecha y la política vigente. Editar observaciones
de un cambio completado conserva la cantidad registrada, aunque el UPS haya cambiado.

Los códigos de permisos y las rutas `/api/Mantenimiento` y `/mantenimientos` conservan
su identidad para mantener los accesos; sus nombres visibles son Cambios de baterías.
No ejecute migraciones EF ni scripts históricos para recrear las columnas retiradas.

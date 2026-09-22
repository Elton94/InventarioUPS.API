# Actualización de lógica de negocio

Esta versión usa la misma tecnología del proyecto: ASP.NET Core, Entity Framework Core, SQL Server, React, Vite y MUI.

## Importante: no usar migraciones para esta actualización

Antes de ejecutar por primera vez la API de la rama `desarrollo`, abrir SQL Server Management Studio y ejecutar:

`backend/Database/20260919_LogicaNegocio.sql`

El script agrega únicamente los campos que necesita la lógica nueva para:

- conservar la agencia histórica de un mantenimiento;
- recordar el estado anterior del UPS durante un mantenimiento;
- recalcular correctamente la fecha del último cambio de baterías.

El script está preparado para ejecutarse una vez sobre `InventarioUPSDB`.

## Número de inventario de UPS

Después del script anterior, ejecutar también:

`backend/Database/20260921_NumeroInventarioUPS.sql`

Este script agrega `NumeroInventario` a la tabla `UPSs`, lo deja obligatorio y único, y asigna temporalmente `INV-PEND-{Id}` a los UPS existentes para que luego se reemplace desde la aplicación por el número de inventario real.

## Reglas principales agregadas

- El tipo de agencia se determina por ciudad: San Pedro Sula, La Ceiba y Distrito Central son Locales; las demás son Foráneas.
- Al desactivar una agencia, sus UPS que no estén De baja pasan a En resguardo.
- De baja es un estado terminal.
- Fuera de servicio sí puede recibir mantenimiento.
- Un mantenimiento En proceso coloca el UPS En mantenimiento.
- Completado finaliza el trabajo y solamente entonces un cambio de baterías modifica la fecha real del UPS.
- Cancelado restaura el estado anterior del UPS cuando corresponde.
- Un mantenimiento histórico no puede cambiarse a otro UPS.
- La agencia del mantenimiento queda guardada como dato histórico.
- El reemplazo de baterías siempre usa el banco completo del UPS.
- Presupuesto usa política global Local/Foránea y costo unitario anual.
- La proyección calcula ciclos sucesivos, vencidos, programados e información insuficiente.
- Usuarios, roles y permisos se validan también en backend.
- Control Total sigue siendo el rol reservado con acceso completo.
- Los demás roles continúan siendo configurables desde la aplicación.

## Orden recomendado para probar

1. Ejecutar el script SQL.
2. Iniciar la API.
3. Iniciar el frontend.
4. Probar inicio de sesión y permisos.
5. Probar Agencias.
6. Probar UPS.
7. Probar Mantenimiento.
8. Probar Presupuesto.
9. Probar Usuarios, Roles y Permisos.

No pasar esta rama a `master` hasta terminar estas pruebas.

## Código, inventario y serie únicos

Después de los scripts anteriores, ejecutar en SSMS:
`backend/Database/20260922_IdentificadoresUnicosUPS.sql`.

Cada campo es único por separado en toda la tabla, incluidos UPS De baja o
En resguardo. Se ignoran mayúsculas/minúsculas y espacios exteriores;
los espacios interiores y acentos se conservan.

El script primero muestra Id, campo y valor de los duplicados, vacíos o
valores demasiado largos. Si los encuentra, se detiene y revierte toda la
operación. Corrija los datos con los identificadores reales, preferiblemente
editando el UPS desde la aplicación, y vuelva a ejecutarlo. Si un UPS tiene
varios campos duplicados, corrija todos en la misma edición.
No elimine equipos ni modifique números al azar para superar la validación.

Al finalizar correctamente, debe mostrar tres índices con EsUnico = 1 y
Deshabilitado = 0. Las columnas calculadas auxiliares son exclusivas de SQL
Server: no sustituyen los campos originales ni cambian Id o las relaciones
con mantenimientos. Los índices protegen también escrituras concurrentes.
No basta con descargar el código: esta protección requiere ejecutar el script.

Pruebas:
1. Crear un UPS con código existente y otros identificadores distintos: rechazar.
2. Repetir para inventario y serie por separado: rechazar.
3. Repetir con mayúsculas diferentes o espacios exteriores: rechazar.
4. Editar un UPS usando un identificador de otro equipo: rechazar.
5. Guardar el mismo UPS conservando sus identificadores únicos: permitir.
6. Crear con tres identificadores nuevos: permitir.

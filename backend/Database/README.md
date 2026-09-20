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

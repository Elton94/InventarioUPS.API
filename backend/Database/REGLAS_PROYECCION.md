# Reglas de operación y proyección

## Presupuesto

- Se proyecta desde el último cambio completado, o desde la instalación cuando no hay cambios.
- Cada fecha se calcula desde el día base más N ciclos; febrero no desplaza permanentemente el día del mes.
- Años futuros: se suman todas las fechas de enero a diciembre con el costo activo de ese año. No se hereda un precio de otro año ni se aplica inflación automática.
- Año actual: se estima lo pendiente desde hoy hasta diciembre. Los cambios ya completados no son gastos pendientes.
- Si vencieron uno o varios ciclos sin reemplazo, se añade un solo banco pendiente en el año actual. Las fechas futuras mantienen el calendario teórico hasta registrar el cambio real. Si un ciclo vence hoy y ya había un pendiente, no se duplica.
- Los años futuros no suman nuevamente el vencido. Se avisa que la planificación depende de atenderlo. No se crean órdenes ni registros de cambios por generar un presupuesto.
- Un trabajo Programado es una referencia operativa; no añade un segundo costo ni modifica la fecha real del último reemplazo.
- UPS, cambios y baterías se cuentan por separado: 1 UPS con 4 baterías y 4 cambios requiere 16 baterías.
- La consulta de años anteriores se rechaza: no hay un historial de presupuestos congelados ni de gastos reales.
- Se excluyen agencias inactivas y UPS de baja o fuera de servicio. Las fechas o cantidades inválidas se muestran como datos por corregir.
- Los precios deben ser positivos y tener máximo dos decimales. Se admite configurar 2029 y otros años futuros dentro del rango 2020–2100. La interfaz propone los próximos cinco años y otros años configurados.
- La política es global para todos los años. Modificarla recalcula las estimaciones; no modifica hechos históricos.

## Operación

- Solo Completado modifica la fecha del último cambio. Cancelar o reprogramar un completado restaura la fecha resultante de los otros cambios y la fecha base.
- Un reemplazo no prueba que el UPS esté reparado: al salir de En proceso recupera su estado anterior; no convierte automáticamente Fuera de servicio en Operativo.
- En mantenimiento se administra desde Cambios de baterías. Durante un cambio En proceso no se permite trasladar el UPS, alterar su banco ni forzar otro estado.
- El cambio no puede ser anterior a la instalación. Tampoco se puede mover la instalación a una fecha posterior a cambios ya registrados.
- No se permiten dos cambios En proceso en la validación operativa; debe finalizarse el existente antes de iniciar o completar otro.

## Aplicación local

Esta revisión no cambia columnas ni tablas. Si ya se aplicaron los scripts de las
versiones anteriores, basta descargar `desarrollo`, recompilar/reiniciar la API y
reiniciar el frontend. Las pruebas usan SQLite y no sustituyen la verificación
con SQL Server local. Crear y editar cambios utiliza una transacción serializable
para que la validación y el guardado sean atómicos. Un conflicto de bloqueo de SQL
Server devuelve un mensaje para recargar y reintentar. No se ha realizado una
prueba de carga concurrente en SQL Server desde este entorno.

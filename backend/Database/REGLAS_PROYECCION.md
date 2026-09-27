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

## Precisión y compras por modelo

- Cada modelo pertenece a un tipo; los registros antiguos sin modelo siguen visibles como Sin identificar.
- El precio por modelo y año reemplaza al costo general del mismo año. Si falta, se utiliza la referencia general y se advierte en pantalla. No se heredan precios de otros años.
- Se requiere una configuración general activa para generar el presupuesto; garantiza una referencia explícita para equipos sin precio específico.
- La distribución mensual reconcilia las cantidades e importes del detalle. El banco vencido se asigna al mes actual como atención pendiente, sin cambiar su fecha histórica de vencimiento.
- El resumen de compras agrupa unidades por modelo y tipo. Los filtros afectan el resumen, los meses y la exportación CSV.
- Los cambios programados no sustituyen el modelo instalado ni su costo proyectado: el presupuesto parte de la ficha vigente hasta completar el trabajo.
- No se presume inflación, expansión de inventario ni gastos de transporte o mano de obra. No es una predicción estadística ni un presupuesto aprobado e inmutable.

## Catálogos y clasificación

La condición Local/Foránea se obtiene de la clasificación persistente de la ciudad.
Editar su nombre no cambia el calendario. Las opciones inactivas se conservan en los
registros existentes y en las proyecciones; su desactivación impide nuevas selecciones,
pero no retira equipos del inventario ni elimina necesidades de reemplazo.

## Operación

- Solo Completado modifica la fecha del último cambio. Cancelar o reprogramar un completado restaura la fecha resultante de los otros cambios y la fecha base.
- Un reemplazo no prueba que el UPS esté reparado: al salir de En proceso recupera su estado anterior; no convierte automáticamente Fuera de servicio en Operativo.
- En mantenimiento se administra desde Cambios de baterías. Durante un cambio En proceso no se permite trasladar el UPS, alterar su banco ni forzar otro estado.
- El cambio no puede ser anterior a la instalación. Tampoco se puede mover la instalación a una fecha posterior a cambios ya registrados.
- No se permiten dos cambios En proceso en la validación operativa; debe finalizarse el existente antes de iniciar o completar otro.

## Aplicación local

Esta revisión incorpora el catálogo de baterías. Aplique primero
`20260927_CatalogoBaterias.sql` y `20260927_AdministracionCatalogos.sql`, siguiendo el README de esta carpeta, y reinicie
la API y el frontend. Las pruebas usan SQLite y no sustituyen la verificación
con SQL Server local. Crear y editar cambios utiliza una transacción serializable
para que la validación y el guardado sean atómicos. Un conflicto de bloqueo de SQL
Server devuelve un mensaje para recargar y reintentar. No se ha realizado una
prueba de carga concurrente en SQL Server desde este entorno.

# Campos y catálogos

Criterio: un catálogo contiene opciones reutilizables con identidad estable. Un dato
propio de un equipo, una persona o un evento se captura y valida; no se convierte en
catálogo solamente porque se pueda mostrar en una lista.

| Módulo | Campos | Tratamiento |
|---|---|---|
| Agencias | Departamento y ciudad | Catálogos editables; ciudad depende de departamento. Solo CiudadId se guarda en la agencia; el departamento se obtiene por la relación. |
| Agencias | Tipo Local/Foránea | Catálogo protegido; se determina por la clasificación de la ciudad. No se elige manualmente. |
| Agencias | Código, nombre, dirección | Datos del registro. Código único. Activa es un booleano, no otro catálogo. |
| UPS | Marca y modelo del UPS | Catálogos relacionados; el modelo pertenece a una marca. Las cadenas existentes se conservan por compatibilidad y se sincronizan con la referencia normalizada. |
| UPS | Agencia | Relación con una agencia registrada; no duplicar agencias en un catálogo adicional. |
| UPS | Estado | Lista protegida por las reglas operativas. En mantenimiento se administra desde los cambios. |
| UPS | Tipo de batería | Catálogo obligatorio en altas y ediciones. No se inventa un tipo para los registros antiguos sin información. |
| UPS | Modelo/referencia de batería | Catálogo opcional. Si se selecciona, debe corresponder al tipo. No certifica compatibilidad eléctrica o física. |
| UPS | Inventario y serie | Identificadores únicos de texto, no catálogos. Conservan ceros iniciales. |
| UPS | Capacidad VA, cantidad de baterías | Números enteros positivos, no catálogos. |
| UPS | Instalación y último cambio | Fechas, no catálogos. Los cambios completados gobiernan la fecha y batería instaladas. Las referencias base son internas y permiten restaurar el historial al cancelar. |
| Cambios | UPS | Relación por Id interno; el usuario busca por inventario, serie o agencia. |
| Cambios | Estado | Catálogo protegido: Programado, En proceso, Completado, Cancelado. |
| Cambios | Tipo y modelo de batería | Tipo filtra el catálogo de modelos; el modelo es obligatorio y determina el tipo. Se conserva qué repuesto se registró. |
| Cambios | Responsable del servicio | Nuevo catálogo de técnicos internos y proveedores externos. No requiere cuenta de usuario. Se guarda su Id y una copia histórica del nombre. |
| Cambios | Fecha, detalle del trabajo, observaciones | Fecha validada; textos libres opcionales de hasta 1000 caracteres. No catálogo de descripciones. |
| Cambios | Cantidad reemplazada | Se obtiene del banco del UPS; al editar un completado se conserva la cantidad histórica. |
| Usuarios | Rol | Relación con roles administrados desde Usuarios y roles. Permisos protegidos por su función. |
| Usuarios | Nombre completo, usuario, correo, contraseña | Datos propios y credenciales; no catálogos. Activo es booleano. |
| Proyecciones | Año | Número validado y opciones de interfaz; no tabla de años. |
| Proyecciones | Meses de política, costos general y por modelo/año | Valores numéricos de configuración, no catálogos de meses o precios. |
| Proyecciones | Fechas previstas, cambios, baterías y totales | Resultados calculados; no se capturan ni se almacenan como opciones de catálogo. |

## Cambios aplicados

- Nuevo catálogo Responsables del servicio en Configuración, con permisos, unicidad,
  activación/desactivación y auditoría. Los nombres históricos se conservan al
  renombrar o desactivar un responsable. Las altas rechazan nombres libres e IDs
  inválidos; una edición puede conservar su responsable inactivo original.
- UPS admite un tipo conocido con modelo pendiente. Agregados TipoBateriaId y
  TipoBateriaBaseId con claves foráneas; el modelo continúa siendo opcional en la
  base y ahora también en formulario/API. Modelo y tipo no pueden contradecirse.
- La ficha se actualiza al completar el reemplazo; cancelar recupera el tipo/modelo
  anterior, incluido un tipo cuyo modelo todavía se desconoce.
- Se retiró la entrada libre y creación de modelos desde el selector operativo.
  Las opciones se administran en Configuración.
- Descripción pasa a Detalle del trabajo (opcional); se conserva su columna y contenido.
- Sin modelo o precio específico, la proyección utiliza la referencia general del
  año y lo señala. Las compras sin modelo se agrupan por tipo, sin mezclar tecnologías.
  Ese agrupamiento no constituye una especificación suficiente para comprar repuestos.
- No se eliminaron datos ni se cambiaron las políticas de reemplazo existentes.
  Inspecciones sin reemplazo siguen fuera del alcance operativo de este módulo.

## Actualización

Con la API detenida, ejecutar `20260927_RevisionCampos.sql` después de
`20260927_AdministracionCatalogos.sql`. El script crea referencias, conserva datos
anteriores, importa los nombres de responsables existentes y no los vuelve a
recrear al ejecutarlo nuevamente. No crea cuentas de usuario para esos responsables.
Los registros antiguos sin tipo/modelo conocido requieren identificación al editarlos.

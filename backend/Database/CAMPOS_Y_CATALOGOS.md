# Campos y catálogos vigentes

| Módulo | Campos | Tratamiento |
|---|---|---|
| Agencias | Departamento, ciudad | Catálogos; la ciudad determina el departamento. |
| Agencias | Local/Foránea | Clasificación automática y protegida según ciudad. |
| Agencias | Código, nombre, dirección | Datos propios; código único. |
| UPS | Marca, modelo del UPS | Catálogos relacionados. |
| UPS | Agencia | Relación con las agencias registradas. |
| UPS | Estado | Lista protegida; En mantenimiento se controla desde los cambios. |
| UPS y cambios | Tipo de batería | Catálogo administrable; no existe modelo de batería operativo. |
| UPS y cambios | Voltaje (V), capacidad (Ah) | Números positivos de hasta tres decimales, por batería individual; no son catálogos. |
| UPS | Inventario, serie | Identificadores únicos de texto. |
| UPS | Capacidad VA, cantidad de baterías | Enteros positivos. VA del UPS y Ah de la batería son conceptos diferentes. |
| UPS | Instalación, último cambio | Fechas; los cambios completados gobiernan el último reemplazo y las especificaciones instaladas. |
| Cambios | UPS, estado | Relación con inventario y lista protegida de estados. |
| Cambios | Usuario responsable | Relación con las cuentas creadas; no catálogo independiente de nombres/proveedores. |
| Cambios | Observaciones | Texto libre opcional, hasta 1000 caracteres. Se retiró Detalle del trabajo del formulario y contrato de entrada. |
| Cambios | Cantidad reemplazada | Banco completo del UPS; se conserva la cantidad histórica al editar completados. |
| Usuarios | Rol | Catálogo de roles con permisos; gestión en Usuarios y roles. |
| Usuarios | Nombre, usuario, correo, contraseña | Datos y credenciales, no catálogos. |
| Proyección | Año, referencia, costos por tipo, meses | Configuración numérica. Costos por tipo/año, sin heredar otros años. |
| Proyección | Método de cálculo | Selección protegida: referencia o tipo. |
| Proyección | Fechas y totales | Resultados calculados. |

## Responsables

El técnico queda asignado al usuario autenticado; la API rechaza suplantación y
edición de cambios de otro usuario. Administrador y Control Total pueden elegir
usuarios habilitados. El nombre histórico se conserva aunque la cuenta cambie de
nombre o se desactive. No se asignan usuarios inactivos a cambios nuevos.

Configuración → Usuarios / técnicos reutiliza la gestión de usuarios y su modal
completo. Crear técnico en un cambio abre el mismo modal con el rol Técnico
preseleccionado. Crear cuentas conserva el permiso GESTIONAR_USUARIOS.

## Especificaciones y presupuesto

Completar un cambio actualiza tipo, voltaje y capacidad del UPS. Cancelar recupera
la batería anterior. Los datos base son internos y no se editan desde el formulario.

El modo referencia usa el mismo precio anual para todas las baterías. El modo tipo
usa los precios personalizados y, donde falten, la referencia del año. En la tabla,
restablecer referencia vuelve a vincular ese tipo al valor anual.

Las compras se separan por tipo, V y Ah aunque el costo se configure por tipo.
La aplicación no certifica compatibilidad física o eléctrica con el UPS.

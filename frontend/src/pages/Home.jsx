import {
    useEffect,
    useMemo,
    useState
} from "react";

import {
    Alert,
    Box,
    Card,
    CardContent,
    CircularProgress,
    Grid,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography
} from "@mui/material";

import {
    obtenerAgencias
} from "../services/agenciaService";
import {
    obtenerUPS
} from "../services/upsService";
import {
    obtenerMantenimientos
} from "../services/mantenimientoService";
import {
    obtenerProyeccionPresupuesto
} from "../services/presupuestoService";
import {
    tienePermiso
} from "../services/permisos";

const moneda = new Intl.NumberFormat(
    "es-HN",
    {
        style: "currency",
        currency: "HNL"
    }
);

const numero =
    new Intl.NumberFormat("es-HN");

const fecha = (valor) =>
    valor
        ? new Date(
            valor
        ).toLocaleDateString("es-HN")
        : "—";

export default function Home() {
    const [agencias, setAgencias] =
        useState([]);
    const [ups, setUps] =
        useState([]);
    const [
        mantenimientos,
        setMantenimientos
    ] = useState([]);
    const [
        detalleProyeccion,
        setDetalleProyeccion
    ] = useState([]);
    const [cargando, setCargando] =
        useState(true);
    const [error, setError] =
        useState("");

    const puedeAgencias =
        tienePermiso("VER_AGENCIAS");
    const puedeUPS =
        tienePermiso("VER_UPS");
    const puedeMantenimiento =
        tienePermiso(
            "VER_MANTENIMIENTO"
        );
    const puedePresupuesto =
        tienePermiso(
            "VER_PRESUPUESTO"
        );

    const anio =
        new Date().getFullYear();

    useEffect(() => {
        const cargar = async () => {
            setCargando(true);
            setError("");

            const tareas = [];

            if (puedeAgencias) {
                tareas.push(
                    obtenerAgencias()
                        .then((datos) =>
                            setAgencias(
                                datos || []
                            )
                        )
                );
            }

            if (puedeUPS) {
                tareas.push(
                    obtenerUPS()
                        .then((datos) =>
                            setUps(
                                datos || []
                            )
                        )
                );
            }

            if (puedeMantenimiento) {
                tareas.push(
                    obtenerMantenimientos()
                        .then((datos) =>
                            setMantenimientos(
                                datos || []
                            )
                        )
                );
            }

            if (puedePresupuesto) {
                tareas.push(
                    obtenerProyeccionPresupuesto(
                        anio
                    )
                        .then((datos) => {
                            const detalle =
                                Array.isArray(
                                    datos?.agencias
                                )
                                    ? datos.agencias
                                        .flatMap(
                                            (a) =>
                                                Array.isArray(
                                                    a.ups
                                                )
                                                    ? a.ups
                                                    : []
                                        )
                                    : [];

                            setDetalleProyeccion(
                                detalle
                            );
                        })
                        .catch((e) => {
                            if (
                                e.response
                                    ?.status !==
                                404
                            ) {
                                throw e;
                            }
                            setDetalleProyeccion(
                                []
                            );
                        })
                );
            }

            const resultados =
                await Promise.allSettled(
                    tareas
                );

            if (
                resultados.some(
                    (r) =>
                        r.status ===
                        "rejected"
                )
            ) {
                setError(
                    "Parte de la información del inicio no pudo cargarse."
                );
            }

            setCargando(false);
        };

        cargar();
    }, []);

    const agenciasActivas =
        agencias.filter(
            (a) => a.activa
        ).length;

    const baterias =
        ups.reduce(
            (total, equipo) =>
                total +
                Number(
                    equipo.cantidadBaterias ||
                    0
                ),
            0
        );

    const fueraServicio =
        ups.filter(
            (equipo) =>
                [
                    "fuera de servicio",
                    "de baja"
                ].includes(
                    String(
                        equipo.estado || ""
                    )
                        .toLowerCase()
                )
        ).length;

    const costoMantenimiento =
        mantenimientos.reduce(
            (total, m) =>
                total +
                Number(m.costo || 0) +
                (
                    m.incluyeCambioBaterias
                        ? Number(
                            m.costoBaterias ||
                            0
                        )
                        : 0
                ),
            0
        );

    const presupuesto =
        detalleProyeccion.reduce(
            (total, item) =>
                total +
                Number(
                    item.costoEstimado ||
                    0
                ),
            0
        );

    const bateriasProyectadas =
        detalleProyeccion.reduce(
            (total, item) =>
                total +
                Number(
                    item.cantidadBaterias ||
                    0
                ),
            0
        );

    const recientes = useMemo(
        () =>
            [...mantenimientos]
                .sort(
                    (a, b) =>
                        new Date(
                            b.fechaMantenimiento
                        ) -
                        new Date(
                            a.fechaMantenimiento
                        )
                )
                .slice(0, 5),
        [mantenimientos]
    );

    if (cargando) {
        return (
            <Box
                sx={{
                    display: "flex",
                    justifyContent: "center",
                    py: 10
                }}
            >
                <CircularProgress />
            </Box>
        );
    }

    const tarjetas = [
        puedeAgencias && {
            titulo: "Agencias activas",
            valor:
                numero.format(
                    agenciasActivas
                )
        },
        puedeUPS && {
            titulo: "UPS registrados",
            valor:
                numero.format(ups.length)
        },
        puedeUPS && {
            titulo:
                "Fuera de servicio / baja",
            valor:
                numero.format(
                    fueraServicio
                )
        },
        puedeUPS && {
            titulo:
                "Baterías instaladas",
            valor:
                numero.format(baterias)
        },
        puedeMantenimiento && {
            titulo: "Mantenimientos",
            valor:
                numero.format(
                    mantenimientos.length
                )
        },
        puedeMantenimiento && {
            titulo:
                "Costo de mantenimiento",
            valor:
                moneda.format(
                    costoMantenimiento
                )
        },
        puedePresupuesto && {
            titulo:
                `Baterías proyectadas ${anio}`,
            valor:
                numero.format(
                    bateriasProyectadas
                )
        },
        puedePresupuesto && {
            titulo:
                `Presupuesto ${anio}`,
            valor:
                moneda.format(
                    presupuesto
                )
        }
    ].filter(Boolean);

    return (
        <Box>
            <Typography
                variant="h4"
                fontWeight="bold"
            >
                Inicio
            </Typography>
            <Typography
                color="text.secondary"
                sx={{ mb: 3 }}
            >
                Resumen de los módulos a los que tiene acceso.
            </Typography>

            {error && (
                <Alert
                    severity="warning"
                    sx={{ mb: 3 }}
                >
                    {error}
                </Alert>
            )}

            <Grid
                container
                spacing={2}
                sx={{ mb: 4 }}
            >
                {tarjetas.map(
                    (tarjeta) => (
                        <Grid
                            key={
                                tarjeta.titulo
                            }
                            size={{
                                xs: 12,
                                sm: 6,
                                md: 3
                            }}
                        >
                            <Card>
                                <CardContent>
                                    <Typography
                                        color="text.secondary"
                                        variant="body2"
                                    >
                                        {
                                            tarjeta.titulo
                                        }
                                    </Typography>
                                    <Typography
                                        variant="h5"
                                        fontWeight="bold"
                                        sx={{ mt: 1 }}
                                    >
                                        {
                                            tarjeta.valor
                                        }
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Grid>
                    )
                )}
            </Grid>

            {puedeMantenimiento && (
                <Card sx={{ mb: 3 }}>
                    <CardContent>
                        <Typography
                            variant="h6"
                            fontWeight="bold"
                            sx={{ mb: 2 }}
                        >
                            Mantenimientos recientes
                        </Typography>

                        <TableContainer>
                            <Table size="small">
                                <TableHead>
                                    <TableRow>
                                        <TableCell>
                                            Fecha
                                        </TableCell>
                                        <TableCell>
                                            UPS
                                        </TableCell>
                                        <TableCell>
                                            Agencia histórica
                                        </TableCell>
                                        <TableCell>
                                            Tipo
                                        </TableCell>
                                        <TableCell>
                                            Estado
                                        </TableCell>
                                        <TableCell
                                            align="right"
                                        >
                                            Total
                                        </TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {recientes.map(
                                        (m) => (
                                            <TableRow
                                                key={
                                                    m.id
                                                }
                                            >
                                                <TableCell>
                                                    {
                                                        fecha(
                                                            m.fechaMantenimiento
                                                        )
                                                    }
                                                </TableCell>
                                                <TableCell>
                                                    <strong>
                                                        {m.numeroInventarioUPS || "—"}
                                                    </strong>
                                                </TableCell>
                                                <TableCell>
                                                    {
                                                        m.agenciaNombre
                                                    }
                                                </TableCell>
                                                <TableCell>
                                                    {
                                                        m.tipoMantenimiento
                                                    }
                                                </TableCell>
                                                <TableCell>
                                                    {
                                                        m.estado
                                                    }
                                                </TableCell>
                                                <TableCell
                                                    align="right"
                                                >
                                                    {
                                                        moneda.format(
                                                            Number(
                                                                m.costo ||
                                                                0
                                                            ) +
                                                            (
                                                                m.incluyeCambioBaterias
                                                                    ? Number(
                                                                        m.costoBaterias ||
                                                                        0
                                                                    )
                                                                    : 0
                                                            )
                                                        )
                                                    }
                                                </TableCell>
                                            </TableRow>
                                        )
                                    )}

                                    {recientes.length ===
                                        0 && (
                                        <TableRow>
                                            <TableCell
                                                colSpan={6}
                                                align="center"
                                            >
                                                No hay mantenimientos registrados.
                                            </TableCell>
                                        </TableRow>
                                    )}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </CardContent>
                </Card>
            )}

            {puedePresupuesto && (
                <Card>
                    <CardContent>
                        <Typography
                            variant="h6"
                            fontWeight="bold"
                            sx={{ mb: 2 }}
                        >
                            Reemplazos de baterías {anio}
                        </Typography>

                        <TableContainer>
                            <Table size="small">
                                <TableHead>
                                    <TableRow>
                                        <TableCell>
                                            Agencia
                                        </TableCell>
                                        <TableCell>
                                            UPS
                                        </TableCell>
                                        <TableCell>
                                            Fecha
                                        </TableCell>
                                        <TableCell>
                                            Estado de proyección
                                        </TableCell>
                                        <TableCell
                                            align="right"
                                        >
                                            Costo
                                        </TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {detalleProyeccion
                                        .slice(0, 8)
                                        .map(
                                            (item) => (
                                                <TableRow
                                                    key={
                                                        item.numeroInventario
                                                    }
                                                >
                                                    <TableCell>
                                                        {
                                                            item.agencia
                                                        }
                                                    </TableCell>
                                                    <TableCell>
                                                        <strong>
                                                            {item.numeroInventarioUPS || "—"}
                                                        </strong>
                                                    </TableCell>
                                                    <TableCell>
                                                        {
                                                            fecha(
                                                                item.fechaProyectadaReemplazo
                                                            )
                                                        }
                                                    </TableCell>
                                                    <TableCell>
                                                        {
                                                            item.estaVencido
                                                                ? "Vencido"
                                                                : item.tieneMantenimientoProgramado
                                                                    ? "Programado"
                                                                    : "Proyectado"
                                                        }
                                                    </TableCell>
                                                    <TableCell
                                                        align="right"
                                                    >
                                                        {
                                                            moneda.format(
                                                                item.costoEstimado ||
                                                                0
                                                            )
                                                        }
                                                    </TableCell>
                                                </TableRow>
                                            )
                                        )}

                                    {detalleProyeccion.length ===
                                        0 && (
                                        <TableRow>
                                            <TableCell
                                                colSpan={5}
                                                align="center"
                                            >
                                                No hay reemplazos proyectados para este año o falta configurar el costo anual.
                                            </TableCell>
                                        </TableRow>
                                    )}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </CardContent>
                </Card>
            )}
        </Box>
    );
}

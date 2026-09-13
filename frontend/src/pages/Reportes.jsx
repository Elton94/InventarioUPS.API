import { useEffect, useMemo, useState } from "react";

import {
    Alert,
    Box,
    Card,
    CardContent,
    CircularProgress,
    Divider,
    Grid,
    MenuItem,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
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


export default function Reportes() {

    const anioActual =
        new Date().getFullYear();

    const [agencias, setAgencias] =
        useState([]);

    const [ups, setUps] =
        useState([]);

    const [mantenimientos, setMantenimientos] =
        useState([]);

    const [proyeccion, setProyeccion] =
        useState(null);

    const [anio, setAnio] =
        useState(anioActual);

    const [cargando, setCargando] =
        useState(true);

    const [cargandoPresupuesto, setCargandoPresupuesto] =
        useState(false);

    const [error, setError] =
        useState("");

    const [errorPresupuesto, setErrorPresupuesto] =
        useState("");


    // =====================================================
    // CARGAR INFORMACIÓN GENERAL
    // =====================================================

    useEffect(() => {

        cargarReportes();

    }, []);


    // =====================================================
    // CARGAR PRESUPUESTO
    // =====================================================

    useEffect(() => {

        cargarPresupuesto(anio);

    }, [anio]);


    const cargarReportes = async () => {

        try {

            setCargando(true);
            setError("");

            const [
                agenciasData,
                upsData,
                mantenimientosData
            ] = await Promise.all([

                obtenerAgencias(),

                obtenerUPS(),

                obtenerMantenimientos()

            ]);


            setAgencias(
                agenciasData || []
            );

            setUps(
                upsData || []
            );

            setMantenimientos(
                mantenimientosData || []
            );

        } catch (error) {

            console.error(
                "Error al cargar reportes:",
                error
            );

            setError(
                "No se pudieron cargar los datos de reportes."
            );

        } finally {

            setCargando(false);

        }

    };


    const cargarPresupuesto = async (
        anioSeleccionado
    ) => {

        try {

            setCargandoPresupuesto(true);
            setErrorPresupuesto("");

            const resultado =
                await obtenerProyeccionPresupuesto(
                    anioSeleccionado
                );

            setProyeccion(
                resultado
            );

        } catch (error) {

            console.error(
                "Error al cargar presupuesto:",
                error
            );

            setProyeccion(null);

            const mensaje =
                error.response?.data
                    ?.mensaje ||
                error.response?.data
                    ?.message ||
                "No se pudo cargar la proyección de presupuesto.";

            setErrorPresupuesto(
                mensaje
            );

        } finally {

            setCargandoPresupuesto(false);

        }

    };


    // =====================================================
    // UTILIDADES
    // =====================================================

    const obtenerNombreAgencia = (
        agencia
    ) => {

        return (
            agencia?.nombre ||
            agencia?.nombreAgencia ||
            "Sin agencia"
        );

    };


    const obtenerNombreUPS = (
        equipo
    ) => {

        const codigo =
            equipo?.codigo || "";

        const serie =
            equipo?.numeroSerie || "";

        const marca =
            equipo?.marca || "";

        const modelo =
            equipo?.modelo || "";

        return [
            codigo,
            serie,
            `${marca} ${modelo}`.trim()
        ]
            .filter(Boolean)
            .join(" - ");

    };


    const obtenerNombreEstadoUPS = (
        equipo
    ) => {

        return (
            equipo?.estadoUPS?.nombre ||
            equipo?.estadoUPSNombre ||
            "Sin estado"
        );

    };


    const obtenerNombreTipoAgencia = (
        agencia
    ) => {

        return (
            agencia?.tipoAgencia?.nombre ||
            agencia?.tipoAgenciaNombre ||
            "Sin tipo"
        );

    };


    const obtenerNombreCiudad = (
        agencia
    ) => {

        return (
            agencia?.ciudad?.nombre ||
            agencia?.ciudadNombre ||
            "Sin ciudad"
        );

    };


    const obtenerNombreTecnico = (
        mantenimiento
    ) => {

        return (
            mantenimiento?.tecnicoProveedor ||
            "Sin técnico/proveedor"
        );

    };


    const obtenerNombreTipoMantenimiento = (
        mantenimiento
    ) => {

        return (
            mantenimiento?.tipoMantenimiento?.nombre ||
            mantenimiento?.tipoMantenimientoNombre ||
            "Sin tipo"
        );

    };


    const formatearMoneda = (
        valor
    ) => {

        const numero =
            Number(valor || 0);

        return numero.toLocaleString(
            "es-HN",
            {
                style: "currency",
                currency: "HNL",
                minimumFractionDigits: 2
            }
        );

    };


    const formatearFecha = (
        fecha
    ) => {

        if (!fecha) {
            return "-";
        }

        const texto =
            String(fecha)
                .substring(0, 10);

        const partes =
            texto.split("-");

        if (partes.length !== 3) {
            return texto;
        }

        return `${partes[2]}/${partes[1]}/${partes[0]}`;

    };


    // =====================================================
    // RESUMEN
    // =====================================================

    const totalAgencias =
        agencias.length;


    const agenciasActivas =
        agencias.filter(
            agencia =>
                agencia.activa !== false
        ).length;


    const totalUPS =
        ups.length;


    const totalBaterias =
        ups.reduce(
            (total, equipo) =>
                total +
                Number(
                    equipo.cantidadBaterias || 0
                ),
            0
        );


    const totalMantenimientos =
        mantenimientos.length;


    const costoMantenimientos =
        mantenimientos.reduce(
            (total, mantenimiento) =>
                total +
                Number(
                    mantenimiento.costo || 0
                ),
            0
        );


    // =====================================================
    // UPS POR ESTADO
    // =====================================================

    const upsPorEstado =
        useMemo(() => {

            const resultado = {};

            ups.forEach(
                equipo => {

                    const estado =
                        obtenerNombreEstadoUPS(
                            equipo
                        );

                    resultado[estado] =
                        (resultado[estado] || 0) +
                        1;

                }
            );

            return Object.entries(
                resultado
            ).sort(
                (a, b) =>
                    b[1] - a[1]
            );

        }, [ups]);


    // =====================================================
    // INVENTARIO POR AGENCIA
    // =====================================================

    const inventarioPorAgencia =
        useMemo(() => {

            return agencias
                .map(agencia => {

                    const agenciaId =
                        Number(
                            agencia.id
                        );

                    const equipos =
                        ups.filter(
                            equipo =>
                                Number(
                                    equipo.agenciaId
                                ) === agenciaId
                        );

                    const baterias =
                        equipos.reduce(
                            (total, equipo) =>
                                total +
                                Number(
                                    equipo.cantidadBaterias || 0
                                ),
                            0
                        );

                    return {

                        id:
                            agencia.id,

                        nombre:
                            obtenerNombreAgencia(
                                agencia
                            ),

                        ciudad:
                            obtenerNombreCiudad(
                                agencia
                            ),

                        tipo:
                            obtenerNombreTipoAgencia(
                                agencia
                            ),

                        ups:
                            equipos.length,

                        baterias

                    };

                })
                .sort(
                    (a, b) =>
                        b.ups - a.ups
                );

        }, [agencias, ups]);


    // =====================================================
    // MANTENIMIENTOS POR TIPO
    // =====================================================

    const mantenimientosPorTipo =
        useMemo(() => {

            const resultado = {};

            mantenimientos.forEach(
                mantenimiento => {

                    const tipo =
                        obtenerNombreTipoMantenimiento(
                            mantenimiento
                        );

                    resultado[tipo] =
                        (resultado[tipo] || 0) +
                        1;

                }
            );

            return Object.entries(
                resultado
            ).sort(
                (a, b) =>
                    b[1] - a[1]
            );

        }, [mantenimientos]);


    // =====================================================
    // ÚLTIMOS MANTENIMIENTOS
    // =====================================================

    const ultimosMantenimientos =
        useMemo(() => {

            return [
                ...mantenimientos
            ]
                .sort(
                    (a, b) =>
                        new Date(
                            b.fechaMantenimiento
                        ) -
                        new Date(
                            a.fechaMantenimiento
                        )
                )
                .slice(0, 10);

        }, [mantenimientos]);


    // =====================================================
    // PRESUPUESTO
    // =====================================================

    const presupuestoTotal =
        Number(
            proyeccion?.totalNacional ||
            proyeccion?.total ||
            proyeccion?.montoTotal ||
            0
        );


    const presupuestoUPS =
        Number(
            proyeccion?.totalUPS ||
            proyeccion?.cantidadUPS ||
            0
        );


    const presupuestoBaterias =
        Number(
            proyeccion?.totalBaterias ||
            proyeccion?.cantidadBaterias ||
            0
        );


    // =====================================================
    // CARGANDO
    // =====================================================

    if (cargando) {

        return (

            <Box
                sx={{
                    display: "flex",
                    justifyContent: "center",
                    alignItems: "center",
                    minHeight: "60vh"
                }}
            >

                <CircularProgress />

            </Box>

        );

    }


    return (

        <Box>

            {/* =================================================
                TÍTULO
            ================================================= */}

            <Box
                sx={{
                    mb: 3
                }}
            >

                <Typography
                    variant="h4"
                    fontWeight="bold"
                >
                    Reportes
                </Typography>

                <Typography
                    variant="body1"
                    color="text.secondary"
                >
                    Resumen general del inventario UPS,
                    mantenimientos y presupuesto.
                </Typography>

            </Box>


            {error && (

                <Alert
                    severity="error"
                    sx={{
                        mb: 3
                    }}
                >
                    {error}
                </Alert>

            )}


            {/* =================================================
                RESUMEN GENERAL
            ================================================= */}

            <Grid
                container
                spacing={2}
                sx={{
                    mb: 3
                }}
            >

                <Grid
                    item
                    xs={12}
                    sm={6}
                    md={2}
                >

                    <Card>

                        <CardContent>

                            <Typography
                                color="text.secondary"
                                variant="body2"
                            >
                                Agencias
                            </Typography>

                            <Typography
                                variant="h4"
                                fontWeight="bold"
                            >
                                {totalAgencias}
                            </Typography>

                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                {agenciasActivas} activas
                            </Typography>

                        </CardContent>

                    </Card>

                </Grid>


                <Grid
                    item
                    xs={12}
                    sm={6}
                    md={2}
                >

                    <Card>

                        <CardContent>

                            <Typography
                                color="text.secondary"
                                variant="body2"
                            >
                                UPS
                            </Typography>

                            <Typography
                                variant="h4"
                                fontWeight="bold"
                            >
                                {totalUPS}
                            </Typography>

                        </CardContent>

                    </Card>

                </Grid>


                <Grid
                    item
                    xs={12}
                    sm={6}
                    md={2}
                >

                    <Card>

                        <CardContent>

                            <Typography
                                color="text.secondary"
                                variant="body2"
                            >
                                Baterías
                            </Typography>

                            <Typography
                                variant="h4"
                                fontWeight="bold"
                            >
                                {totalBaterias}
                            </Typography>

                        </CardContent>

                    </Card>

                </Grid>


                <Grid
                    item
                    xs={12}
                    sm={6}
                    md={2}
                >

                    <Card>

                        <CardContent>

                            <Typography
                                color="text.secondary"
                                variant="body2"
                            >
                                Mantenimientos
                            </Typography>

                            <Typography
                                variant="h4"
                                fontWeight="bold"
                            >
                                {totalMantenimientos}
                            </Typography>

                        </CardContent>

                    </Card>

                </Grid>


                <Grid
                    item
                    xs={12}
                    sm={6}
                    md={4}
                >

                    <Card>

                        <CardContent>

                            <Typography
                                color="text.secondary"
                                variant="body2"
                            >
                                Costo de mantenimientos
                            </Typography>

                            <Typography
                                variant="h4"
                                fontWeight="bold"
                            >
                                {formatearMoneda(
                                    costoMantenimientos
                                )}
                            </Typography>

                        </CardContent>

                    </Card>

                </Grid>

            </Grid>


            {/* =================================================
                ESTADO UPS / MANTENIMIENTOS
            ================================================= */}

            <Grid
                container
                spacing={3}
                sx={{
                    mb: 3
                }}
            >

                <Grid
                    item
                    xs={12}
                    md={6}
                >

                    <Paper
                        sx={{
                            p: 2
                        }}
                    >

                        <Typography
                            variant="h6"
                            fontWeight="bold"
                            sx={{
                                mb: 2
                            }}
                        >
                            UPS por estado
                        </Typography>

                        <TableContainer>

                            <Table
                                size="small"
                            >

                                <TableHead>

                                    <TableRow>

                                        <TableCell>
                                            Estado
                                        </TableCell>

                                        <TableCell align="right">
                                            Cantidad
                                        </TableCell>

                                    </TableRow>

                                </TableHead>

                                <TableBody>

                                    {upsPorEstado.length === 0 ? (

                                        <TableRow>

                                            <TableCell
                                                colSpan={2}
                                                align="center"
                                            >
                                                No hay información.
                                            </TableCell>

                                        </TableRow>

                                    ) : (

                                        upsPorEstado.map(
                                            ([estado, cantidad]) => (

                                                <TableRow
                                                    key={estado}
                                                >

                                                    <TableCell>
                                                        {estado}
                                                    </TableCell>

                                                    <TableCell align="right">
                                                        {cantidad}
                                                    </TableCell>

                                                </TableRow>

                                            )
                                        )

                                    )}

                                </TableBody>

                            </Table>

                        </TableContainer>

                    </Paper>

                </Grid>


                <Grid
                    item
                    xs={12}
                    md={6}
                >

                    <Paper
                        sx={{
                            p: 2
                        }}
                    >

                        <Typography
                            variant="h6"
                            fontWeight="bold"
                            sx={{
                                mb: 2
                            }}
                        >
                            Mantenimientos por tipo
                        </Typography>

                        <TableContainer>

                            <Table
                                size="small"
                            >

                                <TableHead>

                                    <TableRow>

                                        <TableCell>
                                            Tipo
                                        </TableCell>

                                        <TableCell align="right">
                                            Cantidad
                                        </TableCell>

                                    </TableRow>

                                </TableHead>

                                <TableBody>

                                    {mantenimientosPorTipo.length === 0 ? (

                                        <TableRow>

                                            <TableCell
                                                colSpan={2}
                                                align="center"
                                            >
                                                No hay información.
                                            </TableCell>

                                        </TableRow>

                                    ) : (

                                        mantenimientosPorTipo.map(
                                            ([tipo, cantidad]) => (

                                                <TableRow
                                                    key={tipo}
                                                >

                                                    <TableCell>
                                                        {tipo}
                                                    </TableCell>

                                                    <TableCell align="right">
                                                        {cantidad}
                                                    </TableCell>

                                                </TableRow>

                                            )
                                        )

                                    )}

                                </TableBody>

                            </Table>

                        </TableContainer>

                    </Paper>

                </Grid>

            </Grid>


            {/* =================================================
                INVENTARIO POR AGENCIA
            ================================================= */}

            <Paper
                sx={{
                    p: 2,
                    mb: 3
                }}
            >

                <Typography
                    variant="h6"
                    fontWeight="bold"
                    sx={{
                        mb: 2
                    }}
                >
                    Inventario por agencia
                </Typography>

                <TableContainer>

                    <Table
                        size="small"
                    >

                        <TableHead>

                            <TableRow>

                                <TableCell>
                                    Agencia
                                </TableCell>

                                <TableCell>
                                    Ciudad
                                </TableCell>

                                <TableCell>
                                    Tipo
                                </TableCell>

                                <TableCell align="right">
                                    UPS
                                </TableCell>

                                <TableCell align="right">
                                    Baterías
                                </TableCell>

                            </TableRow>

                        </TableHead>

                        <TableBody>

                            {inventarioPorAgencia.length === 0 ? (

                                <TableRow>

                                    <TableCell
                                        colSpan={5}
                                        align="center"
                                    >
                                        No hay agencias registradas.
                                    </TableCell>

                                </TableRow>

                            ) : (

                                inventarioPorAgencia.map(
                                    agencia => (

                                        <TableRow
                                            key={agencia.id}
                                        >

                                            <TableCell>
                                                {agencia.nombre}
                                            </TableCell>

                                            <TableCell>
                                                {agencia.ciudad}
                                            </TableCell>

                                            <TableCell>
                                                {agencia.tipo}
                                            </TableCell>

                                            <TableCell align="right">
                                                {agencia.ups}
                                            </TableCell>

                                            <TableCell align="right">
                                                {agencia.baterias}
                                            </TableCell>

                                        </TableRow>

                                    )
                                )

                            )}

                        </TableBody>

                    </Table>

                </TableContainer>

            </Paper>


            {/* =================================================
                PRESUPUESTO
            ================================================= */}

            <Paper
                sx={{
                    p: 2,
                    mb: 3
                }}
            >

                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center",
                        flexWrap: "wrap",
                        gap: 2,
                        mb: 2
                    }}
                >

                    <Box>

                        <Typography
                            variant="h6"
                            fontWeight="bold"
                        >
                            Proyección de presupuesto
                        </Typography>

                        <Typography
                            variant="body2"
                            color="text.secondary"
                        >
                            Proyección de reemplazo de baterías.
                        </Typography>

                    </Box>


                    <TextField
                        select
                        size="small"
                        label="Año"
                        value={anio}
                        onChange={e =>
                            setAnio(
                                Number(
                                    e.target.value
                                )
                            )
                        }
                        sx={{
                            width: 140
                        }}
                    >

                        {[0, 1, 2, 3, 4].map(
                            offset => {

                                const valor =
                                    anioActual +
                                    offset;

                                return (

                                    <MenuItem
                                        key={valor}
                                        value={valor}
                                    >
                                        {valor}
                                    </MenuItem>

                                );

                            }
                        )}

                    </TextField>

                </Box>


                <Divider
                    sx={{
                        mb: 2
                    }}
                />


                {errorPresupuesto && (

                    <Alert
                        severity="warning"
                        sx={{
                            mb: 2
                        }}
                    >
                        {errorPresupuesto}
                    </Alert>

                )}


                {cargandoPresupuesto ? (

                    <Box
                        sx={{
                            display: "flex",
                            justifyContent: "center",
                            p: 4
                        }}
                    >

                        <CircularProgress />

                    </Box>

                ) : (

                    <Grid
                        container
                        spacing={2}
                    >

                        <Grid
                            item
                            xs={12}
                            md={4}
                        >

                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                UPS proyectados
                            </Typography>

                            <Typography
                                variant="h5"
                                fontWeight="bold"
                            >
                                {presupuestoUPS}
                            </Typography>

                        </Grid>


                        <Grid
                            item
                            xs={12}
                            md={4}
                        >

                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                Baterías proyectadas
                            </Typography>

                            <Typography
                                variant="h5"
                                fontWeight="bold"
                            >
                                {presupuestoBaterias}
                            </Typography>

                        </Grid>


                        <Grid
                            item
                            xs={12}
                            md={4}
                        >

                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                Presupuesto proyectado
                            </Typography>

                            <Typography
                                variant="h5"
                                fontWeight="bold"
                            >
                                {formatearMoneda(
                                    presupuestoTotal
                                )}
                            </Typography>

                        </Grid>

                    </Grid>

                )}

            </Paper>


            {/* =================================================
                ÚLTIMOS MANTENIMIENTOS
            ================================================= */}

            <Paper
                sx={{
                    p: 2,
                    mb: 3
                }}
            >

                <Typography
                    variant="h6"
                    fontWeight="bold"
                    sx={{
                        mb: 2
                    }}
                >
                    Últimos mantenimientos
                </Typography>

                <TableContainer>

                    <Table
                        size="small"
                    >

                        <TableHead>

                            <TableRow>

                                <TableCell>
                                    Fecha
                                </TableCell>

                                <TableCell>
                                    UPS
                                </TableCell>

                                <TableCell>
                                    Tipo
                                </TableCell>

                                <TableCell>
                                    Técnico / proveedor
                                </TableCell>

                                <TableCell align="right">
                                    Costo
                                </TableCell>

                            </TableRow>

                        </TableHead>

                        <TableBody>

                            {ultimosMantenimientos.length === 0 ? (

                                <TableRow>

                                    <TableCell
                                        colSpan={5}
                                        align="center"
                                    >
                                        No hay mantenimientos registrados.
                                    </TableCell>

                                </TableRow>

                            ) : (

                                ultimosMantenimientos.map(
                                    mantenimiento => (

                                        <TableRow
                                            key={
                                                mantenimiento.id
                                            }
                                        >

                                            <TableCell>
                                                {formatearFecha(
                                                    mantenimiento.fechaMantenimiento
                                                )}
                                            </TableCell>

                                            <TableCell>
                                                {obtenerNombreUPS(
                                                    mantenimiento.ups
                                                ) ||
                                                    mantenimiento.upsId}
                                            </TableCell>

                                            <TableCell>
                                                {obtenerNombreTipoMantenimiento(
                                                    mantenimiento
                                                )}
                                            </TableCell>

                                            <TableCell>
                                                {obtenerNombreTecnico(
                                                    mantenimiento
                                                )}
                                            </TableCell>

                                            <TableCell align="right">
                                                {formatearMoneda(
                                                    mantenimiento.costo
                                                )}
                                            </TableCell>

                                        </TableRow>

                                    )
                                )

                            )}

                        </TableBody>

                    </Table>

                </TableContainer>

            </Paper>

        </Box>

    );

}
import {
    useEffect,
    useMemo,
    useState
} from "react";

import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    Grid,
    InputLabel,
    MenuItem,
    Select,
    Snackbar,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Typography
} from "@mui/material";

import ExpandMoreIcon
    from "@mui/icons-material/ExpandMore";
import SettingsIcon
    from "@mui/icons-material/Settings";

import {
    obtenerConfiguracionesPresupuesto,
    crearConfiguracionPresupuesto,
    actualizarConfiguracionPresupuesto,
    obtenerPoliticaReemplazo,
    actualizarPoliticaReemplazo,
    obtenerProyeccionPresupuesto
} from "../services/presupuestoService";

import {
    tienePermiso
} from "../services/permisos";

const formatearMoneda = (valor) =>
    new Intl.NumberFormat(
        "es-HN",
        {
            style: "currency",
            currency: "HNL"
        }
    ).format(
        Number(valor || 0)
    );

const formatearFecha = (valor) =>
    valor
        ? new Date(
            valor
        ).toLocaleDateString("es-HN")
        : "—";

export default function Presupuesto() {
    const anioActual =
        new Date().getFullYear();

    const [anio, setAnio] =
        useState(anioActual);
    const [
        configuraciones,
        setConfiguraciones
    ] = useState([]);
    const [politica, setPolitica] =
        useState(null);
    const [proyeccion, setProyeccion] =
        useState(null);
    const [cargando, setCargando] =
        useState(true);
    const [
        cargandoProyeccion,
        setCargandoProyeccion
    ] = useState(false);
    const [dialogoCosto, setDialogoCosto] =
        useState(false);
    const [
        dialogoPolitica,
        setDialogoPolitica
    ] = useState(false);
    const [costo, setCosto] =
        useState("");
    const [mesesLocal, setMesesLocal] =
        useState("");
    const [mesesForanea, setMesesForanea] =
        useState("");
    const [mensaje, setMensaje] =
        useState("");
    const [error, setError] =
        useState("");

    const puedeGestionar =
        tienePermiso(
            "GESTIONAR_PRESUPUESTO"
        );

    const configuracion =
        useMemo(
            () =>
                configuraciones.find(
                    (c) =>
                        Number(c.anio) ===
                        Number(anio)
                ),
            [
                configuraciones,
                anio
            ]
        );

    const anios =
        useMemo(() => {
            const lista = [];

            for (
                let i = 0;
                i <= 5;
                i++
            ) {
                lista.push(
                    anioActual + i
                );
            }

            configuraciones.forEach(
                (c) =>
                    lista.push(
                        Number(c.anio)
                    )
            );

            return [
                ...new Set(lista)
            ].sort(
                (a, b) => a - b
            );
        }, [
            configuraciones,
            anioActual
        ]);

    const cargarBase = async () => {
        const [configs, politicaData] =
            await Promise.all([
                obtenerConfiguracionesPresupuesto(),
                obtenerPoliticaReemplazo()
            ]);

        setConfiguraciones(
            Array.isArray(configs)
                ? configs
                : []
        );
        setPolitica(politicaData);
    };

    useEffect(() => {
        const cargar = async () => {
            try {
                setCargando(true);
                await cargarBase();
            } catch (e) {
                setError(
                    e.response?.data?.mensaje ||
                    "No fue posible cargar la configuración del presupuesto."
                );
            } finally {
                setCargando(false);
            }
        };

        cargar();
    }, []);

    const generar = async () => {
        if (
            !configuracion ||
            !configuracion.activo
        ) {
            setError(
                `Configure un costo de batería activo para ${anio}.`
            );
            return;
        }

        try {
            setCargandoProyeccion(
                true
            );
            setError("");

            const datos =
                await obtenerProyeccionPresupuesto(
                    anio
                );

            setProyeccion(datos);
        } catch (e) {
            setProyeccion(null);
            setError(
                e.response?.data?.mensaje ||
                "No fue posible generar la proyección."
            );
        } finally {
            setCargandoProyeccion(
                false
            );
        }
    };

    const abrirCosto = () => {
        setCosto(
            configuracion
                ?.costoUnitarioBateria ??
            ""
        );
        setDialogoCosto(true);
    };

    const guardarCosto = async () => {
        const valor =
            Number(costo);

        if (
            costo === "" ||
            valor <= 0
        ) {
            setError(
                "El costo unitario debe ser mayor que cero."
            );
            return;
        }

        try {
            if (configuracion) {
                await actualizarConfiguracionPresupuesto(
                    configuracion.id,
                    {
                        costoUnitarioBateria:
                            valor,
                        activo: true
                    }
                );
            } else {
                await crearConfiguracionPresupuesto(
                    {
                        anio:
                            Number(anio),
                        costoUnitarioBateria:
                            valor
                    }
                );
            }

            await cargarBase();
            setProyeccion(null);
            setDialogoCosto(false);
            setMensaje(
                "Costo anual guardado correctamente."
            );
        } catch (e) {
            setError(
                e.response?.data?.mensaje ||
                "No fue posible guardar el costo anual."
            );
        }
    };

    const abrirPolitica = () => {
        if (!politica) return;

        setMesesLocal(
            politica.mesesReemplazoLocal
        );
        setMesesForanea(
            politica.mesesReemplazoForanea
        );
        setDialogoPolitica(true);
    };

    const guardarPolitica = async () => {
        const local =
            Number(mesesLocal);
        const foranea =
            Number(mesesForanea);

        if (
            local <= 0 ||
            local > 120 ||
            foranea <= 0 ||
            foranea > 120
        ) {
            setError(
                "Los ciclos deben estar entre 1 y 120 meses."
            );
            return;
        }

        try {
            await actualizarPoliticaReemplazo(
                {
                    mesesReemplazoLocal:
                        local,
                    mesesReemplazoForanea:
                        foranea
                }
            );

            await cargarBase();
            setProyeccion(null);
            setDialogoPolitica(
                false
            );
            setMensaje(
                "Política actualizada correctamente."
            );
        } catch (e) {
            setError(
                e.response?.data?.mensaje ||
                "No fue posible guardar la política."
            );
        }
    };

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

    return (
        <Box>
            <Typography
                variant="h4"
                fontWeight="bold"
            >
                Presupuesto de Baterías
            </Typography>
            <Typography
                color="text.secondary"
                sx={{ mb: 3 }}
            >
                La política define cada cuánto se reemplaza el banco de baterías y el costo anual permite estimar el presupuesto.
            </Typography>

            {error && (
                <Alert
                    severity="error"
                    sx={{ mb: 3 }}
                    onClose={() =>
                        setError("")
                    }
                >
                    {error}
                </Alert>
            )}

            <Card sx={{ mb: 3 }}>
                <CardContent>
                    <Box
                        sx={{
                            display: "flex",
                            justifyContent:
                                "space-between",
                            alignItems: "center",
                            gap: 2,
                            flexWrap: "wrap"
                        }}
                    >
                        <Box>
                            <Typography
                                variant="h6"
                                fontWeight="bold"
                            >
                                Política global de reemplazo
                            </Typography>
                            <Typography
                                color="text.secondary"
                            >
                                Local:{" "}
                                <strong>
                                    {
                                        politica
                                            ?.mesesReemplazoLocal
                                    } meses
                                </strong>
                                {" · "}
                                Foránea:{" "}
                                <strong>
                                    {
                                        politica
                                            ?.mesesReemplazoForanea
                                    } meses
                                </strong>
                            </Typography>
                        </Box>

                        {puedeGestionar && (
                            <Button
                                variant="outlined"
                                startIcon={
                                    <SettingsIcon />
                                }
                                onClick={
                                    abrirPolitica
                                }
                            >
                                Editar política
                            </Button>
                        )}
                    </Box>
                </CardContent>
            </Card>

            <Card sx={{ mb: 3 }}>
                <CardContent>
                    <Grid
                        container
                        spacing={2}
                        alignItems="center"
                    >
                        <Grid
                            size={{
                                xs: 12,
                                md: 3
                            }}
                        >
                            <FormControl
                                fullWidth
                            >
                                <InputLabel>
                                    Año
                                </InputLabel>
                                <Select
                                    label="Año"
                                    value={anio}
                                    onChange={(e) => {
                                        setAnio(
                                            Number(
                                                e.target.value
                                            )
                                        );
                                        setProyeccion(
                                            null
                                        );
                                    }}
                                >
                                    {anios.map(
                                        (valor) => (
                                            <MenuItem
                                                key={valor}
                                                value={valor}
                                            >
                                                {valor}
                                            </MenuItem>
                                        )
                                    )}
                                </Select>
                            </FormControl>
                        </Grid>

                        <Grid
                            size={{
                                xs: 12,
                                md: 4
                            }}
                        >
                            <Typography
                                color="text.secondary"
                                variant="body2"
                            >
                                Costo estimado por batería
                            </Typography>
                            <Typography
                                variant="h5"
                                fontWeight="bold"
                            >
                                {configuracion
                                    ? formatearMoneda(
                                        configuracion
                                            .costoUnitarioBateria
                                    )
                                    : "Sin configurar"}
                            </Typography>
                        </Grid>

                        <Grid
                            size={{
                                xs: 12,
                                md: 5
                            }}
                        >
                            <Box
                                sx={{
                                    display: "flex",
                                    gap: 1,
                                    justifyContent:
                                        "flex-end",
                                    flexWrap: "wrap"
                                }}
                            >
                                {puedeGestionar && (
                                    <Button
                                        variant="outlined"
                                        onClick={
                                            abrirCosto
                                        }
                                    >
                                        {configuracion
                                            ? "Editar costo"
                                            : "Configurar costo"}
                                    </Button>
                                )}

                                <Button
                                    variant="contained"
                                    onClick={generar}
                                    disabled={
                                        cargandoProyeccion
                                    }
                                >
                                    Generar proyección
                                </Button>
                            </Box>
                        </Grid>
                    </Grid>
                </CardContent>
            </Card>

            {cargandoProyeccion && (
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "center",
                        py: 8
                    }}
                >
                    <CircularProgress />
                </Box>
            )}

            {!cargandoProyeccion &&
                proyeccion && (
                <>
                    <Grid
                        container
                        spacing={2}
                        sx={{ mb: 3 }}
                    >
                        {[
                            [
                                "UPS en inventario",
                                proyeccion
                                    .totalUPSInventario
                            ],
                            [
                                "UPS con reemplazo",
                                proyeccion
                                    .totalUPSConReemplazo
                            ],
                            [
                                "Baterías requeridas",
                                proyeccion
                                    .totalBaterias
                            ],
                            [
                                "Presupuesto",
                                formatearMoneda(
                                    proyeccion
                                        .totalPresupuesto
                                )
                            ],
                            [
                                "Vencidos",
                                proyeccion
                                    .totalUPSVencidos
                            ],
                            [
                                "Ya programados",
                                proyeccion
                                    .totalUPSProgramados
                            ]
                        ].map(
                            ([titulo, valor]) => (
                                <Grid
                                    key={titulo}
                                    size={{
                                        xs: 12,
                                        sm: 6,
                                        md: 4
                                    }}
                                >
                                    <Card>
                                        <CardContent>
                                            <Typography
                                                color="text.secondary"
                                                variant="body2"
                                            >
                                                {titulo}
                                            </Typography>
                                            <Typography
                                                variant="h5"
                                                fontWeight="bold"
                                            >
                                                {valor ?? 0}
                                            </Typography>
                                        </CardContent>
                                    </Card>
                                </Grid>
                            )
                        )}
                    </Grid>

                    {(proyeccion.agencias ||
                        []).map(
                        (agencia) => (
                            <Accordion
                                key={
                                    agencia.agenciaId
                                }
                                sx={{ mb: 1 }}
                            >
                                <AccordionSummary
                                    expandIcon={
                                        <ExpandMoreIcon />
                                    }
                                >
                                    <Box
                                        sx={{
                                            width: "100%",
                                            display: "flex",
                                            justifyContent:
                                                "space-between",
                                            pr: 2
                                        }}
                                    >
                                        <Box>
                                            <Typography
                                                fontWeight="bold"
                                            >
                                                {agencia.agencia}
                                            </Typography>
                                            <Typography
                                                variant="body2"
                                                color="text.secondary"
                                            >
                                                {agencia.tipoAgencia}
                                                {" · "}
                                                {agencia.cantidadUPSConReemplazo} UPS
                                                {" · "}
                                                {agencia.cantidadBaterias} baterías
                                            </Typography>
                                        </Box>
                                        <Typography
                                            fontWeight="bold"
                                        >
                                            {formatearMoneda(
                                                agencia.subtotal
                                            )}
                                        </Typography>
                                    </Box>
                                </AccordionSummary>

                                <AccordionDetails>
                                    <TableContainer>
                                        <Table size="small">
                                            <TableHead>
                                                <TableRow>
                                                    <TableCell>
                                                        UPS
                                                    </TableCell>
                                                    <TableCell>
                                                        Marca / modelo
                                                    </TableCell>
                                                    <TableCell>
                                                        Fecha proyectada
                                                    </TableCell>
                                                    <TableCell>
                                                        Situación
                                                    </TableCell>
                                                    <TableCell>
                                                        Mantenimiento programado
                                                    </TableCell>
                                                    <TableCell
                                                        align="right"
                                                    >
                                                        Baterías
                                                    </TableCell>
                                                    <TableCell
                                                        align="right"
                                                    >
                                                        Costo
                                                    </TableCell>
                                                </TableRow>
                                            </TableHead>
                                            <TableBody>
                                                {(agencia.ups ||
                                                    []).map(
                                                    (equipo) => (
                                                        <TableRow
                                                            key={
                                                                equipo.upsId
                                                            }
                                                        >
                                                            <TableCell>
                                                                <strong>
                                                                    {equipo.codigoUPS}
                                                                </strong>
                                                                <br />
                                                                Inventario: {equipo.numeroInventario || "—"}
                                                                <br />
                                                                Serie: {equipo.numeroSerie || "—"}
                                                            </TableCell>
                                                            <TableCell>
                                                                {equipo.marca}
                                                                {" "}
                                                                {equipo.modelo}
                                                            </TableCell>
                                                            <TableCell>
                                                                {formatearFecha(
                                                                    equipo
                                                                        .fechaProyectadaReemplazo
                                                                )}
                                                            </TableCell>
                                                            <TableCell>
                                                                {equipo.estaVencido ? (
                                                                    <Chip
                                                                        label="Vencido"
                                                                        color="error"
                                                                        size="small"
                                                                    />
                                                                ) : equipo.reemplazoAnteriorPendiente ? (
                                                                    <Chip
                                                                        label="Existe ciclo anterior pendiente"
                                                                        color="warning"
                                                                        size="small"
                                                                    />
                                                                ) : (
                                                                    <Chip
                                                                        label="Proyectado"
                                                                        color="info"
                                                                        size="small"
                                                                    />
                                                                )}
                                                            </TableCell>
                                                            <TableCell>
                                                                {equipo.tieneMantenimientoProgramado
                                                                    ? formatearFecha(
                                                                        equipo
                                                                            .fechaMantenimientoProgramado
                                                                    )
                                                                    : "No"}
                                                            </TableCell>
                                                            <TableCell
                                                                align="right"
                                                            >
                                                                {equipo.cantidadBaterias}
                                                            </TableCell>
                                                            <TableCell
                                                                align="right"
                                                            >
                                                                {formatearMoneda(
                                                                    equipo.costoEstimado
                                                                )}
                                                            </TableCell>
                                                        </TableRow>
                                                    )
                                                )}
                                            </TableBody>
                                        </Table>
                                    </TableContainer>
                                </AccordionDetails>
                            </Accordion>
                        )
                    )}

                    {proyeccion
                        .upsInformacionInsuficiente >
                        0 && (
                        <Card sx={{ mt: 3 }}>
                            <CardContent>
                                <Typography
                                    variant="h6"
                                    fontWeight="bold"
                                    sx={{ mb: 2 }}
                                >
                                    UPS con información insuficiente
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
                                                    Observación
                                                </TableCell>
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {(proyeccion
                                                .upsInformacionInsuficienteDetalle ||
                                                []).map(
                                                (equipo) => (
                                                    <TableRow
                                                        key={
                                                            equipo.upsId
                                                        }
                                                    >
                                                        <TableCell>
                                                            {equipo.agencia}
                                                        </TableCell>
                                                        <TableCell>
                                                            <strong>{equipo.codigoUPS}</strong>
                                                            <br />
                                                            Inventario: {equipo.numeroInventario || "—"}
                                                        </TableCell>
                                                        <TableCell>
                                                            {equipo.observacion}
                                                        </TableCell>
                                                    </TableRow>
                                                )
                                            )}
                                        </TableBody>
                                    </Table>
                                </TableContainer>
                            </CardContent>
                        </Card>
                    )}
                </>
            )}

            <Dialog
                open={dialogoCosto}
                onClose={() =>
                    setDialogoCosto(false)
                }
                fullWidth
                maxWidth="sm"
            >
                <DialogTitle>
                    Costo estimado {anio}
                </DialogTitle>
                <DialogContent>
                    <TextField
                        fullWidth
                        type="number"
                        label="Costo unitario por batería"
                        value={costo}
                        onChange={(e) =>
                            setCosto(
                                e.target.value
                            )
                        }
                        sx={{ mt: 2 }}
                        helperText="Debe ser mayor que cero."
                    />
                </DialogContent>
                <DialogActions>
                    <Button
                        onClick={() =>
                            setDialogoCosto(
                                false
                            )
                        }
                    >
                        Cancelar
                    </Button>
                    <Button
                        variant="contained"
                        onClick={
                            guardarCosto
                        }
                    >
                        Guardar
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={dialogoPolitica}
                onClose={() =>
                    setDialogoPolitica(
                        false
                    )
                }
                fullWidth
                maxWidth="sm"
            >
                <DialogTitle>
                    Política global
                </DialogTitle>
                <DialogContent>
                    <TextField
                        fullWidth
                        type="number"
                        label="Ciclo Local (meses)"
                        value={mesesLocal}
                        onChange={(e) =>
                            setMesesLocal(
                                e.target.value
                            )
                        }
                        sx={{ mt: 2 }}
                    />
                    <TextField
                        fullWidth
                        type="number"
                        label="Ciclo Foránea (meses)"
                        value={mesesForanea}
                        onChange={(e) =>
                            setMesesForanea(
                                e.target.value
                            )
                        }
                        sx={{ mt: 2 }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button
                        onClick={() =>
                            setDialogoPolitica(
                                false
                            )
                        }
                    >
                        Cancelar
                    </Button>
                    <Button
                        variant="contained"
                        onClick={
                            guardarPolitica
                        }
                    >
                        Guardar
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={Boolean(mensaje)}
                autoHideDuration={4000}
                onClose={() =>
                    setMensaje("")
                }
            >
                <Alert
                    severity="success"
                    onClose={() =>
                        setMensaje("")
                    }
                >
                    {mensaje}
                </Alert>
            </Snackbar>
        </Box>
    );
}

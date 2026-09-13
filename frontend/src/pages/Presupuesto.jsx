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
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
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

import RefreshIcon
    from "@mui/icons-material/Refresh";

import CalculateIcon
    from "@mui/icons-material/Calculate";

import AccountBalanceIcon
    from "@mui/icons-material/AccountBalance";

import BatteryChargingFullIcon
    from "@mui/icons-material/BatteryChargingFull";

import MemoryIcon
    from "@mui/icons-material/Memory";

import BusinessIcon
    from "@mui/icons-material/Business";

import WarningAmberIcon
    from "@mui/icons-material/WarningAmber";

import SaveIcon
    from "@mui/icons-material/Save";


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


export default function Presupuesto() {

    // =====================================================
    // AÑO ACTUAL
    // =====================================================

    const anioActual =
        new Date().getFullYear();


    // =====================================================
    // ESTADO GENERAL
    // =====================================================

    const [
        anio,
        setAnio
    ] = useState(
        anioActual
    );


    const [
        proyeccion,
        setProyeccion
    ] = useState(
        null
    );


    const [
        configuraciones,
        setConfiguraciones
    ] = useState(
        []
    );


    const [
        politica,
        setPolitica
    ] = useState(
        null
    );


    const [
        cargando,
        setCargando
    ] = useState(
        false
    );


    const [
        cargandoInicial,
        setCargandoInicial
    ] = useState(
        true
    );


    const [
        error,
        setError
    ] = useState(
        ""
    );


    const [
        mensaje,
        setMensaje
    ] = useState(
        ""
    );


    // =====================================================
    // DIÁLOGO COSTO ANUAL
    // =====================================================

    const [
        dialogoCosto,
        setDialogoCosto
    ] = useState(
        false
    );


    const [
        configuracionSeleccionada,
        setConfiguracionSeleccionada
    ] = useState(
        null
    );


    const [
        guardandoCosto,
        setGuardandoCosto
    ] = useState(
        false
    );


    const [
        formularioCosto,
        setFormularioCosto
    ] = useState({
        anio:
            anioActual,

        costoUnitarioBateria:
            ""
    });


    // =====================================================
    // DIÁLOGO POLÍTICA
    // =====================================================

    const [
        dialogoPolitica,
        setDialogoPolitica
    ] = useState(
        false
    );


    const [
        guardandoPolitica,
        setGuardandoPolitica
    ] = useState(
        false
    );


    const [
        formularioPolitica,
        setFormularioPolitica
    ] = useState({
        mesesReemplazoLocal:
            "",

        mesesReemplazoForanea:
            ""
    });


    // =====================================================
    // PERMISOS
    // =====================================================

    const puedeGestionar =
        tienePermiso(
            "GESTIONAR_PRESUPUESTO"
        );


    // =====================================================
    // CONFIGURACIÓN DEL AÑO SELECCIONADO
    // =====================================================

    const configuracionActual =
        useMemo(
            () => {

                return configuraciones.find(
                    configuracion =>
                        Number(
                            configuracion.anio
                        ) ===
                        Number(anio)
                );

            },
            [
                configuraciones,
                anio
            ]
        );


    // =====================================================
    // AÑOS DISPONIBLES
    //
    // Año actual + próximos 5 años
    // + cualquier año que ya exista en BD.
    // =====================================================

    const aniosDisponibles =
        useMemo(
            () => {

                const anios =
                    [];


                for (
                    let i = 0;
                    i <= 5;
                    i++
                ) {

                    anios.push(
                        anioActual + i
                    );

                }


                configuraciones.forEach(
                    configuracion => {

                        const valor =
                            Number(
                                configuracion.anio
                            );

                        if (
                            !Number.isNaN(
                                valor
                            )
                        ) {

                            anios.push(
                                valor
                            );

                        }

                    }
                );


                return [
                    ...new Set(
                        anios
                    )
                ].sort(
                    (a, b) =>
                        a - b
                );

            },
            [
                configuraciones,
                anioActual
            ]
        );


    // =====================================================
    // FORMATEAR MONEDA
    // =====================================================

    const formatearMoneda =
        valor => {

            return new Intl
                .NumberFormat(
                    "es-HN",
                    {
                        style:
                            "currency",

                        currency:
                            "HNL",

                        minimumFractionDigits:
                            2
                    }
                )
                .format(
                    Number(
                        valor ||
                        0
                    )
                );

        };


    // =====================================================
    // FORMATEAR FECHA
    // =====================================================

    const formatearFecha =
        fecha => {

            if (!fecha) {
                return "—";
            }


            const valor =
                String(
                    fecha
                ).substring(
                    0,
                    10
                );


            const partes =
                valor.split(
                    "-"
                );


            if (
                partes.length !==
                3
            ) {

                return valor;
            }


            return (
                `${partes[2]}/` +
                `${partes[1]}/` +
                `${partes[0]}`
            );

        };


    // =====================================================
    // CARGAR CONFIGURACIONES
    // =====================================================

    const cargarConfiguraciones =
        async () => {

            const datos =
                await obtenerConfiguracionesPresupuesto();


            setConfiguraciones(
                Array.isArray(
                    datos
                )
                    ? datos
                    : []
            );

        };


    // =====================================================
    // CARGAR POLÍTICA
    // =====================================================

    const cargarPolitica =
        async () => {

            const datos =
                await obtenerPoliticaReemplazo();


            setPolitica(
                datos
            );

        };


    // =====================================================
    // CARGA INICIAL
    // =====================================================

    useEffect(
        () => {

            const cargar =
                async () => {

                    try {

                        setCargandoInicial(
                            true
                        );

                        setError(
                            ""
                        );


                        await Promise.all([
                            cargarConfiguraciones(),
                            cargarPolitica()
                        ]);

                    } catch (errorCarga) {

                        console.error(
                            "Error al cargar presupuesto:",
                            errorCarga
                        );


                        setError(
                            errorCarga
                                .response
                                ?.data
                                ?.mensaje ||
                            "No fue posible cargar la información de presupuesto."
                        );

                    } finally {

                        setCargandoInicial(
                            false
                        );

                    }

                };


            cargar();

        },
        []
    );


    // =====================================================
    // AL CAMBIAR AÑO
    // =====================================================

    const cambiarAnio =
        valor => {

            setAnio(
                Number(
                    valor
                )
            );

            setProyeccion(
                null
            );

            setError(
                ""
            );

        };


    // =====================================================
    // CARGAR PROYECCIÓN
    // =====================================================

    const cargarProyeccion =
        async () => {

            if (
                !anio ||
                Number(anio) <
                    2020 ||
                Number(anio) >
                    2100
            ) {

                setError(
                    "Seleccione un año válido."
                );

                return;
            }


            if (
                !configuracionActual
            ) {

                setError(
                    `Debe configurar el costo estimado de batería para ${anio} antes de generar la proyección.`
                );

                return;
            }


            if (!politica) {

                setError(
                    "No existe una política de reemplazo configurada."
                );

                return;
            }


            try {

                setCargando(
                    true
                );

                setError(
                    ""
                );

                setProyeccion(
                    null
                );


                const datos =
                    await obtenerProyeccionPresupuesto(
                        Number(anio)
                    );


                setProyeccion(
                    datos
                );

            } catch (errorProyeccion) {

                console.error(
                    "Error al obtener proyección:",
                    errorProyeccion
                );


                if (
                    errorProyeccion
                        .response
                        ?.status ===
                    404
                ) {

                    setError(
                        errorProyeccion
                            .response
                            ?.data
                            ?.mensaje ||
                        `No existe información suficiente para proyectar ${anio}.`
                    );

                } else if (
                    errorProyeccion
                        .response
                        ?.status ===
                    403
                ) {

                    setError(
                        "No tiene permisos para consultar el presupuesto."
                    );

                } else {

                    setError(
                        "No fue posible obtener la proyección."
                    );

                }

            } finally {

                setCargando(
                    false
                );

            }

        };


    // =====================================================
    // ABRIR COSTO ANUAL
    // =====================================================

    const abrirCosto =
        () => {

            setError(
                ""
            );


            if (
                configuracionActual
            ) {

                setConfiguracionSeleccionada(
                    configuracionActual
                );


                setFormularioCosto({
                    anio:
                        Number(
                            configuracionActual
                                .anio
                        ),

                    costoUnitarioBateria:
                        configuracionActual
                            .costoUnitarioBateria
                });

            } else {

                setConfiguracionSeleccionada(
                    null
                );


                setFormularioCosto({
                    anio:
                        Number(anio),

                    costoUnitarioBateria:
                        ""
                });

            }


            setDialogoCosto(
                true
            );

        };


    // =====================================================
    // CERRAR COSTO
    // =====================================================

    const cerrarCosto =
        () => {

            if (guardandoCosto) {
                return;
            }


            setDialogoCosto(
                false
            );


            setConfiguracionSeleccionada(
                null
            );

        };


    // =====================================================
    // GUARDAR COSTO ANUAL
    // =====================================================

    const guardarCosto =
        async () => {

            const anioCosto =
                Number(
                    formularioCosto
                        .anio
                );


            const costo =
                Number(
                    formularioCosto
                        .costoUnitarioBateria
                );


            if (
                anioCosto <
                    2020 ||
                anioCosto >
                    2100
            ) {

                setError(
                    "El año debe estar entre 2020 y 2100."
                );

                return;
            }


            if (
                formularioCosto
                    .costoUnitarioBateria ===
                    "" ||
                costo < 0
            ) {

                setError(
                    "Ingrese un costo unitario válido."
                );

                return;
            }


            if (!politica) {

                setError(
                    "No existe una política de reemplazo disponible."
                );

                return;
            }


            try {

                setGuardandoCosto(
                    true
                );

                setError(
                    ""
                );


                if (
                    configuracionSeleccionada
                ) {

                    await actualizarConfiguracionPresupuesto(
                        configuracionSeleccionada.id,
                        {
                            /*
                             * Estos meses se mantienen
                             * únicamente por compatibilidad
                             * con el modelo anterior.
                             *
                             * La proyección YA NO utiliza
                             * estos campos.
                             */
                            mesesReemplazoLocal:
                                Number(
                                    politica
                                        .mesesReemplazoLocal
                                ),

                            mesesReemplazoForanea:
                                Number(
                                    politica
                                        .mesesReemplazoForanea
                                ),

                            costoUnitarioBateria:
                                costo,

                            activo:
                                true
                        }
                    );

                } else {

                    await crearConfiguracionPresupuesto(
                        {
                            anio:
                                anioCosto,

                            /*
                             * Compatibilidad temporal
                             * con el backend existente.
                             */
                            mesesReemplazoLocal:
                                Number(
                                    politica
                                        .mesesReemplazoLocal
                                ),

                            mesesReemplazoForanea:
                                Number(
                                    politica
                                        .mesesReemplazoForanea
                                ),

                            costoUnitarioBateria:
                                costo
                        }
                    );

                }


                await cargarConfiguraciones();


                setAnio(
                    anioCosto
                );


                setProyeccion(
                    null
                );


                setMensaje(
                    `Costo estimado para ${anioCosto} guardado correctamente.`
                );


                setDialogoCosto(
                    false
                );


                setConfiguracionSeleccionada(
                    null
                );

            } catch (errorGuardar) {

                console.error(
                    "Error al guardar costo:",
                    errorGuardar
                );


                setError(
                    errorGuardar
                        .response
                        ?.data
                        ?.mensaje ||
                    "No fue posible guardar el costo estimado."
                );

            } finally {

                setGuardandoCosto(
                    false
                );

            }

        };


    // =====================================================
    // ABRIR POLÍTICA
    // =====================================================

    const abrirPolitica =
        () => {

            if (!politica) {

                setError(
                    "No existe una política de reemplazo disponible."
                );

                return;
            }


            setFormularioPolitica({
                mesesReemplazoLocal:
                    politica
                        .mesesReemplazoLocal,

                mesesReemplazoForanea:
                    politica
                        .mesesReemplazoForanea
            });


            setDialogoPolitica(
                true
            );

        };


    // =====================================================
    // CERRAR POLÍTICA
    // =====================================================

    const cerrarPolitica =
        () => {

            if (
                guardandoPolitica
            ) {
                return;
            }


            setDialogoPolitica(
                false
            );

        };


    // =====================================================
    // GUARDAR POLÍTICA
    // =====================================================

    const guardarPolitica =
        async () => {

            const mesesLocal =
                Number(
                    formularioPolitica
                        .mesesReemplazoLocal
                );


            const mesesForanea =
                Number(
                    formularioPolitica
                        .mesesReemplazoForanea
                );


            if (
                mesesLocal <= 0 ||
                mesesLocal > 120
            ) {

                setError(
                    "El ciclo Local debe estar entre 1 y 120 meses."
                );

                return;
            }


            if (
                mesesForanea <= 0 ||
                mesesForanea > 120
            ) {

                setError(
                    "El ciclo Foránea debe estar entre 1 y 120 meses."
                );

                return;
            }


            try {

                setGuardandoPolitica(
                    true
                );

                setError(
                    ""
                );


                await actualizarPoliticaReemplazo({
                    mesesReemplazoLocal:
                        mesesLocal,

                    mesesReemplazoForanea:
                        mesesForanea
                });


                await cargarPolitica();


                setProyeccion(
                    null
                );


                setMensaje(
                    "Política de reemplazo actualizada correctamente."
                );


                setDialogoPolitica(
                    false
                );

            } catch (errorGuardar) {

                console.error(
                    "Error al guardar política:",
                    errorGuardar
                );


                setError(
                    errorGuardar
                        .response
                        ?.data
                        ?.mensaje ||
                    "No fue posible actualizar la política."
                );

            } finally {

                setGuardandoPolitica(
                    false
                );

            }

        };


    // =====================================================
    // CARGANDO INICIAL
    // =====================================================

    if (
        cargandoInicial
    ) {

        return (

            <Box
                sx={{
                    display:
                        "flex",

                    justifyContent:
                        "center",

                    alignItems:
                        "center",

                    minHeight:
                        350
                }}
            >

                <CircularProgress />

            </Box>

        );

    }


    // =====================================================
    // INTERFAZ
    // =====================================================

    return (

        <Box>

            {/* ================================================= */}
            {/* ENCABEZADO */}
            {/* ================================================= */}

            <Box
                sx={{
                    display:
                        "flex",

                    justifyContent:
                        "space-between",

                    alignItems: {
                        xs:
                            "flex-start",

                        md:
                            "center"
                    },

                    flexDirection: {
                        xs:
                            "column",

                        md:
                            "row"
                    },

                    gap:
                        2,

                    mb:
                        3
                }}
            >

                <Box>

                    <Typography
                        variant="h4"
                        fontWeight="bold"
                    >
                        Presupuesto de Baterías
                    </Typography>


                    <Typography
                        variant="body2"
                        color="text.secondary"
                        sx={{
                            mt:
                                0.5
                        }}
                    >
                        Proyección de reemplazo de baterías basada en el inventario de UPS.
                    </Typography>

                </Box>


                <Button
                    variant="contained"
                    startIcon={
                        <CalculateIcon />
                    }
                    onClick={
                        cargarProyeccion
                    }
                    disabled={
                        cargando
                    }
                >
                    Generar proyección
                </Button>

            </Box>


            {/* ================================================= */}
            {/* ERROR */}
            {/* ================================================= */}

            {error && (

                <Alert
                    severity="error"
                    sx={{
                        mb:
                            3
                    }}
                    onClose={() =>
                        setError("")
                    }
                >
                    {error}
                </Alert>

            )}


            {/* ================================================= */}
            {/* POLÍTICA DE REEMPLAZO */}
            {/* ================================================= */}

            <Card
                sx={{
                    mb:
                        3,

                    borderRadius:
                        2
                }}
            >

                <CardContent>

                    <Box
                        sx={{
                            display:
                                "flex",

                            justifyContent:
                                "space-between",

                            alignItems:
                                "center",

                            mb:
                                2
                        }}
                    >

                        <Box>

                            <Typography
                                variant="h6"
                                fontWeight="bold"
                            >
                                Política de reemplazo
                            </Typography>


                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                Frecuencia general utilizada para calcular el próximo cambio de baterías.
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


                    {politica ? (

                        <Grid
                            container
                            spacing={3}
                        >

                            <Grid
                                item
                                xs={12}
                                sm={6}
                            >

                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Agencias Locales
                                </Typography>


                                <Typography
                                    variant="h5"
                                    fontWeight="bold"
                                >
                                    {
                                        politica
                                            .mesesReemplazoLocal
                                    } meses
                                </Typography>

                            </Grid>


                            <Grid
                                item
                                xs={12}
                                sm={6}
                            >

                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Agencias Foráneas
                                </Typography>


                                <Typography
                                    variant="h5"
                                    fontWeight="bold"
                                >
                                    {
                                        politica
                                            .mesesReemplazoForanea
                                    } meses
                                </Typography>

                            </Grid>

                        </Grid>

                    ) : (

                        <Alert
                            severity="warning"
                        >
                            No existe una política de reemplazo configurada.
                        </Alert>

                    )}

                </CardContent>

            </Card>


            {/* ================================================= */}
            {/* AÑO Y COSTO */}
            {/* ================================================= */}

            <Card
                sx={{
                    mb:
                        3,

                    borderRadius:
                        2
                }}
            >

                <CardContent>

                    <Typography
                        variant="h6"
                        fontWeight="bold"
                        sx={{
                            mb:
                                2
                        }}
                    >
                        Proyección anual
                    </Typography>


                    <Grid
                        container
                        spacing={2}
                        alignItems="center"
                    >

                        <Grid
                            item
                            xs={12}
                            md={3}
                        >

                            <FormControl
                                fullWidth
                            >

                                <InputLabel>
                                    Año
                                </InputLabel>


                                <Select
                                    value={
                                        Number(anio)
                                    }
                                    label="Año"
                                    onChange={e =>
                                        cambiarAnio(
                                            e.target.value
                                        )
                                    }
                                >

                                    {aniosDisponibles.map(
                                        valor => (

                                            <MenuItem
                                                key={
                                                    valor
                                                }
                                                value={
                                                    valor
                                                }
                                            >
                                                {valor}
                                            </MenuItem>

                                        )
                                    )}

                                </Select>

                            </FormControl>

                        </Grid>


                        <Grid
                            item
                            xs={12}
                            md={4}
                        >

                            {configuracionActual ? (

                                <Box>

                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                    >
                                        Costo estimado por batería
                                    </Typography>


                                    <Typography
                                        variant="h5"
                                        fontWeight="bold"
                                    >
                                        {
                                            formatearMoneda(
                                                configuracionActual
                                                    .costoUnitarioBateria
                                            )
                                        }
                                    </Typography>

                                </Box>

                            ) : (

                                <Alert
                                    severity="warning"
                                >
                                    No hay costo configurado para {anio}.
                                </Alert>

                            )}

                        </Grid>


                        <Grid
                            item
                            xs={12}
                            md={5}
                        >

                            <Box
                                sx={{
                                    display:
                                        "flex",

                                    justifyContent: {
                                        xs:
                                            "stretch",

                                        md:
                                            "flex-end"
                                    },

                                    gap:
                                        1,

                                    flexWrap:
                                        "wrap"
                                }}
                            >

                                {puedeGestionar && (

                                    <Button
                                        variant="outlined"
                                        startIcon={
                                            <SettingsIcon />
                                        }
                                        onClick={
                                            abrirCosto
                                        }
                                    >

                                        {configuracionActual
                                            ? "Editar costo"
                                            : "Configurar costo"
                                        }

                                    </Button>

                                )}


                                <Button
                                    variant="outlined"
                                    startIcon={
                                        <RefreshIcon />
                                    }
                                    onClick={
                                        cargarProyeccion
                                    }
                                    disabled={
                                        cargando
                                    }
                                >
                                    Actualizar
                                </Button>

                            </Box>

                        </Grid>

                    </Grid>

                </CardContent>

            </Card>


            {/* ================================================= */}
            {/* CARGANDO */}
            {/* ================================================= */}

            {cargando && (

                <Box
                    sx={{
                        display:
                            "flex",

                        justifyContent:
                            "center",

                        alignItems:
                            "center",

                        py:
                            8
                    }}
                >

                    <Box
                        sx={{
                            textAlign:
                                "center"
                        }}
                    >

                        <CircularProgress />


                        <Typography
                            color="text.secondary"
                            sx={{
                                mt:
                                    2
                            }}
                        >
                            Calculando proyección...
                        </Typography>

                    </Box>

                </Box>

            )}


            {/* ================================================= */}
            {/* RESULTADO */}
            {/* ================================================= */}

            {!cargando &&
                proyeccion && (

                <>

                    {/* INDICADORES */}

                    <Grid
                        container
                        spacing={2}
                        sx={{
                            mb:
                                3
                        }}
                    >

                        <Grid
                            item
                            xs={12}
                            sm={6}
                            md={3}
                        >

                            <Card
                                sx={{
                                    height:
                                        "100%"
                                }}
                            >

                                <CardContent>

                                    <MemoryIcon
                                        color="primary"
                                    />


                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                        sx={{
                                            mt:
                                                1
                                        }}
                                    >
                                        UPS en inventario
                                    </Typography>


                                    <Typography
                                        variant="h4"
                                        fontWeight="bold"
                                    >
                                        {
                                            proyeccion
                                                .totalUPSInventario
                                        }
                                    </Typography>

                                </CardContent>

                            </Card>

                        </Grid>


                        <Grid
                            item
                            xs={12}
                            sm={6}
                            md={3}
                        >

                            <Card
                                sx={{
                                    height:
                                        "100%"
                                }}
                            >

                                <CardContent>

                                    <BatteryChargingFullIcon
                                        color="primary"
                                    />


                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                        sx={{
                                            mt:
                                                1
                                        }}
                                    >
                                        UPS con reemplazo
                                    </Typography>


                                    <Typography
                                        variant="h4"
                                        fontWeight="bold"
                                    >
                                        {
                                            proyeccion
                                                .totalUPSConReemplazo
                                        }
                                    </Typography>

                                </CardContent>

                            </Card>

                        </Grid>


                        <Grid
                            item
                            xs={12}
                            sm={6}
                            md={3}
                        >

                            <Card
                                sx={{
                                    height:
                                        "100%"
                                }}
                            >

                                <CardContent>

                                    <BatteryChargingFullIcon
                                        color="primary"
                                    />


                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                        sx={{
                                            mt:
                                                1
                                        }}
                                    >
                                        Baterías requeridas
                                    </Typography>


                                    <Typography
                                        variant="h4"
                                        fontWeight="bold"
                                    >
                                        {
                                            proyeccion
                                                .totalBaterias
                                        }
                                    </Typography>

                                </CardContent>

                            </Card>

                        </Grid>


                        <Grid
                            item
                            xs={12}
                            sm={6}
                            md={3}
                        >

                            <Card
                                sx={{
                                    height:
                                        "100%"
                                }}
                            >

                                <CardContent>

                                    <AccountBalanceIcon
                                        color="primary"
                                    />


                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                        sx={{
                                            mt:
                                                1
                                        }}
                                    >
                                        Presupuesto estimado
                                    </Typography>


                                    <Typography
                                        variant="h5"
                                        fontWeight="bold"
                                    >
                                        {
                                            formatearMoneda(
                                                proyeccion
                                                    .totalPresupuesto
                                            )
                                        }
                                    </Typography>

                                </CardContent>

                            </Card>

                        </Grid>

                    </Grid>


                    {/* RESUMEN POR AGENCIA */}

                    <Card
                        sx={{
                            mb:
                                3
                        }}
                    >

                        <CardContent>

                            <Box
                                sx={{
                                    display:
                                        "flex",

                                    justifyContent:
                                        "space-between",

                                    alignItems:
                                        "center",

                                    mb:
                                        2
                                }}
                            >

                                <Box>

                                    <Typography
                                        variant="h6"
                                        fontWeight="bold"
                                    >
                                        Resumen presupuestario {proyeccion.anio}
                                    </Typography>


                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                    >
                                        Distribución estimada por agencia
                                    </Typography>

                                </Box>


                                <BusinessIcon
                                    color="primary"
                                />

                            </Box>


                            <Divider
                                sx={{
                                    mb:
                                        2
                                }}
                            />


                            {proyeccion
                                .agencias
                                ?.length >
                                0 ? (

                                <TableContainer>

                                    <Table>

                                        <TableHead>

                                            <TableRow>

                                                <TableCell>
                                                    Agencia
                                                </TableCell>

                                                <TableCell>
                                                    Tipo
                                                </TableCell>

                                                <TableCell
                                                    align="right"
                                                >
                                                    UPS inventario
                                                </TableCell>

                                                <TableCell
                                                    align="right"
                                                >
                                                    UPS reemplazo
                                                </TableCell>

                                                <TableCell
                                                    align="right"
                                                >
                                                    Baterías
                                                </TableCell>

                                                <TableCell
                                                    align="right"
                                                >
                                                    Subtotal
                                                </TableCell>

                                            </TableRow>

                                        </TableHead>


                                        <TableBody>

                                            {proyeccion
                                                .agencias
                                                .map(
                                                    agencia => (

                                                        <TableRow
                                                            key={
                                                                agencia
                                                                    .agenciaId
                                                            }
                                                            hover
                                                        >

                                                            <TableCell>
                                                                {
                                                                    agencia
                                                                        .agencia
                                                                }
                                                            </TableCell>

                                                            <TableCell>
                                                                {
                                                                    agencia
                                                                        .tipoAgencia
                                                                }
                                                            </TableCell>

                                                            <TableCell
                                                                align="right"
                                                            >
                                                                {
                                                                    agencia
                                                                        .cantidadUPS
                                                                }
                                                            </TableCell>

                                                            <TableCell
                                                                align="right"
                                                            >
                                                                {
                                                                    agencia
                                                                        .cantidadUPSConReemplazo
                                                                }
                                                            </TableCell>

                                                            <TableCell
                                                                align="right"
                                                            >
                                                                {
                                                                    agencia
                                                                        .cantidadBaterias
                                                                }
                                                            </TableCell>

                                                            <TableCell
                                                                align="right"
                                                            >
                                                                <strong>
                                                                    {
                                                                        formatearMoneda(
                                                                            agencia
                                                                                .subtotal
                                                                        )
                                                                    }
                                                                </strong>
                                                            </TableCell>

                                                        </TableRow>

                                                    )
                                                )}

                                        </TableBody>

                                    </Table>

                                </TableContainer>

                            ) : (

                                <Alert
                                    severity="info"
                                >
                                    No existen UPS con reemplazo proyectado para {proyeccion.anio}.
                                </Alert>

                            )}

                        </CardContent>

                    </Card>


                    {/* ================================================= */}
                    {/* DETALLE DE UPS POR AGENCIA */}
                    {/* ================================================= */}

                    {proyeccion
                        .agencias
                        ?.map(
                            agencia => (

                                <Accordion
                                    key={
                                        agencia.agenciaId
                                    }
                                    sx={{
                                        mb: 1,
                                        borderRadius: 2,
                                        "&:before": {
                                            display: "none"
                                        }
                                    }}
                                >

                                    <AccordionSummary
                                        expandIcon={
                                            <ExpandMoreIcon />
                                        }
                                    >

                                        <Box
                                            sx={{
                                                display: "flex",
                                                justifyContent:
                                                    "space-between",
                                                alignItems: {
                                                    xs: "flex-start",
                                                    md: "center"
                                                },
                                                width: "100%",
                                                pr: 2,
                                                flexDirection: {
                                                    xs: "column",
                                                    md: "row"
                                                },
                                                gap: 1
                                            }}
                                        >

                                            <Box>

                                                <Typography
                                                    fontWeight="bold"
                                                >
                                                    {
                                                        agencia.agencia
                                                    }
                                                </Typography>

                                                <Typography
                                                    variant="body2"
                                                    color="text.secondary"
                                                >
                                                    {
                                                        agencia.tipoAgencia
                                                    }
                                                    {" · "}
                                                    {
                                                        agencia
                                                            .cantidadUPSConReemplazo
                                                    }
                                                    {" UPS · "}
                                                    {
                                                        agencia
                                                            .cantidadBaterias
                                                    }
                                                    {" baterías"}
                                                </Typography>

                                            </Box>

                                            <Typography
                                                fontWeight="bold"
                                            >
                                                {
                                                    formatearMoneda(
                                                        agencia.subtotal
                                                    )
                                                }
                                            </Typography>

                                        </Box>

                                    </AccordionSummary>


                                    <AccordionDetails>

                                        {Array.isArray(
                                            agencia.ups
                                        ) &&
                                        agencia.ups.length > 0 ? (

                                            <TableContainer>

                                                <Table
                                                    size="small"
                                                >

                                                    <TableHead>

                                                        <TableRow>

                                                            <TableCell>
                                                                Código UPS
                                                            </TableCell>

                                                            <TableCell>
                                                                Serie
                                                            </TableCell>

                                                            <TableCell>
                                                                Marca / Modelo
                                                            </TableCell>

                                                            <TableCell>
                                                                Estado
                                                            </TableCell>

                                                            <TableCell
                                                                align="right"
                                                            >
                                                                Baterías
                                                            </TableCell>

                                                            <TableCell>
                                                                Último cambio
                                                            </TableCell>

                                                            <TableCell>
                                                                Fecha proyectada
                                                            </TableCell>

                                                            <TableCell
                                                                align="right"
                                                            >
                                                                Ciclo
                                                            </TableCell>

                                                            <TableCell
                                                                align="right"
                                                            >
                                                                Costo unitario
                                                            </TableCell>

                                                            <TableCell
                                                                align="right"
                                                            >
                                                                Costo estimado
                                                            </TableCell>

                                                        </TableRow>

                                                    </TableHead>


                                                    <TableBody>

                                                        {agencia.ups.map(
                                                            equipo => (

                                                                <TableRow
                                                                    key={
                                                                        equipo
                                                                            .upsId
                                                                    }
                                                                    hover
                                                                >

                                                                    <TableCell>

                                                                        <Typography
                                                                            fontWeight="bold"
                                                                        >
                                                                            {
                                                                                equipo
                                                                                    .codigoUPS
                                                                            }
                                                                        </Typography>

                                                                    </TableCell>


                                                                    <TableCell>
                                                                        {
                                                                            equipo
                                                                                .numeroSerie ||
                                                                            "—"
                                                                        }
                                                                    </TableCell>


                                                                    <TableCell>

                                                                        <Typography
                                                                            variant="body2"
                                                                            fontWeight="bold"
                                                                        >
                                                                            {
                                                                                equipo
                                                                                    .marca
                                                                            }
                                                                        </Typography>

                                                                        <Typography
                                                                            variant="caption"
                                                                            color="text.secondary"
                                                                        >
                                                                            {
                                                                                equipo
                                                                                    .modelo
                                                                            }
                                                                        </Typography>

                                                                    </TableCell>


                                                                    <TableCell>
                                                                        {
                                                                            equipo
                                                                                .estadoUPS ||
                                                                            "—"
                                                                        }
                                                                    </TableCell>


                                                                    <TableCell
                                                                        align="right"
                                                                    >
                                                                        {
                                                                            equipo
                                                                                .cantidadBaterias
                                                                        }
                                                                    </TableCell>


                                                                    <TableCell>
                                                                        {
                                                                            formatearFecha(
                                                                                equipo
                                                                                    .fechaUltimoCambioBaterias
                                                                            )
                                                                        }
                                                                    </TableCell>


                                                                    <TableCell>
                                                                        {
                                                                            formatearFecha(
                                                                                equipo
                                                                                    .fechaProyectadaReemplazo
                                                                            )
                                                                        }
                                                                    </TableCell>


                                                                    <TableCell
                                                                        align="right"
                                                                    >
                                                                        {
                                                                            equipo
                                                                                .mesesCicloReemplazo
                                                                        }{" "}
                                                                        meses
                                                                    </TableCell>


                                                                    <TableCell
                                                                        align="right"
                                                                    >
                                                                        {
                                                                            formatearMoneda(
                                                                                equipo
                                                                                    .costoUnitarioBateria
                                                                            )
                                                                        }
                                                                    </TableCell>


                                                                    <TableCell
                                                                        align="right"
                                                                    >

                                                                        <Typography
                                                                            fontWeight="bold"
                                                                        >
                                                                            {
                                                                                formatearMoneda(
                                                                                    equipo
                                                                                        .costoEstimado
                                                                                )
                                                                            }
                                                                        </Typography>

                                                                    </TableCell>

                                                                </TableRow>

                                                            )
                                                        )}

                                                    </TableBody>

                                                </Table>

                                            </TableContainer>

                                        ) : (

                                            <Alert
                                                severity="info"
                                            >
                                                No hay detalle de UPS para esta agencia.
                                            </Alert>

                                        )}

                                    </AccordionDetails>

                                </Accordion>

                            )
                        )}


                    {/* INFORMACIÓN INSUFICIENTE */}

                    {proyeccion
                        .upsInformacionInsuficiente >
                        0 && (

                        <Card
                            sx={{
                                mt:
                                    3
                            }}
                        >

                            <CardContent>

                                <Box
                                    sx={{
                                        display:
                                            "flex",

                                        alignItems:
                                            "center",

                                        gap:
                                            1,

                                        mb:
                                            2
                                    }}
                                >

                                    <WarningAmberIcon
                                        color="warning"
                                    />


                                    <Typography
                                        variant="h6"
                                        fontWeight="bold"
                                    >
                                        Información insuficiente
                                    </Typography>

                                </Box>


                                <Alert
                                    severity="warning"
                                >
                                    Hay{" "}
                                    {
                                        proyeccion
                                            .upsInformacionInsuficiente
                                    }{" "}
                                    UPS que no pueden proyectarse.
                                </Alert>

                            </CardContent>

                        </Card>

                    )}

                </>

            )}


            {/* ================================================= */}
            {/* ESTADO INICIAL */}
            {/* ================================================= */}

            {!cargando &&
                !proyeccion &&
                !error && (

                <Card>

                    <CardContent
                        sx={{
                            textAlign:
                                "center",

                            py:
                                8
                        }}
                    >

                        <AccountBalanceIcon
                            sx={{
                                fontSize:
                                    60,

                                opacity:
                                    0.35
                            }}
                        />


                        <Typography
                            variant="h6"
                            fontWeight="bold"
                            sx={{
                                mt:
                                    2
                            }}
                        >
                            Proyección {anio}
                        </Typography>


                        <Typography
                            color="text.secondary"
                            sx={{
                                maxWidth:
                                    600,

                                mx:
                                    "auto",

                                mt:
                                    1,

                                mb:
                                    3
                            }}
                        >
                            Seleccione el año, configure el costo estimado de la batería y genere la proyección.
                        </Typography>


                        <Button
                            variant="contained"
                            startIcon={
                                <CalculateIcon />
                            }
                            onClick={
                                cargarProyeccion
                            }
                        >
                            Generar proyección
                        </Button>

                    </CardContent>

                </Card>

            )}


            {/* ================================================= */}
            {/* DIÁLOGO COSTO ANUAL */}
            {/* ================================================= */}

            <Dialog
                open={
                    dialogoCosto
                }
                onClose={
                    guardandoCosto
                        ? undefined
                        : cerrarCosto
                }
                fullWidth
                maxWidth="sm"
            >

                <DialogTitle>

                    {configuracionSeleccionada
                        ? `Editar costo estimado ${anio}`
                        : `Configurar costo estimado ${anio}`
                    }

                </DialogTitle>


                <DialogContent>

                    <Box
                        sx={{
                            pt:
                                1
                        }}
                    >

                        <TextField
                            fullWidth
                            label="Año"
                            value={
                                formularioCosto
                                    .anio
                            }
                            disabled
                            margin="normal"
                        />


                        <TextField
                            fullWidth
                            required
                            label="Costo estimado por batería"
                            type="number"
                            margin="normal"
                            value={
                                formularioCosto
                                    .costoUnitarioBateria
                            }
                            onChange={e =>
                                setFormularioCosto(
                                    actual => ({
                                        ...actual,

                                        costoUnitarioBateria:
                                            e.target.value
                                    })
                                )
                            }
                            inputProps={{
                                min:
                                    0,

                                step:
                                    "0.01"
                            }}
                            helperText="Costo unitario que se utilizará para calcular el presupuesto de este año."
                        />

                    </Box>

                </DialogContent>


                <DialogActions
                    sx={{
                        px:
                            3,

                        pb:
                            2
                    }}
                >

                    <Button
                        onClick={
                            cerrarCosto
                        }
                        disabled={
                            guardandoCosto
                        }
                    >
                        Cancelar
                    </Button>


                    <Button
                        variant="contained"
                        startIcon={
                            guardandoCosto
                                ? (
                                    <CircularProgress
                                        size={18}
                                        color="inherit"
                                    />
                                )
                                : (
                                    <SaveIcon />
                                )
                        }
                        onClick={
                            guardarCosto
                        }
                        disabled={
                            guardandoCosto
                        }
                    >
                        Guardar
                    </Button>

                </DialogActions>

            </Dialog>


            {/* ================================================= */}
            {/* DIÁLOGO POLÍTICA */}
            {/* ================================================= */}

            <Dialog
                open={
                    dialogoPolitica
                }
                onClose={
                    guardandoPolitica
                        ? undefined
                        : cerrarPolitica
                }
                fullWidth
                maxWidth="sm"
            >

                <DialogTitle>
                    Política de reemplazo de baterías
                </DialogTitle>


                <DialogContent>

                    <Box
                        sx={{
                            pt:
                                1
                        }}
                    >

                        <Alert
                            severity="info"
                            sx={{
                                mb:
                                    2
                            }}
                        >
                            Esta política se aplica a todos los años de proyección.
                        </Alert>


                        <TextField
                            fullWidth
                            required
                            margin="normal"
                            label="Ciclo Local (meses)"
                            type="number"
                            value={
                                formularioPolitica
                                    .mesesReemplazoLocal
                            }
                            onChange={e =>
                                setFormularioPolitica(
                                    actual => ({
                                        ...actual,

                                        mesesReemplazoLocal:
                                            e.target.value
                                    })
                                )
                            }
                            inputProps={{
                                min:
                                    1,

                                max:
                                    120
                            }}
                            helperText="Tiempo estimado entre cambios completos de baterías para agencias Locales."
                        />


                        <TextField
                            fullWidth
                            required
                            margin="normal"
                            label="Ciclo Foránea (meses)"
                            type="number"
                            value={
                                formularioPolitica
                                    .mesesReemplazoForanea
                            }
                            onChange={e =>
                                setFormularioPolitica(
                                    actual => ({
                                        ...actual,

                                        mesesReemplazoForanea:
                                            e.target.value
                                    })
                                )
                            }
                            inputProps={{
                                min:
                                    1,

                                max:
                                    120
                            }}
                            helperText="Tiempo estimado entre cambios completos de baterías para agencias Foráneas."
                        />

                    </Box>

                </DialogContent>


                <DialogActions
                    sx={{
                        px:
                            3,

                        pb:
                            2
                    }}
                >

                    <Button
                        onClick={
                            cerrarPolitica
                        }
                        disabled={
                            guardandoPolitica
                        }
                    >
                        Cancelar
                    </Button>


                    <Button
                        variant="contained"
                        startIcon={
                            guardandoPolitica
                                ? (
                                    <CircularProgress
                                        size={18}
                                        color="inherit"
                                    />
                                )
                                : (
                                    <SaveIcon />
                                )
                        }
                        onClick={
                            guardarPolitica
                        }
                        disabled={
                            guardandoPolitica
                        }
                    >
                        Guardar
                    </Button>

                </DialogActions>

            </Dialog>


            {/* ================================================= */}
            {/* MENSAJE */}
            {/* ================================================= */}

            <Snackbar
                open={
                    Boolean(
                        mensaje
                    )
                }
                autoHideDuration={
                    4000
                }
                onClose={() =>
                    setMensaje("")
                }
                message={
                    mensaje
                }
            />

        </Box>

    );
}
import { useEffect, useState } from "react";

import {
    Alert,
    Autocomplete,
    Box,
    Button,
    Checkbox,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControlLabel,
    MenuItem,
    Paper,
    TextField,
    Typography
} from "@mui/material";

import { obtenerUPS } from "../../services/upsService";

import {
    crearMantenimiento,
    actualizarMantenimiento
} from "../../services/mantenimientoService";

import {
    obtenerUsuarios
} from "../../services/usuarioService";

import {
    obtenerTiposMantenimiento,
    obtenerEstadosMantenimiento
} from "../../services/catalogosService";


const formularioInicial = {
    upsId: "",
    tipoMantenimientoId: "",
    fechaMantenimiento: "",
    descripcion: "",
    tecnicoProveedor: "",
    costo: "",
    estadoMantenimientoId: "",
    observaciones: "",
    incluyeCambioBaterias: false,
    costoUnitarioBateria: ""
};


const obtenerFechaActual = () => {

    const fecha = new Date();

    const anio =
        fecha.getFullYear();

    const mes =
        String(
            fecha.getMonth() + 1
        ).padStart(2, "0");

    const dia =
        String(
            fecha.getDate()
        ).padStart(2, "0");

    return `${anio}-${mes}-${dia}`;
};


const formatearMoneda = (valor) => {

    return Number(
        valor || 0
    ).toLocaleString(
        "es-HN",
        {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }
    );
};


export default function NuevoMantenimientoModal({
    open,
    onClose,
    onGuardado,
    mantenimientoEditar = null
}) {

    const [ups, setUps] =
        useState([]);

    const [
        upsSeleccionado,
        setUpsSeleccionado
    ] = useState(null);

    const [
        tiposMantenimiento,
        setTiposMantenimiento
    ] = useState([]);

    const [
        estadosMantenimiento,
        setEstadosMantenimiento
    ] = useState([]);

    const [
        usuariosTecnicos,
        setUsuariosTecnicos
    ] = useState([]);

    const [
        usuarioTecnicoSeleccionado,
        setUsuarioTecnicoSeleccionado
    ] = useState(null);

    const [
        formulario,
        setFormulario
    ] = useState(formularioInicial);

    const [
        guardando,
        setGuardando
    ] = useState(false);

    const [
        cargando,
        setCargando
    ] = useState(false);

    const [
        cargandoUsuarios,
        setCargandoUsuarios
    ] = useState(false);

    const [
        error,
        setError
    ] = useState("");


    const rol =
        localStorage.getItem("rol") || "";

    const nombreCompleto =
        localStorage.getItem(
            "nombreCompleto"
        ) || "Usuario actual";

    const esAdministrador =
        rol === "Administrador" ||
        rol === "Control Total";

    const esTecnico =
        rol === "Técnico";


    // =====================================================
    // ESTADO
    // =====================================================

    const estadoSeleccionado =
        estadosMantenimiento.find(
            estado =>
                Number(estado.id) ===
                Number(
                    formulario.estadoMantenimientoId
                )
        );

    const esProgramado =
        estadoSeleccionado
            ?.nombre
            ?.trim()
            .toLowerCase() ===
        "programado";


    // =====================================================
    // CÁLCULO DE BATERÍAS
    // =====================================================

    const cantidadBateriasUPS =
        Number(
            upsSeleccionado
                ?.cantidadBaterias ||
            0
        );

    const costoUnitarioBateria =
        Number(
            formulario
                .costoUnitarioBateria ||
            0
        );

    const costoTotalBaterias =
        cantidadBateriasUPS *
        costoUnitarioBateria;


    // =====================================================
    // CARGAR DATOS
    // =====================================================

    useEffect(() => {

        if (!open) {
            return;
        }

        setError("");

        cargarDatos();

    }, [
        open,
        mantenimientoEditar
    ]);


    const cargarDatos = async () => {

        try {

            setCargando(true);

            const [
                upsData,
                tiposData,
                estadosData
            ] = await Promise.all([
                obtenerUPS(),
                obtenerTiposMantenimiento(),
                obtenerEstadosMantenimiento()
            ]);


            setUps(
                upsData || []
            );

            setTiposMantenimiento(
                tiposData || []
            );

            setEstadosMantenimiento(
                estadosData || []
            );


            // =================================================
            // USUARIOS
            // =================================================

            if (esAdministrador) {

                try {

                    setCargandoUsuarios(
                        true
                    );

                    const usuarios =
                        await obtenerUsuarios();

                    const activos =
                        (usuarios || [])
                            .filter(
                                usuario =>
                                    usuario.activo !==
                                    false
                            );

                    setUsuariosTecnicos(
                        activos
                    );

                } catch (errorUsuarios) {

                    console.error(
                        "Error al cargar usuarios:",
                        errorUsuarios
                    );

                    setUsuariosTecnicos([]);

                } finally {

                    setCargandoUsuarios(
                        false
                    );
                }

            } else {

                setUsuariosTecnicos([]);
            }


            // =================================================
            // EDICIÓN
            // =================================================

            if (mantenimientoEditar) {

                const upsEncontrado =
                    (upsData || []).find(
                        equipo =>
                            Number(equipo.id) ===
                            Number(
                                mantenimientoEditar.upsId
                            )
                    );


                setUpsSeleccionado(
                    upsEncontrado || null
                );


                const tipoId =
                    mantenimientoEditar
                        .tipoMantenimientoId ??
                    mantenimientoEditar
                        .tipoMantenimiento?.id ??
                    "";


                const estadoId =
                    mantenimientoEditar
                        .estadoMantenimientoId ??
                    mantenimientoEditar
                        .estadoMantenimiento?.id ??
                    "";


                // =============================================
                // RECUPERAR COSTO UNITARIO
                //
                // En BD tenemos:
                // CostoBaterias = COSTO TOTAL
                //
                // Por eso:
                //
                // costo unitario =
                // costo total / cantidad reemplazada
                // =============================================

                const cantidadHistorica =
                    Number(
                        mantenimientoEditar
                            .cantidadBateriasReemplazadas ||
                        0
                    );

                const costoTotalHistorico =
                    Number(
                        mantenimientoEditar
                            .costoBaterias ||
                        0
                    );


                let costoUnitarioCalculado =
                    "";


                if (
                    mantenimientoEditar
                        .incluyeCambioBaterias &&
                    cantidadHistorica > 0
                ) {

                    costoUnitarioCalculado =
                        costoTotalHistorico /
                        cantidadHistorica;
                }


                setFormulario({

                    upsId:
                        mantenimientoEditar.upsId
                            ? String(
                                mantenimientoEditar.upsId
                            )
                            : "",

                    tipoMantenimientoId:
                        tipoId
                            ? String(tipoId)
                            : "",

                    fechaMantenimiento:
                        mantenimientoEditar
                            .fechaMantenimiento
                            ? mantenimientoEditar
                                .fechaMantenimiento
                                .substring(0, 10)
                            : "",

                    descripcion:
                        mantenimientoEditar
                            .descripcion || "",

                    tecnicoProveedor:
                        mantenimientoEditar
                            .tecnicoProveedor || "",

                    costo:
                        mantenimientoEditar
                            .costo ?? "",

                    estadoMantenimientoId:
                        estadoId
                            ? String(estadoId)
                            : "",

                    observaciones:
                        mantenimientoEditar
                            .observaciones || "",

                    incluyeCambioBaterias:
                        mantenimientoEditar
                            .incluyeCambioBaterias ===
                        true,

                    costoUnitarioBateria:
                        mantenimientoEditar
                            .incluyeCambioBaterias
                            ? costoUnitarioCalculado
                            : ""
                });


                setUsuarioTecnicoSeleccionado(
                    null
                );

            } else {

                setUpsSeleccionado(
                    null
                );

                setUsuarioTecnicoSeleccionado(
                    null
                );

                setFormulario({
                    ...formularioInicial,

                    tecnicoProveedor:
                        esTecnico
                            ? nombreCompleto
                            : ""
                });
            }

        } catch (errorCarga) {

            console.error(
                "Error al cargar datos de mantenimiento:",
                errorCarga
            );

            setError(
                "No se pudieron cargar los datos necesarios."
            );

        } finally {

            setCargando(false);
        }
    };


    // =====================================================
    // TÉCNICO AL EDITAR
    // =====================================================

    useEffect(() => {

        if (
            !mantenimientoEditar ||
            !esAdministrador ||
            usuariosTecnicos.length === 0
        ) {
            return;
        }


        const tecnicoActual =
            mantenimientoEditar
                .tecnicoProveedor;


        const usuario =
            usuariosTecnicos.find(
                item =>
                    item.nombreCompleto ===
                        tecnicoActual ||
                    item.nombreUsuario ===
                        tecnicoActual
            );


        setUsuarioTecnicoSeleccionado(
            usuario || null
        );

    }, [
        usuariosTecnicos,
        mantenimientoEditar,
        esAdministrador
    ]);


    // =====================================================
    // CAMBIAR CAMPO
    // =====================================================

    const cambiarCampo = (
        campo,
        valor
    ) => {

        setFormulario(
            previo => ({
                ...previo,
                [campo]: valor
            })
        );

        if (error) {
            setError("");
        }
    };


    // =====================================================
    // CAMBIO DE BATERÍAS
    // =====================================================

    const cambiarCambioBaterias =
        marcado => {

            setFormulario(
                previo => ({
                    ...previo,

                    incluyeCambioBaterias:
                        marcado,

                    costoUnitarioBateria:
                        marcado
                            ? previo
                                .costoUnitarioBateria
                            : ""
                })
            );

            if (error) {
                setError("");
            }
        };


    // =====================================================
    // GUARDAR
    // =====================================================

    const guardar = async () => {

        setError("");


        if (!upsSeleccionado) {

            setError(
                "Debe seleccionar un UPS."
            );

            return;
        }


        if (!formulario.tipoMantenimientoId) {

            setError(
                "Debe seleccionar el tipo de mantenimiento."
            );

            return;
        }


        if (!formulario.fechaMantenimiento) {

            setError(
                "Debe seleccionar la fecha del mantenimiento."
            );

            return;
        }


        if (!formulario.estadoMantenimientoId) {

            setError(
                "Debe seleccionar el estado del mantenimiento."
            );

            return;
        }


        // =================================================
        // FECHA
        // =================================================

        const fechaActual =
            obtenerFechaActual();

        const fechaEsFutura =
            formulario.fechaMantenimiento >
            fechaActual;


        if (
            fechaEsFutura &&
            !esProgramado
        ) {

            setError(
                "Una fecha futura solamente puede utilizarse cuando el estado del mantenimiento es Programado."
            );

            return;
        }


        if (
            !formulario
                .descripcion
                .trim()
        ) {

            setError(
                "Debe ingresar una descripción."
            );

            return;
        }


        // =================================================
        // TÉCNICO
        // =================================================

        let tecnicoProveedor =
            formulario
                .tecnicoProveedor
                .trim();


        if (esTecnico) {

            tecnicoProveedor =
                nombreCompleto.trim();
        }


        if (!tecnicoProveedor) {

            setError(
                "Debe indicar el técnico o proveedor responsable."
            );

            return;
        }


        // =================================================
        // COSTO SERVICIO
        // =================================================

        if (
            formulario.costo === "" ||
            Number(formulario.costo) < 0
        ) {

            setError(
                "Ingrese un costo válido."
            );

            return;
        }


        // =================================================
        // BATERÍAS
        // =================================================

        if (
            formulario
                .incluyeCambioBaterias
        ) {

            if (
                cantidadBateriasUPS <= 0
            ) {

                setError(
                    "El UPS no tiene una cantidad válida de baterías registrada."
                );

                return;
            }


            if (
                formulario
                    .costoUnitarioBateria === "" ||
                Number(
                    formulario
                        .costoUnitarioBateria
                ) < 0
            ) {

                setError(
                    "Ingrese un costo unitario válido para las baterías."
                );

                return;
            }
        }


        // =================================================
        // DATOS
        // =================================================

        try {

            setGuardando(true);


            const datos = {

                id:
                    mantenimientoEditar
                        ?.id ?? 0,

                upsId:
                    Number(
                        upsSeleccionado.id
                    ),

                tipoMantenimientoId:
                    Number(
                        formulario
                            .tipoMantenimientoId
                    ),

                estadoMantenimientoId:
                    Number(
                        formulario
                            .estadoMantenimientoId
                    ),

                fechaMantenimiento:
                    `${formulario.fechaMantenimiento}T00:00:00`,

                descripcion:
                    formulario
                        .descripcion
                        .trim(),

                tecnicoProveedor:
                    tecnicoProveedor,

                costo:
                    Number(
                        formulario.costo
                    ),

                observaciones:
                    formulario
                        .observaciones
                        .trim(),

                incluyeCambioBaterias:
                    formulario
                        .incluyeCambioBaterias,

                cantidadBateriasReemplazadas:
                    formulario
                        .incluyeCambioBaterias
                        ? cantidadBateriasUPS
                        : 0,

                // =============================================
                // GUARDAMOS EL TOTAL
                // =============================================

                costoBaterias:
                    formulario
                        .incluyeCambioBaterias
                        ? costoTotalBaterias
                        : 0
            };


            if (mantenimientoEditar) {

                await actualizarMantenimiento(
                    mantenimientoEditar.id,
                    datos
                );

            } else {

                await crearMantenimiento(
                    datos
                );
            }


            await onGuardado();

            cerrar();

        } catch (errorGuardar) {

            console.error(
                "Error al guardar mantenimiento:",
                errorGuardar
            );


            const mensaje =
                errorGuardar
                    .response
                    ?.data
                    ?.mensaje ||
                errorGuardar
                    .response
                    ?.data
                    ?.message ||
                "No se pudo guardar el mantenimiento.";


            setError(
                mensaje
            );

        } finally {

            setGuardando(false);
        }
    };


    // =====================================================
    // CERRAR
    // =====================================================

    const cerrar = () => {

        if (guardando) {
            return;
        }

        setUpsSeleccionado(
            null
        );

        setUsuarioTecnicoSeleccionado(
            null
        );

        setFormulario({
            ...formularioInicial
        });

        setError("");

        onClose();
    };


    // =====================================================
    // INTERFAZ
    // =====================================================

    return (

        <Dialog
            open={open}
            onClose={
                guardando
                    ? undefined
                    : cerrar
            }
            fullWidth
            maxWidth="md"
        >

            <DialogTitle
                sx={{
                    fontWeight: "bold"
                }}
            >
                {mantenimientoEditar
                    ? "Editar mantenimiento"
                    : "Nuevo mantenimiento"}
            </DialogTitle>


            <DialogContent>

                {error && (

                    <Alert
                        severity="error"
                        sx={{
                            mt: 1,
                            mb: 2
                        }}
                    >
                        {error}
                    </Alert>

                )}


                <Box
                    sx={{
                        display: "grid",

                        gridTemplateColumns:
                            "repeat(2, minmax(0, 1fr))",

                        gap: 2,

                        mt: 1
                    }}
                >

                    {/* UPS */}

                    <Box
                        sx={{
                            gridColumn: "1 / -1"
                        }}
                    >

                        <Autocomplete
                            fullWidth
                            options={ups}
                            value={upsSeleccionado}
                            loading={cargando}
                            disabled={
                                cargando ||
                                guardando
                            }
                            onChange={(
                                event,
                                nuevoValor
                            ) => {

                                setUpsSeleccionado(
                                    nuevoValor
                                );

                                cambiarCampo(
                                    "upsId",
                                    nuevoValor
                                        ? nuevoValor.id
                                        : ""
                                );
                            }}
                            getOptionLabel={
                                option =>
                                    `${option.codigo || ""} - ${option.numeroSerie || ""} - ${option.marca || ""} ${option.modelo || ""}`
                            }
                            isOptionEqualToValue={
                                (
                                    option,
                                    value
                                ) =>
                                    Number(option.id) ===
                                    Number(value.id)
                            }
                            renderInput={
                                params => (

                                    <TextField
                                        {...params}
                                        label="UPS"
                                        placeholder="Buscar por código, serie o equipo..."
                                        required
                                    />

                                )
                            }
                        />

                    </Box>


                    {/* TIPO */}

                    <TextField
                        select
                        fullWidth
                        required
                        label="Tipo de mantenimiento"
                        value={
                            formulario
                                .tipoMantenimientoId
                        }
                        onChange={e =>
                            cambiarCampo(
                                "tipoMantenimientoId",
                                e.target.value
                            )
                        }
                        disabled={
                            cargando ||
                            guardando
                        }
                    >

                        <MenuItem value="">
                            Seleccione...
                        </MenuItem>

                        {tiposMantenimiento.map(
                            tipo => (

                                <MenuItem
                                    key={tipo.id}
                                    value={tipo.id}
                                >
                                    {tipo.nombre}
                                </MenuItem>

                            )
                        )}

                    </TextField>


                    {/* FECHA */}

                    <Box>

                        <Box
                            component="label"
                            sx={{
                                display: "block",
                                fontSize: "0.9rem",
                                color: "text.secondary",
                                mb: 0.5
                            }}
                        >
                            Fecha de mantenimiento *
                        </Box>

                        <TextField
                            fullWidth
                            required
                            type="date"
                            value={
                                formulario
                                    .fechaMantenimiento
                            }
                            onChange={e =>
                                cambiarCampo(
                                    "fechaMantenimiento",
                                    e.target.value
                                )
                            }
                            disabled={guardando}
                            inputProps={
                                esProgramado
                                    ? {}
                                    : {
                                        max:
                                            obtenerFechaActual()
                                    }
                            }
                        />

                    </Box>


                    {/* DESCRIPCIÓN */}

                    <Box
                        sx={{
                            gridColumn: "1 / -1"
                        }}
                    >

                        <TextField
                            fullWidth
                            required
                            multiline
                            rows={4}
                            label="Descripción"
                            value={
                                formulario
                                    .descripcion
                            }
                            onChange={e =>
                                cambiarCampo(
                                    "descripcion",
                                    e.target.value
                                )
                            }
                            disabled={guardando}
                        />

                    </Box>


                    {/* TÉCNICO */}

                    <Box>

                        {esAdministrador ? (

                            <Autocomplete
                                fullWidth
                                options={
                                    usuariosTecnicos
                                }
                                value={
                                    usuarioTecnicoSeleccionado
                                }
                                loading={
                                    cargandoUsuarios
                                }
                                disabled={
                                    guardando ||
                                    cargandoUsuarios
                                }
                                onChange={(
                                    event,
                                    nuevoValor
                                ) => {

                                    setUsuarioTecnicoSeleccionado(
                                        nuevoValor
                                    );

                                    cambiarCampo(
                                        "tecnicoProveedor",
                                        nuevoValor
                                            ? (
                                                nuevoValor
                                                    .nombreCompleto ||
                                                nuevoValor
                                                    .nombreUsuario ||
                                                ""
                                            )
                                            : ""
                                    );
                                }}
                                getOptionLabel={
                                    usuario =>
                                        `${usuario.nombreCompleto || ""} (${usuario.nombreUsuario || ""})`
                                }
                                isOptionEqualToValue={
                                    (
                                        option,
                                        value
                                    ) =>
                                        Number(option.id) ===
                                        Number(value.id)
                                }
                                renderInput={
                                    params => (

                                        <TextField
                                            {...params}
                                            label="Técnico / proveedor"
                                            required
                                        />

                                    )
                                }
                            />

                        ) : (

                            <TextField
                                fullWidth
                                required
                                label="Técnico / proveedor"
                                value={
                                    nombreCompleto
                                }
                                disabled
                            />

                        )}

                    </Box>


                    {/* COSTO SERVICIO */}

                    <TextField
                        fullWidth
                        label="Costo del mantenimiento"
                        type="number"
                        value={
                            formulario.costo
                        }
                        onChange={e =>
                            cambiarCampo(
                                "costo",
                                e.target.value
                            )
                        }
                        disabled={guardando}
                        inputProps={{
                            min: 0,
                            step: "0.01"
                        }}
                    />


                    {/* ESTADO */}

                    <TextField
                        select
                        fullWidth
                        required
                        label="Estado"
                        value={
                            formulario
                                .estadoMantenimientoId
                        }
                        onChange={e =>
                            cambiarCampo(
                                "estadoMantenimientoId",
                                e.target.value
                            )
                        }
                        disabled={
                            cargando ||
                            guardando
                        }
                    >

                        <MenuItem value="">
                            Seleccione...
                        </MenuItem>

                        {estadosMantenimiento.map(
                            estado => (

                                <MenuItem
                                    key={estado.id}
                                    value={estado.id}
                                >
                                    {estado.nombre}
                                </MenuItem>

                            )
                        )}

                    </TextField>


                    {/* CAMBIO DE BATERÍAS */}

                    <Paper
                        variant="outlined"
                        sx={{
                            gridColumn: "1 / -1",
                            p: 2.5,
                            mt: 0.5
                        }}
                    >

                        <Typography
                            variant="subtitle1"
                            fontWeight="bold"
                        >
                            Cambio de baterías
                        </Typography>


                        <FormControlLabel
                            sx={{ mt: 1 }}
                            control={

                                <Checkbox
                                    checked={
                                        formulario
                                            .incluyeCambioBaterias
                                    }
                                    onChange={e =>
                                        cambiarCambioBaterias(
                                            e.target.checked
                                        )
                                    }
                                    disabled={guardando}
                                />

                            }
                            label={
                                esProgramado
                                    ? "Este mantenimiento contempla cambio de baterías"
                                    : "Se realizó cambio de baterías"
                            }
                        />


                        {formulario
                            .incluyeCambioBaterias && (

                            <Box
                                sx={{
                                    mt: 2,

                                    display: "grid",

                                    gridTemplateColumns:
                                        "repeat(2, minmax(0, 1fr))",

                                    gap: 2
                                }}
                            >

                                {/* CANTIDAD */}

                                <TextField
                                    fullWidth
                                    label={
                                        esProgramado
                                            ? "Baterías a reemplazar"
                                            : "Baterías reemplazadas"
                                    }
                                    value={
                                        cantidadBateriasUPS ||
                                        ""
                                    }
                                    disabled
                                    helperText="Se reemplaza el banco completo registrado en el UPS."
                                />


                                {/* COSTO UNITARIO */}

                                <TextField
                                    fullWidth
                                    required
                                    type="number"
                                    label="Costo unitario por batería"
                                    value={
                                        formulario
                                            .costoUnitarioBateria
                                    }
                                    onChange={e =>
                                        cambiarCampo(
                                            "costoUnitarioBateria",
                                            e.target.value
                                        )
                                    }
                                    disabled={guardando}
                                    inputProps={{
                                        min: 0,
                                        step: "0.01"
                                    }}
                                />


                                {/* RESUMEN */}

                                <Paper
                                    variant="outlined"
                                    sx={{
                                        gridColumn:
                                            "1 / -1",

                                        p: 2
                                    }}
                                >

                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                    >
                                        Cálculo del cambio
                                    </Typography>


                                    <Typography
                                        sx={{
                                            mt: 0.5
                                        }}
                                    >
                                        {cantidadBateriasUPS} baterías
                                        {" × "}
                                        L{" "}
                                        {formatearMoneda(
                                            costoUnitarioBateria
                                        )}
                                    </Typography>


                                    <Typography
                                        variant="h6"
                                        fontWeight="bold"
                                        sx={{
                                            mt: 1
                                        }}
                                    >
                                        Total: L{" "}
                                        {formatearMoneda(
                                            costoTotalBaterias
                                        )}
                                    </Typography>

                                </Paper>

                            </Box>

                        )}

                    </Paper>


                    {/* OBSERVACIONES */}

                    <Box
                        sx={{
                            gridColumn: "1 / -1"
                        }}
                    >

                        <TextField
                            fullWidth
                            label="Observaciones"
                            multiline
                            rows={3}
                            value={
                                formulario
                                    .observaciones
                            }
                            onChange={e =>
                                cambiarCampo(
                                    "observaciones",
                                    e.target.value
                                )
                            }
                            disabled={guardando}
                        />

                    </Box>

                </Box>

            </DialogContent>


            <DialogActions
                sx={{
                    px: 3,
                    pb: 2
                }}
            >

                <Button
                    onClick={cerrar}
                    disabled={guardando}
                >
                    Cancelar
                </Button>

                <Button
                    variant="contained"
                    onClick={guardar}
                    disabled={
                        guardando ||
                        cargando
                    }
                >
                    {guardando
                        ? "Guardando..."
                        : mantenimientoEditar
                            ? "Actualizar"
                            : "Guardar"}
                </Button>

            </DialogActions>

        </Dialog>

    );
}
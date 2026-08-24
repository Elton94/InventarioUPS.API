import { useEffect, useState } from "react";

import {
    Autocomplete,
    Box,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Grid,
    MenuItem,
    TextField
} from "@mui/material";

import { obtenerUPS } from "../../services/upsService";

import {
    crearMantenimiento,
    actualizarMantenimiento
} from "../../services/mantenimientoService";


export default function NuevoMantenimientoModal({
    open,
    onClose,
    onGuardado,
    mantenimientoEditar = null
}) {

    const [ups, setUps] = useState([]);

    const [upsSeleccionado, setUpsSeleccionado] =
        useState(null);

    const [formulario, setFormulario] = useState({
        tipoMantenimiento: "Preventivo",
        fechaMantenimiento: "",
        descripcion: "",
        tecnicoProveedor: "",
        costo: "",
        estado: "Completado",
        observaciones: ""
    });

    const [guardando, setGuardando] =
        useState(false);


    // =====================================
    // CARGAR UPS
    // =====================================

    const cargarUPS = async () => {

        try {

            const datos = await obtenerUPS();

            setUps(datos);

        } catch (error) {

            console.error(
                "Error al cargar UPS:",
                error
            );

        }

    };


    useEffect(() => {

                if (open) {
                    cargarUPS();
                }

            }, [open]);

    useEffect(() => {

            if (!open) {
                return;
            }

            if (mantenimientoEditar) {

                const upsEncontrado =
                    ups.find(
                        u =>
                            u.id ===
                            mantenimientoEditar.upsId
                    );

                setUpsSeleccionado(
                    upsEncontrado || null
                );

                setFormulario({

                    tipoMantenimiento:
                        mantenimientoEditar
                            .tipoMantenimiento || "",

                    fechaMantenimiento:
                        mantenimientoEditar
                            .fechaMantenimiento
                            ?.substring(0, 10) || "",

                    descripcion:
                        mantenimientoEditar
                            .descripcion || "",

                    tecnicoProveedor:
                        mantenimientoEditar
                            .tecnicoProveedor || "",

                    costo:
                        mantenimientoEditar
                            .costo ?? "",

                    estado:
                        mantenimientoEditar
                            .estado || "Completado",

                    observaciones:
                        mantenimientoEditar
                            .observaciones || ""

                });

            }

        }, [open, mantenimientoEditar, ups]);

        useEffect(() => {

    if (!open) {
        return;
    }

    if (mantenimientoEditar) {

        const upsEncontrado =
            ups.find(
                u =>
                    u.id ===
                    mantenimientoEditar.upsId
            );

        setUpsSeleccionado(
            upsEncontrado || null
        );

        setFormulario({

            tipoMantenimiento:
                mantenimientoEditar
                    .tipoMantenimiento || "",

            fechaMantenimiento:
                mantenimientoEditar
                    .fechaMantenimiento
                    ?.substring(0, 10) || "",

            descripcion:
                mantenimientoEditar
                    .descripcion || "",

            tecnicoProveedor:
                mantenimientoEditar
                    .tecnicoProveedor || "",

            costo:
                mantenimientoEditar
                    .costo ?? "",

            estado:
                mantenimientoEditar
                    .estado || "Completado",

            observaciones:
                mantenimientoEditar
                    .observaciones || ""

        });

    }

}, [open, mantenimientoEditar, ups]);

    // =====================================
    // CAMBIAR CAMPO
    // =====================================

    const cambiarCampo = (campo, valor) => {

        setFormulario(prev => ({
            ...prev,
            [campo]: valor
        }));

    };


    // =====================================
    // GUARDAR
    // =====================================

    const guardar = async () => {

        if (!upsSeleccionado) {

            alert("Debe seleccionar un UPS.");
            return;

        }

        if (!formulario.fechaMantenimiento) {

            alert(
                "Debe seleccionar la fecha del mantenimiento."
            );

            return;
        }

        if (!formulario.descripcion.trim()) {

            alert(
                "Debe ingresar una descripción."
            );

            return;
        }

        if (
            formulario.costo === "" ||
            Number(formulario.costo) < 0
        ) {

            alert("Ingrese un costo válido.");

            return;
        }


        try {

                    setGuardando(true);

                    const datos = {

                            id:
                                mantenimientoEditar?.id ?? 0,

                            upsId:
                                upsSeleccionado.id,

                            tipoMantenimiento:
                                formulario.tipoMantenimiento,

                            fechaMantenimiento:
                                `${formulario.fechaMantenimiento}T00:00:00`,

                            descripcion:
                                formulario.descripcion,

                            tecnicoProveedor:
                                formulario.tecnicoProveedor,

                            costo:
                                Number(formulario.costo),

                            estado:
                                formulario.estado,

                            observaciones:
                                formulario.observaciones

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


                            setUpsSeleccionado(null);

                            setFormulario({
                                tipoMantenimiento: "Preventivo",
                                fechaMantenimiento: "",
                                descripcion: "",
                                tecnicoProveedor: "",
                                costo: "",
                                estado: "Completado",
                                observaciones: ""
                            });

                            onClose();

                            await onGuardado();

                        } catch (error) {

                            console.error(
                                "Error al registrar mantenimiento:",
                                error
                            );

                            alert(
                                "No se pudo registrar el mantenimiento."
                            );

                        } finally {

                            setGuardando(false);

                        }

                    };


                    // =====================================
                    // CERRAR
                    // =====================================

                    const cerrar = () => {

                        if (guardando) {
                            return;
                        }

                        setUpsSeleccionado(null);

                        setFormulario({
                            tipoMantenimiento: "Preventivo",
                            fechaMantenimiento: "",
                            descripcion: "",
                            tecnicoProveedor: "",
                            costo: "",
                            estado: "Completado",
                            observaciones: ""
                        });

                        onClose();

                    };


            return (

                <Dialog
                    open={open}
                    onClose={cerrar}
                    fullWidth
                    maxWidth="md"
                >

                    <DialogTitle
                        sx={{
                            fontWeight: "bold",
                            pb: 1
                        }}
                    >
                        {mantenimientoEditar
                            ? "Editar mantenimiento"
                            : "Nuevo mantenimiento"}
                    </DialogTitle>


                    <DialogContent>

                <Box
                    sx={{
                        pt: 1
                    }}
                >

                    <Grid
                        container
                        spacing={2.5}
                    >

                        {/* ========================= */}
                        {/* UPS */}
                        {/* ========================= */}

                        <Grid item xs={12}>

                            <Autocomplete
                                options={ups}
                                value={upsSeleccionado}
                                onChange={(
                                    event,
                                    nuevoValor
                                ) => {

                                    setUpsSeleccionado(
                                        nuevoValor
                                    );

                                }}
                                getOptionLabel={(option) =>
                                    `${option.codigo || ""} - ${option.numeroSerie || ""}`
                                }
                                isOptionEqualToValue={(
                                    option,
                                    value
                                ) =>
                                    option.id === value.id
                                }
                                renderInput={(params) => (

                                    <TextField
                                        {...params}
                                        label="Buscar UPS"
                                        placeholder="Código o número de serie"
                                        fullWidth
                                    />

                                )}
                            />

                        </Grid>


                        {/* ========================= */}
                        {/* TIPO */}
                        {/* ========================= */}

                        <Grid item xs={12} md={6}>

                            <TextField
                                select
                                fullWidth
                                label="Tipo de mantenimiento"
                                value={
                                    formulario
                                        .tipoMantenimiento
                                }
                                onChange={(e) =>
                                    cambiarCampo(
                                        "tipoMantenimiento",
                                        e.target.value
                                    )
                                }
                            >

                                <MenuItem value="Preventivo">
                                    Preventivo
                                </MenuItem>

                                <MenuItem value="Correctivo">
                                    Correctivo
                                </MenuItem>

                                <MenuItem value="Emergencia">
                                    Emergencia
                                </MenuItem>

                            </TextField>

                        </Grid>


                        {/* ========================= */}
                        {/* FECHA */}
                        {/* ========================= */}

                        <Grid item xs={12} md={6}>

                            <TextField
                                fullWidth
                                type="date"
                                label="Fecha de mantenimiento"
                                value={
                                    formulario
                                        .fechaMantenimiento
                                }
                                onChange={(e) =>
                                    cambiarCampo(
                                        "fechaMantenimiento",
                                        e.target.value
                                    )
                                }
                                InputLabelProps={{
                                    shrink: true
                                }}
                            />

                        </Grid>


                        {/* ========================= */}
                        {/* DESCRIPCIÓN */}
                        {/* ========================= */}

                        <Grid item xs={12}>

                            <TextField
                                fullWidth
                                label="Descripción"
                                multiline
                                rows={3}
                                value={
                                    formulario
                                        .descripcion
                                }
                                onChange={(e) =>
                                    cambiarCampo(
                                        "descripcion",
                                        e.target.value
                                    )
                                }
                            />

                        </Grid>


                        {/* ========================= */}
                        {/* TÉCNICO */}
                        {/* ========================= */}

                        <Grid item xs={12} md={6}>

                            <TextField
                                fullWidth
                                label="Técnico / proveedor"
                                value={
                                    formulario
                                        .tecnicoProveedor
                                }
                                onChange={(e) =>
                                    cambiarCampo(
                                        "tecnicoProveedor",
                                        e.target.value
                                    )
                                }
                            />

                        </Grid>


                        {/* ========================= */}
                        {/* COSTO */}
                        {/* ========================= */}

                        <Grid item xs={12} md={3}>

                            <TextField
                                fullWidth
                                label="Costo"
                                type="number"
                                value={
                                    formulario.costo
                                }
                                onChange={(e) =>
                                    cambiarCampo(
                                        "costo",
                                        e.target.value
                                    )
                                }
                                inputProps={{
                                    min: 0,
                                    step: "0.01"
                                }}
                            />

                        </Grid>


                        {/* ========================= */}
                        {/* ESTADO */}
                        {/* ========================= */}

                        <Grid item xs={12} md={3}>

                            <TextField
                                select
                                fullWidth
                                label="Estado"
                                value={
                                    formulario.estado
                                }
                                onChange={(e) =>
                                    cambiarCampo(
                                        "estado",
                                        e.target.value
                                    )
                                }
                            >

                                <MenuItem value="Completado">
                                    Completado
                                </MenuItem>

                                <MenuItem value="En proceso">
                                    En proceso
                                </MenuItem>

                                <MenuItem value="Programado">
                                    Programado
                                </MenuItem>

                                <MenuItem value="Cancelado">
                                    Cancelado
                                </MenuItem>

                            </TextField>

                        </Grid>


                        {/* ========================= */}
                        {/* OBSERVACIONES */}
                        {/* ========================= */}

                        <Grid item xs={12}>

                            <TextField
                                fullWidth
                                label="Observaciones"
                                multiline
                                rows={3}
                                value={
                                    formulario
                                        .observaciones
                                }
                                onChange={(e) =>
                                    cambiarCampo(
                                        "observaciones",
                                        e.target.value
                                    )
                                }
                            />

                        </Grid>

                    </Grid>

                </Box>

            </DialogContent>


            <DialogActions
                sx={{
                    px: 3,
                    pb: 2,
                    gap: 1
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
                    disabled={guardando}
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
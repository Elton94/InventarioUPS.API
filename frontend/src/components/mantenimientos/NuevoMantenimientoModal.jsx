import {
    useEffect,
    useState
} from "react";

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

import {
    obtenerUPS
} from "../../services/upsService";
import {
    crearMantenimiento,
    actualizarMantenimiento
} from "../../services/mantenimientoService";
import {
    obtenerResponsablesMantenimiento
} from "../../services/usuarioService";
import {
    obtenerTiposMantenimiento,
    obtenerEstadosMantenimiento
} from "../../services/catalogosService";

const inicial = {
    tipoMantenimientoId: "",
    fechaMantenimiento: "",
    descripcion: "",
    tecnicoProveedor: "",
    estadoMantenimientoId: "",
    observaciones: "",
    incluyeCambioBaterias: false
};

const fechaActual = () =>
    new Date()
        .toISOString()
        .substring(0, 10);

export default function NuevoMantenimientoModal({
    open,
    onClose,
    onGuardado,
    mantenimientoEditar = null
}) {
    const [equipos, setEquipos] =
        useState([]);
    const [upsSeleccionado, setUpsSeleccionado] =
        useState(null);
    const [tipos, setTipos] =
        useState([]);
    const [estados, setEstados] =
        useState([]);
    const [responsables, setResponsables] =
        useState([]);
    const [formulario, setFormulario] =
        useState(inicial);
    const [cargando, setCargando] =
        useState(false);
    const [guardando, setGuardando] =
        useState(false);
    const [error, setError] =
        useState("");

    const nombreActual =
        localStorage.getItem(
            "nombreCompleto"
        ) ||
        localStorage.getItem(
            "nombreUsuario"
        ) ||
        "";

    useEffect(() => {
        if (!open) return;

        const cargar = async () => {
            try {
                setCargando(true);
                setError("");

                const [
                    upsData,
                    tiposData,
                    estadosData,
                    responsablesData
                ] = await Promise.all([
                    obtenerUPS(),
                    obtenerTiposMantenimiento(),
                    obtenerEstadosMantenimiento(),
                    obtenerResponsablesMantenimiento()
                ]);

                const disponibles =
                    (upsData || [])
                        .filter(
                            (u) =>
                                String(
                                    u.estado ||
                                    ""
                                )
                                    .toLowerCase() !==
                                "de baja"
                        );

                setEquipos(disponibles);
                setTipos(tiposData || []);
                setEstados(
                    estadosData || []
                );
                setResponsables(
                    responsablesData || []
                );

                if (mantenimientoEditar) {
                    const equipo =
                        (upsData || [])
                            .find(
                                (u) =>
                                    Number(u.id) ===
                                    Number(
                                        mantenimientoEditar
                                            .upsId
                                    )
                            );

                    setUpsSeleccionado(
                        equipo || null
                    );

                    setFormulario({
                        tipoMantenimientoId:
                            mantenimientoEditar
                                .tipoMantenimientoId ||
                            "",
                        fechaMantenimiento:
                            mantenimientoEditar
                                .fechaMantenimiento
                                ?.substring(
                                    0,
                                    10
                                ) ||
                            "",
                        descripcion:
                            mantenimientoEditar
                                .descripcion ||
                            "",
                        tecnicoProveedor:
                            mantenimientoEditar
                                .tecnicoProveedor ||
                            "",
                        estadoMantenimientoId:
                            mantenimientoEditar
                                .estadoMantenimientoId ||
                            "",
                        observaciones:
                            mantenimientoEditar
                                .observaciones ||
                            "",
                        incluyeCambioBaterias:
                            mantenimientoEditar
                                .incluyeCambioBaterias ===
                            true
                    });
                } else {
                    setUpsSeleccionado(
                        null
                    );
                    setFormulario({
                        ...inicial,
                        tecnicoProveedor:
                            nombreActual
                    });
                }
            } catch (e) {
                setError(
                    e.response?.data?.mensaje ||
                    "No fue posible cargar los datos necesarios."
                );
            } finally {
                setCargando(false);
            }
        };

        cargar();
    }, [
        open,
        mantenimientoEditar
    ]);

    const cambiar = (
        campo,
        valor
    ) =>
        setFormulario(
            (actual) => ({
                ...actual,
                [campo]: valor
            })
        );

    const estadoNombre =
        estados.find(
            (e) =>
                Number(e.id) ===
                Number(
                    formulario
                        .estadoMantenimientoId
                )
        )?.nombre || "";

    const esProgramado =
        estadoNombre
            .toLowerCase() ===
        "programado";

    const cantidadBaterias =
        Number(
            upsSeleccionado
                ?.cantidadBaterias ||
            0
        );

    const guardar = async () => {
        setError("");

        if (!upsSeleccionado) {
            setError(
                "Debe seleccionar un UPS."
            );
            return;
        }

        if (
            mantenimientoEditar &&
            Number(
                upsSeleccionado.id
            ) !==
            Number(
                mantenimientoEditar
                    .upsId
            )
        ) {
            setError(
                "No se puede cambiar el UPS de un mantenimiento existente."
            );
            return;
        }

        if (!formulario.tipoMantenimientoId) {
            setError(
                "Seleccione el tipo de mantenimiento."
            );
            return;
        }

        if (!formulario.estadoMantenimientoId) {
            setError(
                "Seleccione el estado del mantenimiento."
            );
            return;
        }

        if (!formulario.fechaMantenimiento) {
            setError(
                "Seleccione la fecha del mantenimiento."
            );
            return;
        }

        if (
            formulario.fechaMantenimiento >
                fechaActual() &&
            !esProgramado
        ) {
            setError(
                "Una fecha futura solamente puede utilizarse con estado Programado."
            );
            return;
        }

        if (!formulario.descripcion.trim()) {
            setError(
                "Ingrese una descripción."
            );
            return;
        }

        if (
            !formulario
                .tecnicoProveedor
                .trim()
        ) {
            setError(
                "Indique un técnico o proveedor."
            );
            return;
        }

        if (
            formulario
                .incluyeCambioBaterias
        ) {
            if (cantidadBaterias <= 0) {
                setError(
                    "El UPS no tiene una cantidad válida de baterías."
                );
                return;
            }
        }

        const datos = {
            id:
                mantenimientoEditar?.id ||
                0,
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
                formulario
                    .tecnicoProveedor
                    .trim(),
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
                    ? cantidadBaterias
                    : 0
        };

        try {
            setGuardando(true);

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
            onClose();
        } catch (e) {
            setError(
                e.response?.data?.mensaje ||
                e.response?.data?.message ||
                "No fue posible guardar el mantenimiento."
            );
        } finally {
            setGuardando(false);
        }
    };

    const responsableOpciones =
        responsables.map(
            (r) =>
                r.nombreCompleto ||
                r.nombreUsuario
        );

    return (
        <Dialog
            open={open}
            onClose={
                guardando
                    ? undefined
                    : onClose
            }
            fullWidth
            maxWidth="md"
        >
            <DialogTitle>
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
                    <Box
                        sx={{
                            gridColumn:
                                "1 / -1"
                        }}
                    >
                        <Autocomplete
                            options={equipos}
                            value={
                                upsSeleccionado
                            }
                            disabled={
                                cargando ||
                                guardando ||
                                Boolean(
                                    mantenimientoEditar
                                )
                            }
                            getOptionLabel={(u) =>
                                [
                                    u.numeroInventario,
                                    u.numeroSerie,
                                    u.marca,
                                    u.modelo,
                                    u.agenciaCodigo,
                                    u.agenciaNombre,
                                    u.estado
                                ]
                                    .filter(Boolean)
                                    .join(" - ")
                            }
                            isOptionEqualToValue={(
                                a,
                                b
                            ) =>
                                Number(a.id) ===
                                Number(b.id)
                            }
                            onChange={(
                                _,
                                valor
                            ) =>
                                setUpsSeleccionado(
                                    valor
                                )
                            }
                            renderInput={(
                                params
                            ) => (
                                <TextField
                                    {...params}
                                    required
                                    label="UPS"
                                    placeholder="Inventario, serie o agencia"
                                    helperText={
                                        mantenimientoEditar
                                            ? "No se puede cambiar el UPS al editar."
                                            : "Solo UPS que no estén de baja."
                                    }
                                />
                            )}
                        />
                    </Box>

                    <TextField
                        select
                        required
                        label="Tipo de mantenimiento"
                        value={
                            formulario
                                .tipoMantenimientoId
                        }
                        onChange={(e) =>
                            cambiar(
                                "tipoMantenimientoId",
                                e.target.value
                            )
                        }
                    >
                        {tipos.map(
                            (tipo) => (
                                <MenuItem
                                    key={tipo.id}
                                    value={tipo.id}
                                >
                                    {tipo.nombre}
                                </MenuItem>
                            )
                        )}
                    </TextField>

                    <TextField
                        select
                        required
                        label="Estado"
                        value={
                            formulario
                                .estadoMantenimientoId
                        }
                        onChange={(e) =>
                            cambiar(
                                "estadoMantenimientoId",
                                e.target.value
                            )
                        }
                    >
                        {estados.map(
                            (estado) => (
                                <MenuItem
                                    key={estado.id}
                                    value={estado.id}
                                >
                                    {estado.nombre}
                                </MenuItem>
                            )
                        )}
                    </TextField>

                    <TextField
                        required
                        type="date"
                        label="Fecha de mantenimiento"
                        value={
                            formulario
                                .fechaMantenimiento
                        }
                        onChange={(e) =>
                            cambiar(
                                "fechaMantenimiento",
                                e.target.value
                            )
                        }
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            },
                            htmlInput:
                                esProgramado
                                    ? {}
                                    : {
                                        max:
                                            fechaActual()
                                    }
                        }}
                    />

                    <Autocomplete
                        freeSolo
                        options={
                            responsableOpciones
                        }
                        inputValue={
                            formulario
                                .tecnicoProveedor
                        }
                        onInputChange={(
                            _,
                            valor
                        ) =>
                            cambiar(
                                "tecnicoProveedor",
                                valor
                            )
                        }
                        renderInput={(
                            params
                        ) => (
                            <TextField
                                {...params}
                                required
                                label="Técnico / proveedor"
                                helperText="Seleccione un técnico o escriba un proveedor."
                            />
                        )}
                    />

                    <TextField
                        sx={{
                            gridColumn:
                                "1 / -1"
                        }}
                        required
                        multiline
                        rows={3}
                        label="Descripción"
                        value={
                            formulario.descripcion
                        }
                        onChange={(e) =>
                            cambiar(
                                "descripcion",
                                e.target.value
                            )
                        }
                    />

                    <Paper
                        variant="outlined"
                        sx={{
                            gridColumn:
                                "1 / -1",
                            p: 2
                        }}
                    >
                        <Typography
                            fontWeight="bold"
                        >
                            Cambio de baterías
                        </Typography>

                        <FormControlLabel
                            control={
                                <Checkbox
                                    checked={
                                        formulario
                                            .incluyeCambioBaterias
                                    }
                                    onChange={(e) =>
                                        cambiar(
                                            "incluyeCambioBaterias",
                                            e.target.checked
                                        )
                                    }
                                />
                            }
                            label={
                                esProgramado
                                    ? "Este mantenimiento contempla cambio de baterías"
                                    : "Este mantenimiento incluye cambio de baterías"
                            }
                        />

                        {formulario
                            .incluyeCambioBaterias && (
                            <Box
                                sx={{
                                    display: "grid",
                                    gridTemplateColumns:
                                        "1fr",
                                    gap: 2,
                                    mt: 1
                                }}
                            >
                                <TextField
                                    label="Cantidad"
                                    value={
                                        cantidadBaterias ||
                                        ""
                                    }
                                    disabled
                                    helperText="Se reemplaza el banco completo."
                                />
                            </Box>
                        )}
                    </Paper>

                    <TextField
                        sx={{
                            gridColumn:
                                "1 / -1"
                        }}
                        multiline
                        rows={2}
                        label="Observaciones"
                        value={
                            formulario
                                .observaciones
                        }
                        onChange={(e) =>
                            cambiar(
                                "observaciones",
                                e.target.value
                            )
                        }
                    />
                </Box>
            </DialogContent>

            <DialogActions>
                <Button
                    onClick={onClose}
                    disabled={guardando}
                >
                    Cancelar
                </Button>
                <Button
                    variant="contained"
                    onClick={guardar}
                    disabled={
                        cargando ||
                        guardando
                    }
                >
                    {guardando
                        ? "Guardando..."
                        : "Guardar"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}

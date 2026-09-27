import {
    useEffect,
    useState
} from "react";

import {
    Alert,
    Autocomplete,
    Box,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    MenuItem,
    TextField
} from "@mui/material";

import {
    obtenerAgencias
} from "../../services/agenciaService";
import {
    crearUPS,
    actualizarUPS
} from "../../services/upsService";
import {
    obtenerEstadosUPS,
    obtenerMarcasUPS,
    obtenerModelosUPS
} from "../../services/catalogosService";

import SelectorBateria from "../baterias/SelectorBateria";

const inicial = {
    tipoBateriaId: null,
    modeloBateriaId: null,
    numeroInventario: "",
    marca: "",
    modelo: "",
    numeroSerie: "",
    capacidadVA: "",
    agenciaId: "",
    fechaInstalacion: "",
    estadoUPSId: "",
    cantidadBaterias: "",
    fechaUltimoCambioBaterias: ""
};

const hoy = () => {
    const fecha = new Date();
    return `${fecha.getFullYear()}-${String(fecha.getMonth() + 1).padStart(2, "0")}-${String(fecha.getDate()).padStart(2, "0")}`;
};

export default function NuevoUPSModal({
    open,
    onClose,
    onGuardado,
    ups
}) {
    const [agencias, setAgencias] =
        useState([]);
    const [estados, setEstados] =
        useState([]);
    const [marcas, setMarcas] =
        useState([]);
    const [modelos, setModelos] =
        useState([]);
    const [formulario, setFormulario] =
        useState(inicial);
    const [cargando, setCargando] =
        useState(false);
    const [guardando, setGuardando] =
        useState(false);
    const [error, setError] =
        useState("");

    const cargarModelos = async (
        marca
    ) => {
        try {
            const datos =
                await obtenerModelosUPS(
                    marca || ""
                );

            setModelos(
                Array.isArray(datos)
                    ? datos
                    : []
            );
        } catch {
            setModelos([]);
        }
    };

    useEffect(() => {
        if (!open) return;

        const cargar = async () => {
            try {
                setCargando(true);
                setError("");

                const [
                    agenciasData,
                    estadosData,
                    marcasData
                ] = await Promise.all([
                    obtenerAgencias(),
                    obtenerEstadosUPS(),
                    obtenerMarcasUPS()
                ]);

                setAgencias(
                    (agenciasData || [])
                        .filter(
                            (a) => a.activa
                        )
                );
                setEstados(
                    estadosData || []
                );
                setMarcas(
                    marcasData || []
                );

                if (ups) {
                    setFormulario({
                        tipoBateriaId: ups.tipoBateriaId || null,
                        modeloBateriaId: ups.modeloBateriaId || null,
                        numeroInventario:
                            ups.numeroInventario || "",
                        marca:
                            ups.marca || "",
                        modelo:
                            ups.modelo || "",
                        numeroSerie:
                            ups.numeroSerie || "",
                        capacidadVA:
                            ups.capacidadVA ?? "",
                        agenciaId:
                            ups.agenciaId ?? "",
                        fechaInstalacion:
                            ups.fechaInstalacion
                                ?.substring(0, 10) ||
                            "",
                        estadoUPSId:
                            ups.estadoUPSId ?? "",
                        cantidadBaterias:
                            ups.cantidadBaterias ??
                            "",
                        fechaUltimoCambioBaterias:
                            ups
                                .fechaUltimoCambioBaterias
                                ?.substring(0, 10) ||
                            ""
                    });

                    await cargarModelos(
                        ups.marca
                    );
                } else {
                    setFormulario({
                        ...inicial
                    });
                    setModelos([]);
                }
            } catch {
                setError(
                    "No fue posible cargar los datos del formulario."
                );
            } finally {
                setCargando(false);
            }
        };

        cargar();
    }, [open, ups]);

    const cambiar = (
        campo,
        valor
    ) => {
        setFormulario(
            (actual) => ({
                ...actual,
                [campo]: valor
            })
        );
    };

    const guardar = async () => {
        setError("");

        if (!formulario.tipoBateriaId) return setError("Seleccione el tipo de batería. El modelo es opcional.");

        if (!formulario.numeroInventario.trim()) {
            setError(
                "El número de inventario es obligatorio."
            );
            return;
        }

        if (!formulario.numeroSerie.trim()) {
            setError(
                "El número de serie es obligatorio."
            );
            return;
        }

        if (!formulario.marca.trim()) {
            setError(
                "La marca es obligatoria."
            );
            return;
        }

        if (!formulario.modelo.trim()) {
            setError(
                "El modelo es obligatorio."
            );
            return;
        }

        if (
            Number(formulario.capacidadVA) <=
            0
        ) {
            setError(
                "La capacidad debe ser mayor que cero."
            );
            return;
        }

        if (!formulario.agenciaId) {
            setError(
                "Seleccione una agencia activa."
            );
            return;
        }

        if (!formulario.fechaInstalacion) {
            setError(
                "La fecha de instalación es obligatoria."
            );
            return;
        }

        if (
            formulario.fechaInstalacion >
            hoy()
        ) {
            setError(
                "La fecha de instalación no puede ser futura."
            );
            return;
        }

        if (!formulario.estadoUPSId) {
            setError(
                "Seleccione un estado."
            );
            return;
        }

        if (
            Number(
                formulario.cantidadBaterias
            ) <= 0
        ) {
            setError(
                "La cantidad de baterías debe ser mayor que cero."
            );
            return;
        }

        if (
            formulario
                .fechaUltimoCambioBaterias &&
            formulario
                .fechaUltimoCambioBaterias <
                formulario.fechaInstalacion
        ) {
            setError(
                "La fecha del último cambio no puede ser anterior a la instalación."
            );
            return;
        }

        if (ups) {
            const nuevoEstado =
                estados.find(
                    (e) =>
                        Number(e.id) ===
                        Number(
                            formulario.estadoUPSId
                        )
                )?.nombre || "";

            const cambioImportante =
                nuevoEstado !== ups.estado &&
                [
                    "Fuera de servicio",
                    "De baja"
                ].includes(nuevoEstado);

            if (
                cambioImportante &&
                !window.confirm(
                    `El UPS cambiará de "${ups.estado}" a "${nuevoEstado}". Este cambio de estado es importante. ¿Desea continuar?`
                )
            ) {
                return;
            }
        }

        try {
            setGuardando(true);

            const datos = {
                ...formulario,
                marca:
                    formulario.marca.trim(),
                modelo:
                    formulario.modelo.trim()
            };

            if (ups) {
                await actualizarUPS(
                    ups.id,
                    datos
                );
            } else {
                await crearUPS(datos);
            }

            await onGuardado();
            onClose();
        } catch (e) {
            setError(
                e.response?.data?.mensaje ||
                e.response?.data?.message ||
                "No fue posible guardar el UPS."
            );
        } finally {
            setGuardando(false);
        }
    };

    const agenciaSeleccionada =
        agencias.find(
            (a) =>
                Number(a.id) ===
                Number(
                    formulario.agenciaId
                )
        ) || null;

    const estadoActual =
        estados.find(
            (e) =>
                Number(e.id) ===
                Number(
                    formulario.estadoUPSId
                )
        )?.nombre;

    const esBaja =
        ups &&
        String(
            ups.estado || estadoActual || ""
        )
            .toLowerCase() ===
            "de baja";

    return (
        <Dialog
            open={open}
            onClose={
                guardando
                    ? undefined
                    : onClose
            }
            fullWidth
            maxWidth="sm"
        >
            <DialogTitle>
                {ups
                    ? "Editar UPS"
                    : "Nuevo UPS"}
            </DialogTitle>

            <DialogContent>
                {error && (
                    <Alert
                        severity="error"
                        sx={{ mt: 1, mb: 2 }}
                    >
                        {error}
                    </Alert>
                )}

                {ups &&
                    !agenciaSeleccionada && (
                    <Alert
                        severity="warning"
                        sx={{ mt: 1, mb: 2 }}
                    >
                        Agencia inactiva. Seleccione una agencia activa para guardar.
                    </Alert>
                )}

                <Box
                    sx={{
                        display: "flex",
                        flexDirection:
                            "column",
                        gap: 2,
                        mt: 1
                    }}
                >
                    <TextField
                        required
                        label="Número de inventario"
                        value={
                            formulario.numeroInventario
                        }
                        onChange={(e) =>
                            cambiar(
                                "numeroInventario",
                                e.target.value
                            )
                        }
                    />

                    <TextField
                        required
                        label="Número de serie"
                        value={
                            formulario.numeroSerie
                        }
                        onChange={(e) =>
                            cambiar(
                                "numeroSerie",
                                e.target.value
                            )
                        }
                    />

                    <Autocomplete
                        options={[...new Set([...marcas, ...(ups?.marca ? [ups.marca] : [])])]}
                        value={formulario.marca}
                        onChange={(
                            _,
                            valor
                        ) => {
                            cambiar(
                                "marca",
                                valor || ""
                            );
                            cambiar(
                                "modelo",
                                ""
                            );
                            cargarModelos(
                                valor
                            );
                        }}
                        renderInput={(params) => (
                            <TextField
                                {...params}
                                required
                                label="Marca"
                                helperText="Seleccione una marca. Las opciones nuevas se agregan desde Configuración."
                            />
                        )}
                    />

                    <Autocomplete
                        options={[...new Set([...modelos, ...(ups?.modelo && formulario.marca === ups.marca ? [ups.modelo] : [])])]}
                        value={formulario.modelo}
                        onChange={(
                            _,
                            valor
                        ) =>
                            cambiar(
                                "modelo",
                                valor || ""
                            )
                        }
                        renderInput={(params) => (
                            <TextField
                                {...params}
                                required
                                label="Modelo"
                                helperText="Seleccione un modelo de la marca elegida."
                            />
                        )}
                    />

                    <TextField
                        required
                        type="number"
                        label="Capacidad (VA)"
                        value={
                            formulario.capacidadVA
                        }
                        onChange={(e) =>
                            cambiar(
                                "capacidadVA",
                                e.target.value
                            )
                        }
                    />

                    <Autocomplete
                        options={agencias}
                        value={
                            agenciaSeleccionada
                        }
                        getOptionLabel={(a) =>
                            [
                                a.codigo,
                                a.nombre,
                                a.ciudadNombre
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
                            cambiar(
                                "agenciaId",
                                valor
                                    ? valor.id
                                    : ""
                            )
                        }
                        renderInput={(params) => (
                            <TextField
                                {...params}
                                required
                                label="Agencia"
                                placeholder="Buscar agencia..."
                            />
                        )}
                    />

                    <TextField
                        required
                        type="date"
                        label="Fecha de instalación"
                        value={
                            formulario
                                .fechaInstalacion
                        }
                        onChange={(e) =>
                            cambiar(
                                "fechaInstalacion",
                                e.target.value
                            )
                        }
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            },
                            htmlInput: {
                                max: hoy()
                            }
                        }}
                    />

                    <TextField
                        required
                        type="number"
                        label="Cantidad de baterías"
                        value={
                            formulario
                                .cantidadBaterias
                        }
                        onChange={(e) =>
                            cambiar(
                                "cantidadBaterias",
                                e.target.value
                            )
                        }
                    />

                    <SelectorBateria modeloOpcional tipoValue={formulario.tipoBateriaId} onTipoChange={id => cambiar("tipoBateriaId", id)} value={formulario.modeloBateriaId} onChange={id => cambiar("modeloBateriaId", id)} disabled={guardando || cargando} />

                    <TextField
                        type="date"
                        label="Último cambio de baterías"
                        value={
                            formulario
                                .fechaUltimoCambioBaterias
                        }
                        onChange={(e) =>
                            cambiar(
                                "fechaUltimoCambioBaterias",
                                e.target.value
                            )
                        }
                        helperText="Se conserva el último cambio de baterías completado."
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            },
                            htmlInput: {
                                min:
                                    formulario
                                        .fechaInstalacion ||
                                    undefined,
                                max: hoy()
                            }
                        }}
                    />

                    <TextField
                        select
                        required
                        label="Estado"
                        value={
                            formulario.estadoUPSId
                        }
                        disabled={esBaja}
                        onChange={(e) =>
                            cambiar(
                                "estadoUPSId",
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

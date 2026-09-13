import { useEffect, useState } from "react";

import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    FormControlLabel,
    Checkbox,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Alert,
    Box
} from "@mui/material";

import {
    crearAgencia,
    actualizarAgencia
} from "../../services/agenciaService";

import {
    obtenerDepartamentos,
    obtenerCiudades,
    obtenerTiposAgencia
} from "../../services/catalogosService";

const formularioInicial = {
    codigo: "",
    nombre: "",
    ciudadId: "",
    direccion: "",
    tipoAgenciaId: "",
    activa: true
};

export default function NuevaAgenciaModal({
    open,
    onClose,
    onGuardado,
    agencia
}) {
    const [formulario, setFormulario] = useState(
        formularioInicial
    );

    const [departamentos, setDepartamentos] = useState([]);
    const [ciudades, setCiudades] = useState([]);
    const [tiposAgencia, setTiposAgencia] = useState([]);

    const [departamentoId, setDepartamentoId] = useState("");

    const [cargandoCatalogos, setCargandoCatalogos] =
        useState(false);

    const [guardando, setGuardando] = useState(false);
    const [error, setError] = useState("");

    useEffect(() => {
        if (!open) return;

        setError("");

        cargarCatalogos();

        if (agencia) {
            const ciudadId =
                agencia.ciudadId ??
                agencia.ciudad?.id ??
                "";

            const tipoAgenciaId =
                agencia.tipoAgenciaId ??
                agencia.tipoAgencia?.id ??
                "";

            const depId =
                agencia.ciudad?.departamentoId ??
                agencia.ciudad?.departamento?.id ??
                "";

            setDepartamentoId(
                depId ? String(depId) : ""
            );

            setFormulario({
                codigo: agencia.codigo || "",
                nombre: agencia.nombre || "",
                ciudadId: ciudadId
                    ? String(ciudadId)
                    : "",
                direccion: agencia.direccion || "",
                tipoAgenciaId: tipoAgenciaId
                    ? String(tipoAgenciaId)
                    : "",
                activa:
                    agencia.activa ?? true
            });

            if (depId) {
                cargarCiudades(depId);
            }
        } else {
            setDepartamentoId("");
            setCiudades([]);
            setFormulario(formularioInicial);
        }
    }, [open, agencia]);

    const cargarCatalogos = async () => {
        try {
            setCargandoCatalogos(true);

            const [
                departamentosData,
                tiposData
            ] = await Promise.all([
                obtenerDepartamentos(),
                obtenerTiposAgencia()
            ]);

            setDepartamentos(
                departamentosData || []
            );

            setTiposAgencia(
                tiposData || []
            );
        } catch (error) {
            console.error(
                "Error al cargar catálogos:",
                error
            );

            setError(
                "No se pudieron cargar los catálogos."
            );
        } finally {
            setCargandoCatalogos(false);
        }
    };

    const cargarCiudades = async (
        idDepartamento
    ) => {
        if (!idDepartamento) {
            setCiudades([]);
            return;
        }

        try {
            const datos =
                await obtenerCiudades(
                    idDepartamento
                );

            setCiudades(datos || []);
        } catch (error) {
            console.error(
                "Error al cargar ciudades:",
                error
            );

            setCiudades([]);

            setError(
                "No se pudieron cargar las ciudades."
            );
        }
    };

    const determinarTipoAgencia = (
        ciudadSeleccionada
    ) => {
        if (!ciudadSeleccionada) {
            return;
        }

        const nombre =
            ciudadSeleccionada.nombre
                ?.trim()
                .toLowerCase();

        const esLocal =
            nombre === "san pedro sula" ||
            nombre === "la ceiba" ||
            nombre === "distrito central";

        const tipoLocal =
            tiposAgencia.find(
                tipo =>
                    tipo.nombre
                        ?.trim()
                        .toLowerCase() === "local"
            );

        const tipoForanea =
            tiposAgencia.find(
                tipo =>
                    tipo.nombre
                        ?.trim()
                        .toLowerCase() ===
                        "foránea" ||
                    tipo.nombre
                        ?.trim()
                        .toLowerCase() ===
                        "foranea"
            );

        const tipo = esLocal
            ? tipoLocal
            : tipoForanea;

        setFormulario(prev => ({
            ...prev,
            tipoAgenciaId: tipo
                ? String(tipo.id)
                : ""
        }));
    };

    const cambiarDepartamento = async event => {
        const idDepartamento =
            event.target.value;

        setDepartamentoId(
            idDepartamento
        );

        setFormulario(prev => ({
            ...prev,
            ciudadId: "",
            tipoAgenciaId: ""
        }));

        await cargarCiudades(
            idDepartamento
        );
    };

    const cambiarCiudad = event => {
        const ciudadId =
            event.target.value;

        const ciudadSeleccionada =
            ciudades.find(
                ciudad =>
                    String(ciudad.id) ===
                    String(ciudadId)
            );

        setFormulario(prev => ({
            ...prev,
            ciudadId
        }));

        determinarTipoAgencia(
            ciudadSeleccionada
        );
    };

    const cambiarValor = event => {
        const {
            name,
            value
        } = event.target;

        setFormulario(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const guardar = async () => {
        setError("");

        if (!formulario.codigo.trim()) {
            setError(
                "El código es obligatorio."
            );
            return;
        }

        if (!formulario.nombre.trim()) {
            setError(
                "El nombre es obligatorio."
            );
            return;
        }

        if (!departamentoId) {
            setError(
                "Seleccione un departamento."
            );
            return;
        }

        if (!formulario.ciudadId) {
            setError(
                "Seleccione una ciudad."
            );
            return;
        }

        if (!formulario.tipoAgenciaId) {
            setError(
                "No se pudo determinar el tipo de agencia."
            );
            return;
        }

        if (!formulario.direccion.trim()) {
            setError(
                "La dirección es obligatoria."
            );
            return;
        }

        try {
            setGuardando(true);

            const datos = {
                id: agencia?.id ?? 0,
                codigo:
                    formulario.codigo.trim(),
                nombre:
                    formulario.nombre.trim(),
                ciudadId: Number(
                    formulario.ciudadId
                ),
                direccion:
                    formulario.direccion.trim(),
                tipoAgenciaId: Number(
                    formulario.tipoAgenciaId
                ),
                activa:
                    formulario.activa
            };

            if (agencia) {
                await actualizarAgencia(
                    agencia.id,
                    datos
                );
            } else {
                await crearAgencia(
                    datos
                );
            }

            await onGuardado();

            onClose();
        } catch (error) {
            console.error(
                "Error al guardar agencia:",
                error
            );

            const mensaje =
                error.response?.data?.mensaje ||
                error.response?.data?.message ||
                "No se pudo guardar la agencia.";

            setError(mensaje);
        } finally {
            setGuardando(false);
        }
    };

    const tipoSeleccionado =
        tiposAgencia.find(
            tipo =>
                String(tipo.id) ===
                String(
                    formulario.tipoAgenciaId
                )
        );

    return (
        <Dialog
            open={open}
            onClose={
                guardando
                    ? undefined
                    : onClose
            }
            maxWidth="md"
            fullWidth
        >
            <DialogTitle>
                {agencia
                    ? "Editar Agencia"
                    : "Nueva Agencia"}
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
                    <TextField
                        fullWidth
                        label="Código"
                        name="codigo"
                        value={
                            formulario.codigo
                        }
                        onChange={
                            cambiarValor
                        }
                        disabled={guardando}
                        required
                    />

                    <TextField
                        fullWidth
                        label="Nombre"
                        name="nombre"
                        value={
                            formulario.nombre
                        }
                        onChange={
                            cambiarValor
                        }
                        disabled={guardando}
                        required
                    />

                    <FormControl
                        fullWidth
                        required
                    >
                        <InputLabel>
                            Departamento
                        </InputLabel>

                        <Select
                            value={
                                departamentoId
                            }
                            label="Departamento"
                            onChange={
                                cambiarDepartamento
                            }
                            disabled={
                                cargandoCatalogos ||
                                guardando
                            }
                        >
                            <MenuItem value="">
                                Seleccione...
                            </MenuItem>

                            {departamentos.map(
                                departamento => (
                                    <MenuItem
                                        key={
                                            departamento.id
                                        }
                                        value={
                                            departamento.id
                                        }
                                    >
                                        {
                                            departamento.nombre
                                        }
                                    </MenuItem>
                                )
                            )}
                        </Select>
                    </FormControl>

                    <FormControl
                        fullWidth
                        required
                        disabled={
                            !departamentoId ||
                            guardando
                        }
                    >
                        <InputLabel>
                            Ciudad
                        </InputLabel>

                        <Select
                            value={
                                formulario.ciudadId
                            }
                            label="Ciudad"
                            onChange={
                                cambiarCiudad
                            }
                        >
                            <MenuItem value="">
                                Seleccione...
                            </MenuItem>

                            {ciudades.map(
                                ciudad => (
                                    <MenuItem
                                        key={
                                            ciudad.id
                                        }
                                        value={
                                            ciudad.id
                                        }
                                    >
                                        {
                                            ciudad.nombre
                                        }
                                    </MenuItem>
                                )
                            )}
                        </Select>
                    </FormControl>

                    <TextField
                        fullWidth
                        label="Dirección"
                        name="direccion"
                        value={
                            formulario.direccion
                        }
                        onChange={
                            cambiarValor
                        }
                        disabled={guardando}
                        required
                    />

                    <TextField
                        fullWidth
                        label="Tipo de Agencia"
                        value={
                            tipoSeleccionado?.nombre ||
                            ""
                        }
                        disabled
                        helperText={
                            formulario.ciudadId
                                ? "Determinado automáticamente"
                                : "Seleccione una ciudad"
                        }
                    />

                    <Box
                        sx={{
                            gridColumn:
                                "1 / -1",
                            display: "flex",
                            alignItems:
                                "center"
                        }}
                    >
                        <FormControlLabel
                            control={
                                <Checkbox
                                    checked={
                                        formulario.activa
                                    }
                                    onChange={
                                        event =>
                                            setFormulario(
                                                prev => ({
                                                    ...prev,
                                                    activa:
                                                        event
                                                            .target
                                                            .checked
                                                })
                                            )
                                    }
                                    disabled={
                                        guardando
                                    }
                                />
                            }
                            label="Agencia activa"
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
                    onClick={onClose}
                    disabled={guardando}
                >
                    Cancelar
                </Button>

                <Button
                    variant="contained"
                    onClick={guardar}
                    disabled={
                        guardando ||
                        cargandoCatalogos
                    }
                >
                    {guardando
                        ? "Guardando..."
                        : agencia
                            ? "Guardar cambios"
                            : "Guardar"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
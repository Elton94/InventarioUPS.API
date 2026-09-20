import {
    useEffect,
    useState
} from "react";

import {
    Alert,
    Box,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    TextField
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

const inicial = {
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
    const [formulario, setFormulario] =
        useState(inicial);
    const [departamentoId, setDepartamentoId] =
        useState("");
    const [departamentos, setDepartamentos] =
        useState([]);
    const [ciudades, setCiudades] =
        useState([]);
    const [tipos, setTipos] =
        useState([]);
    const [cargando, setCargando] =
        useState(false);
    const [guardando, setGuardando] =
        useState(false);
    const [error, setError] =
        useState("");

    const cargarCiudades =
        async (id) => {
            if (!id) {
                setCiudades([]);
                return;
            }

            const datos =
                await obtenerCiudades(id);

            setCiudades(
                Array.isArray(datos)
                    ? datos
                    : []
            );
        };

    useEffect(() => {
        if (!open) return;

        const cargar = async () => {
            try {
                setCargando(true);
                setError("");

                const [deps, tiposData] =
                    await Promise.all([
                        obtenerDepartamentos(),
                        obtenerTiposAgencia()
                    ]);

                setDepartamentos(
                    Array.isArray(deps)
                        ? deps
                        : []
                );
                setTipos(
                    Array.isArray(tiposData)
                        ? tiposData
                        : []
                );

                if (agencia) {
                    const dep =
                        agencia.departamentoId ||
                        "";

                    setDepartamentoId(
                        dep
                            ? String(dep)
                            : ""
                    );

                    setFormulario({
                        codigo:
                            agencia.codigo || "",
                        nombre:
                            agencia.nombre || "",
                        ciudadId:
                            agencia.ciudadId
                                ? String(
                                    agencia.ciudadId
                                )
                                : "",
                        direccion:
                            agencia.direccion || "",
                        tipoAgenciaId:
                            agencia.tipoAgenciaId
                                ? String(
                                    agencia.tipoAgenciaId
                                )
                                : "",
                        activa:
                            agencia.activa !==
                            false
                    });

                    if (dep) {
                        await cargarCiudades(
                            dep
                        );
                    }
                } else {
                    setDepartamentoId("");
                    setCiudades([]);
                    setFormulario({
                        ...inicial
                    });
                }
            } catch {
                setError(
                    "No fue posible cargar los catálogos."
                );
            } finally {
                setCargando(false);
            }
        };

        cargar();
    }, [open, agencia]);

    const determinarTipo = (
        ciudadId
    ) => {
        const ciudad =
            ciudades.find(
                (c) =>
                    String(c.id) ===
                    String(ciudadId)
            );

        if (!ciudad) return;

        const nombre =
            String(ciudad.nombre || "")
                .trim()
                .toLowerCase();

        const local = [
            "san pedro sula",
            "la ceiba",
            "distrito central"
        ].includes(nombre);

        const tipo =
            tipos.find((t) => {
                const valor =
                    String(t.nombre || "")
                        .trim()
                        .toLowerCase();

                return local
                    ? valor === "local"
                    : valor === "foránea" ||
                      valor === "foranea";
            });

        setFormulario((actual) => ({
            ...actual,
            ciudadId:
                String(ciudadId),
            tipoAgenciaId:
                tipo
                    ? String(tipo.id)
                    : ""
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

        if (!formulario.direccion.trim()) {
            setError(
                "La dirección es obligatoria."
            );
            return;
        }

        if (!formulario.tipoAgenciaId) {
            setError(
                "No se pudo determinar el tipo de agencia."
            );
            return;
        }

        try {
            setGuardando(true);

            const datos = {
                ...formulario,
                codigo:
                    formulario.codigo.trim(),
                nombre:
                    formulario.nombre.trim(),
                direccion:
                    formulario.direccion.trim(),
                ciudadId:
                    Number(
                        formulario.ciudadId
                    ),
                tipoAgenciaId:
                    Number(
                        formulario
                            .tipoAgenciaId
                    )
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
        } catch (e) {
            setError(
                e.response?.data?.mensaje ||
                e.response?.data?.message ||
                "No fue posible guardar la agencia."
            );
        } finally {
            setGuardando(false);
        }
    };

    const tipoNombre =
        tipos.find(
            (t) =>
                String(t.id) ===
                String(
                    formulario.tipoAgenciaId
                )
        )?.nombre || "";

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
                {agencia
                    ? "Editar Agencia"
                    : "Nueva Agencia"}
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
                        label="Código"
                        value={formulario.codigo}
                        onChange={(e) =>
                            setFormulario(
                                (a) => ({
                                    ...a,
                                    codigo:
                                        e.target.value
                                })
                            )
                        }
                        disabled={guardando}
                        required
                    />

                    <TextField
                        label="Nombre"
                        value={formulario.nombre}
                        onChange={(e) =>
                            setFormulario(
                                (a) => ({
                                    ...a,
                                    nombre:
                                        e.target.value
                                })
                            )
                        }
                        disabled={guardando}
                        required
                    />

                    <FormControl required>
                        <InputLabel>
                            Departamento
                        </InputLabel>
                        <Select
                            value={departamentoId}
                            label="Departamento"
                            disabled={
                                cargando ||
                                guardando
                            }
                            onChange={async (e) => {
                                const valor =
                                    e.target.value;

                                setDepartamentoId(
                                    valor
                                );
                                setFormulario(
                                    (a) => ({
                                        ...a,
                                        ciudadId: "",
                                        tipoAgenciaId:
                                            ""
                                    })
                                );
                                await cargarCiudades(
                                    valor
                                );
                            }}
                        >
                            <MenuItem value="">
                                Seleccione...
                            </MenuItem>
                            {departamentos.map(
                                (d) => (
                                    <MenuItem
                                        key={d.id}
                                        value={d.id}
                                    >
                                        {d.nombre}
                                    </MenuItem>
                                )
                            )}
                        </Select>
                    </FormControl>

                    <FormControl
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
                                formulario
                                    .ciudadId
                            }
                            label="Ciudad"
                            onChange={(e) =>
                                determinarTipo(
                                    e.target.value
                                )
                            }
                        >
                            <MenuItem value="">
                                Seleccione...
                            </MenuItem>
                            {ciudades.map(
                                (c) => (
                                    <MenuItem
                                        key={c.id}
                                        value={c.id}
                                    >
                                        {c.nombre}
                                    </MenuItem>
                                )
                            )}
                        </Select>
                    </FormControl>

                    <TextField
                        label="Dirección"
                        value={
                            formulario.direccion
                        }
                        onChange={(e) =>
                            setFormulario(
                                (a) => ({
                                    ...a,
                                    direccion:
                                        e.target.value
                                })
                            )
                        }
                        disabled={guardando}
                        required
                    />

                    <TextField
                        label="Tipo de Agencia"
                        value={tipoNombre}
                        disabled
                        helperText="Se determina automáticamente según la ciudad."
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

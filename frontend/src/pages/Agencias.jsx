import {
    useEffect,
    useState
} from "react";

import {
    Alert,
    Box,
    Button,
    InputAdornment,
    Snackbar,
    TextField,
    Typography
} from "@mui/material";

import AddIcon
    from "@mui/icons-material/Add";
import SearchIcon
    from "@mui/icons-material/Search";

import NuevaAgenciaModal
    from "../components/agencias/NuevaAgenciaModal";
import AgenciasTable
    from "../components/agencias/AgenciasTable";

import {
    obtenerAgencias,
    cambiarEstadoAgencia
} from "../services/agenciaService";

import {
    tienePermiso
} from "../services/permisos";

const mensajeError = (
    error,
    predeterminado
) =>
    error.response?.data?.mensaje ||
    error.response?.data?.message ||
    predeterminado;

export default function Agencias() {
    const [agencias, setAgencias] =
        useState([]);
    const [modalAbierto, setModalAbierto] =
        useState(false);
    const [
        agenciaSeleccionada,
        setAgenciaSeleccionada
    ] = useState(null);
    const [busqueda, setBusqueda] =
        useState("");
    const [error, setError] =
        useState("");
    const [mensaje, setMensaje] =
        useState("");

    const puedeCrear =
        tienePermiso("CREAR_AGENCIAS");
    const puedeEditar =
        tienePermiso("EDITAR_AGENCIAS");

    const cargarAgencias = async () => {
        try {
            const datos =
                await obtenerAgencias();

            setAgencias(
                Array.isArray(datos)
                    ? datos
                    : []
            );
        } catch (e) {
            setError(
                mensajeError(
                    e,
                    "No fue posible cargar las agencias."
                )
            );
        }
    };

    useEffect(() => {
        cargarAgencias();
    }, []);

    const cambiarEstado = async (
        agencia,
        nuevoEstado
    ) => {
        if (!puedeEditar) {
            return;
        }

        const accion =
            nuevoEstado
                ? "activar"
                : "desactivar";

        const detalle =
            !nuevoEstado
                ? " Los UPS de esta agencia que no estén De baja pasarán a En resguardo."
                : "";

        if (
            !window.confirm(
                `¿Desea ${accion} la agencia "${agencia.nombre}"?${detalle}`
            )
        ) {
            return;
        }

        try {
            const respuesta =
                await cambiarEstadoAgencia(
                    agencia.id,
                    nuevoEstado
                );

            setMensaje(
                respuesta?.mensaje ||
                "Estado actualizado correctamente."
            );

            await cargarAgencias();
        } catch (e) {
            setError(
                mensajeError(
                    e,
                    "No fue posible cambiar el estado de la agencia."
                )
            );
        }
    };

    const normalizar = (valor) =>
        String(valor ?? "")
            .toLowerCase()
            .trim();

    const texto =
        normalizar(busqueda);

    const filtradas =
        agencias.filter((a) => {
            if (!texto) return true;

            return [
                a.codigo,
                a.nombre,
                a.departamentoNombre,
                a.ciudadNombre,
                a.tipoAgenciaNombre
            ].some((valor) =>
                normalizar(valor)
                    .includes(texto)
            );
        });

    return (
        <Box>
            <Box
                sx={{
                    display: "flex",
                    justifyContent:
                        "space-between",
                    alignItems: "center",
                    mb: 3
                }}
            >
                <Typography
                    variant="h4"
                    fontWeight="bold"
                >
                    Gestión de Agencias
                </Typography>

                {puedeCrear && (
                    <Button
                        variant="contained"
                        startIcon={<AddIcon />}
                        onClick={() => {
                            setAgenciaSeleccionada(
                                null
                            );
                            setModalAbierto(
                                true
                            );
                        }}
                    >
                        Nueva Agencia
                    </Button>
                )}
            </Box>

            <TextField
                fullWidth
                sx={{ mb: 3 }}
                placeholder="Buscar por código, nombre, departamento, ciudad o tipo..."
                value={busqueda}
                onChange={(e) =>
                    setBusqueda(
                        e.target.value
                    )
                }
                slotProps={{
                    input: {
                        startAdornment: (
                            <InputAdornment
                                position="start"
                            >
                                <SearchIcon />
                            </InputAdornment>
                        )
                    }
                }}
            />

            <AgenciasTable
                agencias={filtradas}
                puedeEditar={puedeEditar}
                onEditar={(agencia) => {
                    setAgenciaSeleccionada(
                        agencia
                    );
                    setModalAbierto(true);
                }}
                onCambiarEstado={
                    cambiarEstado
                }
            />

            <NuevaAgenciaModal
                open={modalAbierto}
                agencia={
                    agenciaSeleccionada
                }
                onClose={() => {
                    setModalAbierto(false);
                    setAgenciaSeleccionada(
                        null
                    );
                }}
                onGuardado={async () => {
                    await cargarAgencias();

                    setMensaje(
                        agenciaSeleccionada
                            ? "Agencia actualizada correctamente."
                            : "Agencia creada correctamente."
                    );
                }}
            />

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

            <Snackbar
                open={Boolean(error)}
                autoHideDuration={6000}
                onClose={() =>
                    setError("")
                }
            >
                <Alert
                    severity="error"
                    onClose={() =>
                        setError("")
                    }
                >
                    {error}
                </Alert>
            </Snackbar>
        </Box>
    );
}

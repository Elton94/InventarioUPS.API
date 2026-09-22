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

import NuevaUPSModal
    from "../components/ups/NuevoUPSModal";
import UPSTable
    from "../components/ups/UPSTable";

import {
    obtenerUPS,
    cambiarEstadoUPS
} from "../services/upsService";
import {
    obtenerEstadosUPS
} from "../services/catalogosService";
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

export default function UPS() {
    const [ups, setUps] =
        useState([]);
    const [estados, setEstados] =
        useState([]);
    const [busqueda, setBusqueda] =
        useState("");
    const [modalAbierto, setModalAbierto] =
        useState(false);
    const [seleccionado, setSeleccionado] =
        useState(null);
    const [mensaje, setMensaje] =
        useState("");
    const [error, setError] =
        useState("");

    const puedeCrear =
        tienePermiso("CREAR_UPS");
    const puedeEditar =
        tienePermiso("EDITAR_UPS");

    const cargar = async () => {
        try {
            const [
                equipos,
                estadosData
            ] = await Promise.all([
                obtenerUPS(),
                obtenerEstadosUPS()
            ]);

            setUps(
                Array.isArray(equipos)
                    ? equipos
                    : []
            );
            setEstados(
                Array.isArray(estadosData)
                    ? estadosData
                    : []
            );
        } catch (e) {
            setError(
                mensajeError(
                    e,
                    "No fue posible cargar el inventario de UPS."
                )
            );
        }
    };

    useEffect(() => {
        cargar();
    }, []);

    const cambiarEstado = async (
        equipo,
        estado
    ) => {
        if (!puedeEditar) return;

        if (
            [
                "Fuera de servicio",
                "De baja"
            ].includes(estado)
        ) {
            if (
                !window.confirm(
                    `¿Desea cambiar el UPS "${equipo.numeroInventario}" a "${estado}"?`
                )
            ) {
                return;
            }
        }

        try {
            const respuesta =
                await cambiarEstadoUPS(
                    equipo.numeroInventario,
                    estado
                );

            setMensaje(
                respuesta?.mensaje ||
                "Estado actualizado correctamente."
            );

            await cargar();
        } catch (e) {
            setError(
                mensajeError(
                    e,
                    "No fue posible cambiar el estado del UPS."
                )
            );
        }
    };

    const texto =
        busqueda
            .toLowerCase()
            .trim();

    const filtrados =
        ups.filter((equipo) => {
            if (!texto) return true;

            return [
                equipo.numeroInventario,
                equipo.marca,
                equipo.modelo,
                equipo.numeroSerie,
                equipo.agenciaNombre,
                equipo.estado
            ].some((valor) =>
                String(valor || "")
                    .toLowerCase()
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
                    Gestión de UPS
                </Typography>

                {puedeCrear && (
                    <Button
                        variant="contained"
                        startIcon={<AddIcon />}
                        onClick={() => {
                            setSeleccionado(null);
                            setModalAbierto(true);
                        }}
                    >
                        Nuevo UPS
                    </Button>
                )}
            </Box>

            <TextField
                fullWidth
                sx={{ mb: 3 }}
                placeholder="Buscar por inventario, marca, modelo, serie, agencia o estado..."
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

            <UPSTable
                ups={filtrados}
                estados={estados}
                puedeEditar={puedeEditar}
                onEditar={(equipo) => {
                    setSeleccionado(equipo);
                    setModalAbierto(true);
                }}
                onCambiarEstado={cambiarEstado}
            />

            <NuevaUPSModal
                open={modalAbierto}
                ups={seleccionado}
                onClose={() => {
                    setModalAbierto(false);
                    setSeleccionado(null);
                }}
                onGuardado={async () => {
                    await cargar();
                    setMensaje(
                        seleccionado
                            ? "UPS actualizado correctamente."
                            : "UPS creado correctamente."
                    );
                }}
            />

            <Snackbar
                open={Boolean(mensaje)}
                autoHideDuration={4000}
                onClose={() => setMensaje("")}
            >
                <Alert
                    severity="success"
                    onClose={() => setMensaje("")}
                >
                    {mensaje}
                </Alert>
            </Snackbar>

            <Snackbar
                open={Boolean(error)}
                autoHideDuration={6000}
                onClose={() => setError("")}
            >
                <Alert
                    severity="error"
                    onClose={() => setError("")}
                >
                    {error}
                </Alert>
            </Snackbar>
        </Box>
    );
}

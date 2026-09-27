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
import SecurityIcon
    from "@mui/icons-material/Security";

import UsuariosTable
    from "../components/usuarios/UsuariosTable";
import NuevoUsuarioModal
    from "../components/usuarios/NuevoUsuarioModal";
import GestionarRolesModal
    from "../components/usuarios/GestionarRolesModal";

import {
    obtenerUsuarios,
    cambiarEstadoUsuario
} from "../services/usuarioService";

import {
    tienePermiso
} from "../services/permisos";

export default function Usuarios({ rolPredeterminado = "" }) {
    const [usuarios, setUsuarios] =
        useState([]);
    const [busqueda, setBusqueda] =
        useState("");
    const [modalAbierto, setModalAbierto] =
        useState(false);
    const [rolesAbierto, setRolesAbierto] =
        useState(false);
    const [seleccionado, setSeleccionado] =
        useState(null);
    const [mensaje, setMensaje] =
        useState("");
    const [error, setError] =
        useState("");

    const puedeUsuarios =
        tienePermiso(
            "GESTIONAR_USUARIOS"
        );
    const puedeRoles =
        tienePermiso(
            "GESTIONAR_ROLES_PERMISOS"
        );

    const usuarioActualId =
        Number(
            localStorage.getItem(
                "usuarioId"
            ) ||
            0
        );

    const cargar = async () => {
        if (!puedeUsuarios) {
            setUsuarios([]);
            return;
        }

        try {
            const datos =
                await obtenerUsuarios();

            setUsuarios(
                Array.isArray(datos)
                    ? datos
                    : []
            );
        } catch (e) {
            setError(
                e.response?.data?.mensaje ||
                "No fue posible cargar los usuarios."
            );
        }
    };

    useEffect(() => {
        cargar();
    }, []);

    const cambiarEstado = async (
        usuario
    ) => {
        if (!puedeUsuarios) return;

        if (
            Number(usuario.id) ===
            usuarioActualId &&
            usuario.activo
        ) {
            setError(
                "No puede desactivar su propio usuario."
            );
            return;
        }

        const accion =
            usuario.activo
                ? "desactivar"
                : "activar";

        if (
            !window.confirm(
                `¿Desea ${accion} al usuario "${usuario.nombreUsuario}"?`
            )
        ) {
            return;
        }

        try {
            const respuesta =
                await cambiarEstadoUsuario(
                    usuario.id,
                    !usuario.activo
                );

            setMensaje(
                respuesta?.mensaje ||
                "Estado actualizado correctamente."
            );

            await cargar();
        } catch (e) {
            setError(
                e.response?.data?.mensaje ||
                e.response?.data ||
                "No fue posible cambiar el estado."
            );
        }
    };

    const texto =
        busqueda
            .toLowerCase()
            .trim();

    const filtrados =
        usuarios.filter(
            (u) =>
                !texto ||
                [
                    u.nombreUsuario,
                    u.nombreCompleto,
                    u.correo,
                    u.rolNombre
                ].some((valor) =>
                    String(valor || "")
                        .toLowerCase()
                        .includes(texto)
                )
        );

    return (
        <Box>
            <Box
                sx={{
                    display: "flex",
                    justifyContent:
                        "space-between",
                    alignItems: "center",
                    gap: 2,
                    mb: 3,
                    flexWrap: "wrap"
                }}
            >
                <Typography
                    variant="h4"
                    fontWeight="bold"
                >
                    Gestión de Usuarios y Roles
                </Typography>

                <Box
                    sx={{
                        display: "flex",
                        gap: 1
                    }}
                >
                    {puedeRoles && (
                        <Button
                            variant="outlined"
                            startIcon={
                                <SecurityIcon />
                            }
                            onClick={() =>
                                setRolesAbierto(
                                    true
                                )
                            }
                        >
                            Gestionar Roles
                        </Button>
                    )}

                    {puedeUsuarios && (
                        <Button
                            variant="contained"
                            startIcon={
                                <AddIcon />
                            }
                            onClick={() => {
                                setSeleccionado(
                                    null
                                );
                                setModalAbierto(
                                    true
                                );
                            }}
                        >
                            Nuevo Usuario
                        </Button>
                    )}
                </Box>
            </Box>

            {puedeUsuarios ? (
                <>
                    <TextField
                        fullWidth
                        sx={{ mb: 3 }}
                        placeholder="Buscar por usuario, nombre, correo o rol..."
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

                    <UsuariosTable
                        usuarios={filtrados}
                        onEditar={(u) => {
                            setSeleccionado(u);
                            setModalAbierto(
                                true
                            );
                        }}
                        onCambiarEstado={
                            cambiarEstado
                        }
                        puedeGestionarUsuarios
                        puedeGestionarRoles={
                            puedeRoles
                        }
                        usuarioActualId={
                            usuarioActualId
                        }
                    />
                </>
            ) : (
                <Alert severity="info">
                    Su rol puede gestionar roles y permisos, pero no administrar usuarios.
                </Alert>
            )}

            {puedeUsuarios && (
                <NuevoUsuarioModal rolPredeterminado={rolPredeterminado}
                    open={modalAbierto}
                    usuario={seleccionado}
                    onClose={() => {
                        setModalAbierto(
                            false
                        );
                        setSeleccionado(
                            null
                        );
                    }}
                    onGuardado={async () => {
                        await cargar();
                        setMensaje(
                            seleccionado
                                ? "Usuario actualizado correctamente."
                                : "Usuario creado correctamente."
                        );
                    }}
                />
            )}

            {puedeRoles && (
                <GestionarRolesModal
                    open={rolesAbierto}
                    onClose={() =>
                        setRolesAbierto(
                            false
                        )
                    }
                />
            )}

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
                    {String(error)}
                </Alert>
            </Snackbar>
        </Box>
    );
}

import { useEffect, useState } from "react";

import {
    Alert,
    Box,
    Button,
    Checkbox,
    Chip,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    FormControlLabel,
    IconButton,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableRow,
    TextField,
    Typography
} from "@mui/material";

import EditIcon from "@mui/icons-material/Edit";
import AddIcon from "@mui/icons-material/Add";
import BlockIcon from "@mui/icons-material/Block";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import SaveIcon from "@mui/icons-material/Save";
import SecurityIcon from "@mui/icons-material/Security";

import {
    obtenerRoles,
    crearRol,
    actualizarRol,
    cambiarEstadoRol,
    obtenerPermisos,
    obtenerPermisosRol,
    actualizarPermisosRol
} from "../../services/usuarioService";

export default function GestionarRolesModal({
    open,
    onClose
}) {
    const [roles, setRoles] = useState([]);

    const [permisos, setPermisos] = useState([]);

    const [permisosSeleccionados, setPermisosSeleccionados] =
        useState([]);

    const [cargando, setCargando] =
        useState(false);

    const [cargandoPermisos, setCargandoPermisos] =
        useState(false);

    const [guardando, setGuardando] =
        useState(false);

    const [editando, setEditando] =
        useState(null);

    const [nombre, setNombre] =
        useState("");

    const [descripcion, setDescripcion] =
        useState("");

    const [error, setError] =
        useState("");

    const [mensaje, setMensaje] =
        useState("");

    const cargarRoles = async () => {
        setCargando(true);
        setError("");

        try {
            const datos =
                await obtenerRoles();

            setRoles(
                Array.isArray(datos)
                    ? datos
                    : []
            );
        } catch (error) {
            console.error(
                "Error al cargar roles:",
                error
            );

            setError(
                obtenerMensajeError(
                    error,
                    "No fue posible cargar los roles."
                )
            );
        } finally {
            setCargando(false);
        }
    };

    const cargarPermisos = async () => {
        setCargandoPermisos(true);
        setError("");

        try {
            const datos =
                await obtenerPermisos();

            setPermisos(
                Array.isArray(datos)
                    ? datos
                    : []
            );
        } catch (error) {
            console.error(
                "Error al cargar permisos:",
                error
            );

            setError(
                obtenerMensajeError(
                    error,
                    "No fue posible cargar los permisos."
                )
            );

            setPermisos([]);
        } finally {
            setCargandoPermisos(false);
        }
    };

    useEffect(() => {
        if (!open) {
            return;
        }

        limpiarFormulario();

        cargarRoles();

        cargarPermisos();
    }, [open]);

    const limpiarFormulario = () => {
        setEditando(null);
        setNombre("");
        setDescripcion("");
        setPermisosSeleccionados([]);
        setError("");
        setMensaje("");
    };

    const editar = async (rol) => {
        setEditando(rol);
        setNombre(
            rol.nombre || ""
        );
        setDescripcion(
            rol.descripcion || ""
        );

        setError("");
        setMensaje("");

        setCargandoPermisos(true);

        try {
            const datos =
                await obtenerPermisosRol(
                    rol.id
                );

            const ids =
                Array.isArray(datos)
                    ? datos.map(
                        (permiso) =>
                            Number(permiso.id)
                    )
                    : [];

            setPermisosSeleccionados(
                ids
            );
        } catch (error) {
            console.error(
                "Error al cargar permisos del rol:",
                error
            );

            setPermisosSeleccionados([]);

            setError(
                obtenerMensajeError(
                    error,
                    "No fue posible cargar los permisos del rol."
                )
            );
        } finally {
            setCargandoPermisos(false);
        }
    };

    const cambiarPermiso = (
        permisoId
    ) => {
        if (esControlTotal()) {
            return;
        }

        const id =
            Number(permisoId);

        setPermisosSeleccionados(
            (actuales) => {
                if (actuales.includes(id)) {
                    return actuales.filter(
                        (actual) =>
                            actual !== id
                    );
                }

                return [
                    ...actuales,
                    id
                ];
            }
        );
    };

    const seleccionarTodos = () => {
        if (esControlTotal()) {
            return;
        }

        setPermisosSeleccionados(
            permisos.map(
                (permiso) =>
                    Number(permiso.id)
            )
        );
    };

    const quitarTodos = () => {
        if (esControlTotal()) {
            return;
        }

        setPermisosSeleccionados([]);
    };

    const esControlTotal = () => {
        return (
            editando &&
            editando.nombre?.toLowerCase() ===
                "control total"
        );
    };

    const guardarRol = async () => {
        setError("");
        setMensaje("");

        if (!nombre.trim()) {
            setError(
                "El nombre del rol es obligatorio."
            );

            return;
        }

        if (nombre.trim().length < 3) {
            setError(
                "El nombre del rol debe tener al menos 3 caracteres."
            );

            return;
        }

        setGuardando(true);

        try {
            let rolId;

            if (editando) {
                await actualizarRol(
                    editando.id,
                    {
                        nombre:
                            nombre.trim(),

                        descripcion:
                            descripcion.trim(),

                        activo:
                            editando.activo
                    }
                );

                rolId =
                    editando.id;
            }

            else {
                const resultado =
                    await crearRol({
                        nombre:
                            nombre.trim(),

                        descripcion:
                            descripcion.trim()
                    });

                rolId =
                    resultado?.rolId;
            }

            if (!rolId) {
                throw new Error(
                    "No fue posible obtener el identificador del rol."
                );
            }

            await actualizarPermisosRol(
                rolId,
                permisosSeleccionados
            );

            setMensaje(
                "Rol y permisos guardados correctamente."
            );

            await cargarRoles();

            const rolActualizado =
                await obtenerRoles();

            const rolEncontrado =
                rolActualizado.find(
                    (rol) =>
                        Number(rol.id) ===
                        Number(rolId)
                );

            if (rolEncontrado) {
                await editar(
                    rolEncontrado
                );
            }
        } catch (error) {
            console.error(
                "Error al guardar rol y permisos:",
                error
            );

            setError(
                obtenerMensajeError(
                    error,
                    "No fue posible guardar el rol y sus permisos."
                )
            );
        } finally {
            setGuardando(false);
        }
    };

    const cambiarEstado = async (
        rol
    ) => {
        const accion =
            rol.activo
                ? "desactivar"
                : "activar";

        const confirmar =
            window.confirm(
                `¿Está seguro de que desea ${accion} el rol "${rol.nombre}"?`
            );

        if (!confirmar) {
            return;
        }

        setError("");
        setMensaje("");

        try {
            await cambiarEstadoRol(
                rol.id,
                !rol.activo
            );

            await cargarRoles();

            if (
                editando &&
                editando.id === rol.id
            ) {
                setEditando({
                    ...editando,
                    activo:
                        !rol.activo
                });
            }
        } catch (error) {
            console.error(
                "Error al cambiar estado del rol:",
                error
            );

            setError(
                obtenerMensajeError(
                    error,
                    "No fue posible cambiar el estado."
                )
            );
        }
    };

    const obtenerMensajeError = (
        error,
        mensajePredeterminado
    ) => {
        const data =
            error?.response?.data;

        if (
            typeof data ===
            "string"
        ) {
            return data;
        }

        if (
            typeof data?.mensaje ===
            "string"
        ) {
            return data.mensaje;
        }

        if (
            typeof error?.message ===
            "string"
        ) {
            return error.message;
        }

        return mensajePredeterminado;
    };

    return (
        <Dialog
            open={open}
            onClose={
                guardando
                    ? undefined
                    : onClose
            }
            fullWidth
            maxWidth="lg"
        >
            <DialogTitle>
                <Box
                    display="flex"
                    alignItems="center"
                    gap={1}
                >
                    <SecurityIcon />

                    <Typography
                        variant="h6"
                    >
                        Gestionar Roles y Permisos
                    </Typography>
                </Box>
            </DialogTitle>

            <DialogContent>

                

                {error && (
                    <Alert
                        severity="error"
                        sx={{ mb: 2 }}
                    >
                        {error}
                    </Alert>
                )}

                {mensaje && (
                    <Alert
                        severity="success"
                        sx={{ mb: 2 }}
                    >
                        {mensaje}
                    </Alert>
                )}

                

                <Paper
                    variant="outlined"
                    sx={{
                        p: 2,
                        mb: 3
                    }}
                >
                    <Typography
                        variant="subtitle1"
                        fontWeight="bold"
                        sx={{ mb: 2 }}
                    >
                        {editando
                            ? "Editar rol"
                            : "Crear nuevo rol"}
                    </Typography>

                    <Box
                        display="flex"
                        gap={2}
                        alignItems="center"
                        flexWrap="wrap"
                    >
                        <TextField
                            sx={{
                                flex: 1,
                                minWidth: 220
                            }}
                            label="Nombre del rol"
                            value={nombre}
                            disabled={
                                esControlTotal()
                            }
                            onChange={(e) =>
                                setNombre(
                                    e.target.value
                                )
                            }
                        />

                        <TextField
                            sx={{
                                flex: 2,
                                minWidth: 280
                            }}
                            label="Descripción"
                            value={descripcion}
                            onChange={(e) =>
                                setDescripcion(
                                    e.target.value
                                )
                            }
                        />

                        <Button
                            variant="contained"
                            startIcon={
                                editando
                                    ? <SaveIcon />
                                    : <AddIcon />
                            }
                            disabled={
                                guardando
                            }
                            onClick={
                                guardarRol
                            }
                        >
                            {editando
                                ? "Guardar"
                                : "Crear"}
                        </Button>

                        {editando && (
                            <Button
                                variant="outlined"
                                disabled={
                                    guardando
                                }
                                onClick={
                                    limpiarFormulario
                                }
                            >
                                Nuevo
                            </Button>
                        )}
                    </Box>

                    {editando && (
                        <Box
                            mt={2}
                            display="flex"
                            alignItems="center"
                            gap={1}
                        >
                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                Editando:
                            </Typography>

                            <Chip
                                label={
                                    editando.nombre
                                }
                                color="primary"
                                size="small"
                            />

                            {esControlTotal() && (
                                <Chip
                                    label="Acceso completo"
                                    color="success"
                                    size="small"
                                />
                            )}
                        </Box>
                    )}
                </Paper>

                

                {editando && (
                    <Paper
                        variant="outlined"
                        sx={{
                            p: 2,
                            mb: 3
                        }}
                    >
                        <Box
                            display="flex"
                            justifyContent="space-between"
                            alignItems="center"
                            mb={1}
                            flexWrap="wrap"
                            gap={1}
                        >
                            <Typography
                                variant="subtitle1"
                                fontWeight="bold"
                            >
                                Permisos del rol
                            </Typography>

                            {!esControlTotal() && (
                                <Box
                                    display="flex"
                                    gap={1}
                                >
                                    <Button
                                        size="small"
                                        variant="outlined"
                                        onClick={
                                            seleccionarTodos
                                        }
                                        disabled={
                                            cargandoPermisos ||
                                            guardando
                                        }
                                    >
                                        Seleccionar todos
                                    </Button>

                                    <Button
                                        size="small"
                                        variant="outlined"
                                        onClick={
                                            quitarTodos
                                        }
                                        disabled={
                                            cargandoPermisos ||
                                            guardando
                                        }
                                    >
                                        Quitar todos
                                    </Button>
                                </Box>
                            )}
                        </Box>

                        <Divider
                            sx={{ mb: 2 }}
                        />

                        {esControlTotal() && (
                            <Alert
                                severity="info"
                                sx={{ mb: 2 }}
                            >
                                Control Total incluye todos los permisos.
                            </Alert>
                        )}

                        {cargandoPermisos && (
                            <Typography
                                color="text.secondary"
                            >
                                Cargando permisos...
                            </Typography>
                        )}

                        {!cargandoPermisos &&
                            permisos.length === 0 && (
                                <Typography
                                    color="text.secondary"
                                >
                                    No hay permisos disponibles.
                                </Typography>
                            )}

                        {!cargandoPermisos &&
                            permisos.length > 0 && (
                                <Box
                                    sx={{
                                        display: "grid",
                                        gridTemplateColumns:
                                            "repeat(auto-fit, minmax(280px, 1fr))",
                                        gap: 1
                                    }}
                                >
                                    {permisos.map(
                                        (permiso) => {
                                            const permisoId =
                                                Number(
                                                    permiso.id
                                                );

                                            const seleccionado =
                                                esControlTotal() ||
                                                permisosSeleccionados.includes(
                                                    permisoId
                                                );

                                            return (
                                                <Paper
                                                    key={
                                                        permisoId
                                                    }
                                                    variant="outlined"
                                                    sx={{
                                                        p: 1,
                                                        borderRadius: 2
                                                    }}
                                                >
                                                    <FormControlLabel
                                                        control={
                                                            <Checkbox
                                                                checked={
                                                                    seleccionado
                                                                }
                                                                disabled={
                                                                    esControlTotal() ||
                                                                    guardando
                                                                }
                                                                onChange={() =>
                                                                    cambiarPermiso(
                                                                        permisoId
                                                                    )
                                                                }
                                                            />
                                                        }
                                                        label={
                                                            <Box>
                                                                <Typography
                                                                    variant="body2"
                                                                    fontWeight="bold"
                                                                >
                                                                    {
                                                                        permiso.nombre
                                                                    }
                                                                </Typography>

                                                            </Box>
                                                        }
                                                    />
                                                </Paper>
                                            );
                                        }
                                    )}
                                </Box>
                            )}

                        {editando &&
                            !esControlTotal() &&
                            permisos.length > 0 && (
                                <Box mt={2}>
                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                    >
                                        Permisos seleccionados:{" "}
                                        <strong>
                                            {
                                                permisosSeleccionados.length
                                            }
                                        </strong>
                                    </Typography>
                                </Box>
                            )}
                    </Paper>
                )}

                

                <Typography
                    variant="subtitle1"
                    fontWeight="bold"
                    sx={{ mb: 1 }}
                >
                    Roles registrados
                </Typography>

                <Paper
                    variant="outlined"
                    sx={{
                        overflow: "auto"
                    }}
                >
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell>
                                    <strong>Rol</strong>
                                </TableCell>

                                <TableCell>
                                    <strong>Descripción</strong>
                                </TableCell>

                                <TableCell align="center">
                                    <strong>Estado</strong>
                                </TableCell>

                                <TableCell align="center">
                                    <strong>Acciones</strong>
                                </TableCell>
                            </TableRow>
                        </TableHead>

                        <TableBody>
                            {cargando && (
                                <TableRow>
                                    <TableCell
                                        colSpan={4}
                                        align="center"
                                    >
                                        Cargando roles...
                                    </TableCell>
                                </TableRow>
                            )}

                            {!cargando &&
                                roles.map(
                                    (rol) => (
                                        <TableRow
                                            key={
                                                rol.id
                                            }
                                            hover
                                        >
                                            <TableCell>
                                                <Typography
                                                    fontWeight="bold"
                                                >
                                                    {
                                                        rol.nombre
                                                    }
                                                </Typography>
                                            </TableCell>

                                            <TableCell>
                                                {
                                                    rol.descripcion
                                                }
                                            </TableCell>

                                            <TableCell align="center">
                                                <Chip
                                                    label={
                                                        rol.activo
                                                            ? "Activo"
                                                            : "Inactivo"
                                                    }
                                                    color={
                                                        rol.activo
                                                            ? "success"
                                                            : "default"
                                                    }
                                                    size="small"
                                                />
                                            </TableCell>

                                            <TableCell align="center">
                                                <IconButton
                                                    color="primary"
                                                    disabled={
                                                        guardando
                                                    }
                                                    onClick={() =>
                                                        editar(
                                                            rol
                                                        )
                                                    }
                                                    title="Editar rol y permisos"
                                                >
                                                    <EditIcon />
                                                </IconButton>

                                                <IconButton
                                                    color={
                                                        rol.activo
                                                            ? "error"
                                                            : "success"
                                                    }
                                                    disabled={
                                                        guardando
                                                    }
                                                    onClick={() =>
                                                        cambiarEstado(
                                                            rol
                                                        )
                                                    }
                                                    title={
                                                        rol.activo
                                                            ? "Desactivar rol"
                                                            : "Activar rol"
                                                    }
                                                >
                                                    {rol.activo ? (
                                                        <BlockIcon />
                                                    ) : (
                                                        <CheckCircleIcon />
                                                    )}
                                                </IconButton>
                                            </TableCell>
                                        </TableRow>
                                    )
                                )}

                            {!cargando &&
                                roles.length === 0 && (
                                    <TableRow>
                                        <TableCell
                                            colSpan={4}
                                            align="center"
                                        >
                                            No hay roles registrados.
                                        </TableCell>
                                    </TableRow>
                                )}
                        </TableBody>
                    </Table>
                </Paper>

            </DialogContent>

            <DialogActions>
                <Button
                    onClick={onClose}
                    disabled={guardando}
                >
                    Cerrar
                </Button>
            </DialogActions>
        </Dialog>
    );
}
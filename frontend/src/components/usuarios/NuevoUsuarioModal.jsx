import { useEffect, useState } from "react";

import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    TextField,
    Typography
} from "@mui/material";

import {
    crearUsuario,
    actualizarUsuario,
    obtenerRoles
} from "../../services/usuarioService";

export default function NuevoUsuarioModal({
    open,
    onClose,
    onGuardado,
    usuario
}) {
    const [roles, setRoles] = useState([]);
    const [cargandoRoles, setCargandoRoles] =
        useState(false);
    const [guardando, setGuardando] =
        useState(false);

    const [formulario, setFormulario] = useState({
        nombreUsuario: "",
        nombreCompleto: "",
        correo: "",
        password: "",
        rolId: "",
        activo: true
    });

    const [error, setError] = useState("");

    useEffect(() => {
        if (!open) {
            return;
        }

        setError("");
        cargarRoles();

        if (usuario) {
            setFormulario({
                nombreUsuario:
                    usuario.nombreUsuario || "",
                nombreCompleto:
                    usuario.nombreCompleto || "",
                correo:
                    usuario.correo || "",
                password: "",
                rolId:
                    usuario.rolId
                        ? Number(usuario.rolId)
                        : "",
                activo:
                    usuario.activo ?? true
            });
        } else {
            setFormulario({
                nombreUsuario: "",
                nombreCompleto: "",
                correo: "",
                password: "",
                rolId: "",
                activo: true
            });
        }
    }, [open, usuario]);

    const cargarRoles = async () => {
        setCargandoRoles(true);
        setError("");

        try {
            const datos = await obtenerRoles();

            console.log("Roles recibidos:", datos);

            setRoles(Array.isArray(datos) ? datos : []);

        } catch (error) {
            console.error(
                "Error al cargar roles:",
                error
            );

            if (error.response?.status === 401) {
                setError(
                    "La sesión no es válida. Vuelva a iniciar sesión."
                );
            } else if (error.response?.status === 403) {
                setError(
                    "No tiene permisos para consultar los roles."
                );
            } else {
                setError(
                    "No fue posible cargar los roles."
                );
            }

            setRoles([]);
        } finally {
            setCargandoRoles(false);
        }
    };

    const cambiarCampo = (campo, valor) => {
        setFormulario((actual) => ({
            ...actual,
            [campo]: valor
        }));
    };

    const guardar = async () => {
        setError("");

        if (!formulario.nombreUsuario.trim() && !usuario) {
            setError(
                "El nombre de usuario es obligatorio."
            );
            return;
        }

        if (!formulario.nombreCompleto.trim()) {
            setError(
                "El nombre completo es obligatorio."
            );
            return;
        }

        if (!formulario.correo.trim()) {
            setError(
                "El correo electrónico es obligatorio."
            );
            return;
        }

        if (!formulario.rolId) {
            setError(
                "Debe seleccionar un rol."
            );
            return;
        }

        if (
            !usuario &&
            !formulario.password.trim()
        ) {
            setError(
                "La contraseña es obligatoria."
            );
            return;
        }

        setGuardando(true);

        try {
            if (!usuario) {
                await crearUsuario({
                    nombreUsuario:
                        formulario.nombreUsuario.trim(),

                    nombreCompleto:
                        formulario.nombreCompleto.trim(),

                    correo:
                        formulario.correo.trim(),

                    password:
                        formulario.password,

                    rolId:
                        Number(formulario.rolId)
                });
            } else {
                await actualizarUsuario(
                    usuario.id,
                    {
                        nombreCompleto:
                            formulario.nombreCompleto.trim(),

                        correo:
                            formulario.correo.trim(),

                        rolId:
                            Number(formulario.rolId),

                        activo:
                            formulario.activo
                    }
                );
            }

            await onGuardado();

            onClose();

        } catch (error) {
            console.error(
                "Error al guardar usuario:",
                error
            );

            const mensaje =
                error.response?.data?.mensaje ||
                error.response?.data ||
                "No fue posible guardar el usuario.";

            setError(
                typeof mensaje === "string"
                    ? mensaje
                    : "No fue posible guardar el usuario."
            );
        } finally {
            setGuardando(false);
        }
    };

    return (
        <Dialog
            open={open}
            onClose={guardando ? undefined : onClose}
            fullWidth
            maxWidth="sm"
        >
            <DialogTitle>
                {usuario
                    ? "Editar usuario"
                    : "Nuevo usuario"}
            </DialogTitle>

            <DialogContent>

                {!usuario && (
                    <TextField
                        fullWidth
                        required
                        margin="normal"
                        label="Nombre de usuario"
                        value={
                            formulario.nombreUsuario
                        }
                        onChange={(e) =>
                            cambiarCampo(
                                "nombreUsuario",
                                e.target.value
                            )
                        }
                    />
                )}

                {usuario && (
                    <TextField
                        fullWidth
                        margin="normal"
                        label="Nombre de usuario"
                        value={
                            formulario.nombreUsuario
                        }
                        disabled
                    />
                )}

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Nombre completo"
                    value={
                        formulario.nombreCompleto
                    }
                    onChange={(e) =>
                        cambiarCampo(
                            "nombreCompleto",
                            e.target.value
                        )
                    }
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Correo electrónico"
                    type="email"
                    value={
                        formulario.correo
                    }
                    onChange={(e) =>
                        cambiarCampo(
                            "correo",
                            e.target.value
                        )
                    }
                />

                {!usuario && (
                    <TextField
                        fullWidth
                        required
                        margin="normal"
                        label="Contraseña"
                        type="password"
                        value={
                            formulario.password
                        }
                        onChange={(e) =>
                            cambiarCampo(
                                "password",
                                e.target.value
                            )
                        }
                    />
                )}

                <FormControl
                    fullWidth
                    required
                    margin="normal"
                >
                    <InputLabel id="rol-label">
                        Rol
                    </InputLabel>

                    <Select
                        labelId="rol-label"
                        value={formulario.rolId}
                        label="Rol"
                        disabled={cargandoRoles}
                        onChange={(e) =>
                            cambiarCampo(
                                "rolId",
                                Number(e.target.value)
                            )
                        }
                    >
                        <MenuItem value="">
                            <em>
                                Seleccione un rol
                            </em>
                        </MenuItem>

                        {roles.map((rol) => (
                            <MenuItem
                                key={rol.id}
                                value={Number(rol.id)}
                            >
                                {rol.nombre}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                {cargandoRoles && (
                    <Typography
                        variant="body2"
                        color="text.secondary"
                        sx={{ mt: 1 }}
                    >
                        Cargando roles...
                    </Typography>
                )}

                {usuario && (
                    <FormControl
                        fullWidth
                        margin="normal"
                    >
                        <InputLabel id="estado-label">
                            Estado
                        </InputLabel>

                        <Select
                            labelId="estado-label"
                            value={
                                formulario.activo
                            }
                            label="Estado"
                            onChange={(e) =>
                                cambiarCampo(
                                    "activo",
                                    e.target.value
                                )
                            }
                        >
                            <MenuItem value={true}>
                                Activo
                            </MenuItem>

                            <MenuItem value={false}>
                                Inactivo
                            </MenuItem>
                        </Select>
                    </FormControl>
                )}

                {error && (
                    <Typography
                        color="error"
                        sx={{
                            mt: 2
                        }}
                    >
                        {error}
                    </Typography>
                )}

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
                        guardando ||
                        cargandoRoles
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
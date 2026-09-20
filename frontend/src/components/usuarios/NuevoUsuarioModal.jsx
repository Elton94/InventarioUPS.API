import {
    useEffect,
    useState
} from "react";

import {
    Alert,
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
    crearUsuario,
    actualizarUsuario,
    obtenerRoles
} from "../../services/usuarioService";

import {
    tienePermiso
} from "../../services/permisos";

const inicial = {
    nombreUsuario: "",
    nombreCompleto: "",
    correo: "",
    password: "",
    nuevaPassword: "",
    rolId: "",
    activo: true
};

export default function NuevoUsuarioModal({
    open,
    onClose,
    onGuardado,
    usuario
}) {
    const [roles, setRoles] =
        useState([]);
    const [formulario, setFormulario] =
        useState(inicial);
    const [cargando, setCargando] =
        useState(false);
    const [guardando, setGuardando] =
        useState(false);
    const [error, setError] =
        useState("");

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

    const esMismoUsuario =
        usuario &&
        Number(usuario.id) ===
            usuarioActualId;

    useEffect(() => {
        if (!open) return;

        const cargar = async () => {
            try {
                setCargando(true);
                setError("");

                const datos =
                    await obtenerRoles();

                const disponibles =
                    (datos || [])
                        .filter(
                            (rol) =>
                                rol.activo
                        )
                        .filter(
                            (rol) =>
                                puedeRoles ||
                                String(
                                    rol.nombre ||
                                    ""
                                )
                                    .toLowerCase() !==
                                "control total"
                        );

                setRoles(disponibles);

                if (usuario) {
                    setFormulario({
                        nombreUsuario:
                            usuario.nombreUsuario ||
                            "",
                        nombreCompleto:
                            usuario.nombreCompleto ||
                            "",
                        correo:
                            usuario.correo ||
                            "",
                        password: "",
                        nuevaPassword: "",
                        rolId:
                            usuario.rolId ||
                            "",
                        activo:
                            usuario.activo !==
                            false
                    });
                } else {
                    setFormulario({
                        ...inicial
                    });
                }
            } catch (e) {
                setError(
                    e.response?.data?.mensaje ||
                    "No fue posible cargar los roles."
                );
            } finally {
                setCargando(false);
            }
        };

        cargar();
    }, [
        open,
        usuario,
        puedeRoles
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

    const guardar = async () => {
        setError("");

        if (
            !usuario &&
            !formulario
                .nombreUsuario
                .trim()
        ) {
            setError(
                "El nombre de usuario es obligatorio."
            );
            return;
        }

        if (
            !formulario
                .nombreCompleto
                .trim()
        ) {
            setError(
                "El nombre completo es obligatorio."
            );
            return;
        }

        if (!formulario.correo.trim()) {
            setError(
                "El correo es obligatorio."
            );
            return;
        }

        if (!formulario.rolId) {
            setError(
                "Seleccione un rol."
            );
            return;
        }

        if (
            !usuario &&
            !formulario.password
        ) {
            setError(
                "La contraseña es obligatoria."
            );
            return;
        }

        try {
            setGuardando(true);

            if (usuario) {
                await actualizarUsuario(
                    usuario.id,
                    {
                        nombreCompleto:
                            formulario
                                .nombreCompleto
                                .trim(),
                        correo:
                            formulario
                                .correo
                                .trim(),
                        rolId:
                            Number(
                                formulario
                                    .rolId
                            ),
                        activo:
                            formulario.activo,
                        nuevaPassword:
                            formulario
                                .nuevaPassword ||
                            null
                    }
                );
            } else {
                await crearUsuario({
                    nombreUsuario:
                        formulario
                            .nombreUsuario
                            .trim(),
                    nombreCompleto:
                        formulario
                            .nombreCompleto
                            .trim(),
                    correo:
                        formulario
                            .correo
                            .trim(),
                    password:
                        formulario
                            .password,
                    rolId:
                        Number(
                            formulario
                                .rolId
                        )
                });
            }

            await onGuardado();
            onClose();
        } catch (e) {
            const data =
                e.response?.data;

            setError(
                typeof data === "string"
                    ? data
                    : data?.mensaje ||
                      "No fue posible guardar el usuario."
            );
        } finally {
            setGuardando(false);
        }
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
            maxWidth="sm"
        >
            <DialogTitle>
                {usuario
                    ? "Editar usuario"
                    : "Nuevo usuario"}
            </DialogTitle>

            <DialogContent>
                {error && (
                    <Alert
                        severity="error"
                        sx={{ mt: 1 }}
                    >
                        {error}
                    </Alert>
                )}

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Nombre de usuario"
                    value={
                        formulario
                            .nombreUsuario
                    }
                    disabled={Boolean(usuario)}
                    onChange={(e) =>
                        cambiar(
                            "nombreUsuario",
                            e.target.value
                        )
                    }
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Nombre completo"
                    value={
                        formulario
                            .nombreCompleto
                    }
                    onChange={(e) =>
                        cambiar(
                            "nombreCompleto",
                            e.target.value
                        )
                    }
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    type="email"
                    label="Correo electrónico"
                    value={formulario.correo}
                    onChange={(e) =>
                        cambiar(
                            "correo",
                            e.target.value
                        )
                    }
                />

                {!usuario ? (
                    <TextField
                        fullWidth
                        required
                        margin="normal"
                        type="password"
                        label="Contraseña"
                        value={
                            formulario.password
                        }
                        onChange={(e) =>
                            cambiar(
                                "password",
                                e.target.value
                            )
                        }
                        helperText="Mínimo 8 caracteres, mayúscula, minúscula, número y carácter especial."
                    />
                ) : (
                    <TextField
                        fullWidth
                        margin="normal"
                        type="password"
                        label="Nueva contraseña"
                        value={
                            formulario
                                .nuevaPassword
                        }
                        onChange={(e) =>
                            cambiar(
                                "nuevaPassword",
                                e.target.value
                            )
                        }
                        helperText="Déjela vacía si no desea cambiarla."
                    />
                )}

                <FormControl
                    fullWidth
                    required
                    margin="normal"
                >
                    <InputLabel>
                        Rol
                    </InputLabel>
                    <Select
                        value={
                            formulario.rolId
                        }
                        label="Rol"
                        disabled={cargando}
                        onChange={(e) =>
                            cambiar(
                                "rolId",
                                Number(
                                    e.target.value
                                )
                            )
                        }
                    >
                        {roles.map(
                            (rol) => (
                                <MenuItem
                                    key={rol.id}
                                    value={rol.id}
                                >
                                    {rol.nombre}
                                </MenuItem>
                            )
                        )}
                    </Select>
                </FormControl>

                {usuario && (
                    <FormControl
                        fullWidth
                        margin="normal"
                    >
                        <InputLabel>
                            Estado
                        </InputLabel>
                        <Select
                            value={
                                formulario.activo
                            }
                            label="Estado"
                            disabled={
                                esMismoUsuario
                            }
                            onChange={(e) =>
                                cambiar(
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

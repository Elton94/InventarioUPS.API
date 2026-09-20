import { useState } from "react";
import {
    Navigate,
    useNavigate
} from "react-router-dom";

import {
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    CircularProgress,
    TextField,
    Typography
} from "@mui/material";

import api from "../services/api";
import {
    obtenerRutaInicial
} from "../services/permisos";

export default function Login() {
    const navigate = useNavigate();

    const [nombreUsuario, setNombreUsuario] =
        useState("");
    const [password, setPassword] =
        useState("");
    const [cargando, setCargando] =
        useState(false);
    const [error, setError] =
        useState("");

    if (localStorage.getItem("token")) {
        return (
            <Navigate
                to={obtenerRutaInicial()}
                replace
            />
        );
    }

    const iniciarSesion = async (e) => {
        e.preventDefault();
        setError("");

        if (!nombreUsuario.trim()) {
            setError(
                "Ingrese el nombre de usuario."
            );
            return;
        }

        if (!password) {
            setError(
                "Ingrese la contraseña."
            );
            return;
        }

        try {
            setCargando(true);

            const { data } =
                await api.post(
                    "/Auth/login",
                    {
                        nombreUsuario:
                            nombreUsuario.trim(),
                        password
                    }
                );

            if (!data?.token) {
                setError(
                    "El servidor no devolvió una sesión válida."
                );
                return;
            }

            localStorage.setItem(
                "token",
                data.token
            );
            localStorage.setItem(
                "usuarioId",
                String(data.usuarioId)
            );
            localStorage.setItem(
                "nombreUsuario",
                data.nombreUsuario || ""
            );
            localStorage.setItem(
                "nombreCompleto",
                data.nombreCompleto || ""
            );
            localStorage.setItem(
                "rol",
                data.rol || ""
            );

            navigate(
                obtenerRutaInicial(),
                { replace: true }
            );
        } catch (errorLogin) {
            const mensaje =
                errorLogin.response?.data
                    ?.mensaje;

            setError(
                mensaje ||
                "No fue posible iniciar sesión."
            );
        } finally {
            setCargando(false);
        }
    };

    return (
        <Box
            sx={{
                minHeight: "100vh",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                background:
                    "linear-gradient(135deg, #eef2ff 0%, #f8fafc 100%)",
                p: 2
            }}
        >
            <Card
                elevation={8}
                sx={{
                    width: "100%",
                    maxWidth: 430,
                    borderRadius: 3
                }}
            >
                <CardContent sx={{ p: 4 }}>
                    <Box
                        sx={{
                            textAlign: "center",
                            mb: 4
                        }}
                    >
                        <Typography
                            variant="h4"
                            fontWeight="bold"
                            color="primary"
                        >
                            UPS Asset Manager
                        </Typography>
                        <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{ mt: 1 }}
                        >
                            Gestión Corporativa
                        </Typography>
                    </Box>

                    <Typography
                        variant="h6"
                        fontWeight="bold"
                        sx={{ mb: 3 }}
                    >
                        Iniciar sesión
                    </Typography>

                    {error && (
                        <Alert
                            severity="error"
                            sx={{ mb: 2 }}
                        >
                            {error}
                        </Alert>
                    )}

                    <Box
                        component="form"
                        onSubmit={iniciarSesion}
                    >
                        <TextField
                            fullWidth
                            label="Nombre de usuario"
                            value={nombreUsuario}
                            onChange={(e) =>
                                setNombreUsuario(
                                    e.target.value
                                )
                            }
                            disabled={cargando}
                            autoComplete="username"
                            autoFocus
                            sx={{ mb: 2 }}
                        />

                        <TextField
                            fullWidth
                            label="Contraseña"
                            type="password"
                            value={password}
                            onChange={(e) =>
                                setPassword(
                                    e.target.value
                                )
                            }
                            disabled={cargando}
                            autoComplete="current-password"
                            sx={{ mb: 3 }}
                        />

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            size="large"
                            disabled={cargando}
                            sx={{
                                py: 1.4,
                                fontWeight: "bold"
                            }}
                        >
                            {cargando ? (
                                <>
                                    <CircularProgress
                                        size={22}
                                        color="inherit"
                                        sx={{ mr: 1 }}
                                    />
                                    Ingresando...
                                </>
                            ) : (
                                "Iniciar sesión"
                            )}
                        </Button>
                    </Box>
                </CardContent>
            </Card>
        </Box>
    );
}

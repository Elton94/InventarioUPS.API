import { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

import {
    Box,
    Button,
    Card,
    CardContent,
    CircularProgress,
    TextField,
    Typography,
    Alert
} from "@mui/material";

const API_URL =
    "https://localhost:7295/api/Auth/login";

export default function Login() {

    const navigate =
        useNavigate();

    const [
        nombreUsuario,
        setNombreUsuario
    ] = useState("");

    const [
        password,
        setPassword
    ] = useState("");

    const [
        cargando,
        setCargando
    ] = useState(false);

    const [
        error,
        setError
    ] = useState("");

    // =====================================================
    // DECODIFICAR PERMISOS DEL JWT
    // =====================================================

    const obtenerPermisosDesdeToken = (
        token
    ) => {

        try {

            const partes =
                token.split(".");

            if (partes.length !== 3) {
                return [];
            }

            let base64 =
                partes[1]
                    .replace(/-/g, "+")
                    .replace(/_/g, "/");

            while (
                base64.length % 4 !== 0
            ) {
                base64 += "=";
            }

            const json =
                decodeURIComponent(
                    atob(base64)
                        .split("")
                        .map(
                            (caracter) =>
                                "%" +
                                (
                                    "00" +
                                    caracter
                                        .charCodeAt(0)
                                        .toString(16)
                                ).slice(-2)
                        )
                        .join("")
                );

            const payload =
                JSON.parse(json);

            let permisos =
                payload.Permiso || [];

            if (
                !Array.isArray(permisos)
            ) {
                permisos = [permisos];
            }

            return permisos;

        } catch (error) {

            console.error(
                "No fue posible obtener los permisos del token:",
                error
            );

            return [];
        }
    };

    // =====================================================
    // INICIAR SESIÓN
    // =====================================================

    const iniciarSesion = async (
        e
    ) => {

        e.preventDefault();

        setError("");

        if (
            !nombreUsuario.trim()
        ) {
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

            const response =
                await axios.post(
                    API_URL,
                    {
                        nombreUsuario:
                            nombreUsuario.trim(),

                        password:
                            password
                    }
                );

            const datos =
                response.data;

            if (!datos.token) {
                setError(
                    "El servidor no devolvió un token de autenticación."
                );

                return;
            }

            // -------------------------------------------------
            // GUARDAR SESIÓN
            // -------------------------------------------------

            localStorage.setItem(
                "token",
                datos.token
            );

            localStorage.setItem(
                "usuarioId",
                String(
                    datos.usuarioId
                )
            );

            localStorage.setItem(
                "nombreUsuario",
                datos.nombreUsuario ||
                    ""
            );

            localStorage.setItem(
                "nombreCompleto",
                datos.nombreCompleto ||
                    ""
            );

            localStorage.setItem(
                "rol",
                datos.rol || ""
            );

            // -------------------------------------------------
            // GUARDAR PERMISOS
            // -------------------------------------------------

            const permisos =
                obtenerPermisosDesdeToken(
                    datos.token
                );

            localStorage.setItem(
                "permisos",
                JSON.stringify(
                    permisos
                )
            );

            navigate(
                "/inicio",
                {
                    replace: true
                }
            );

        } catch (error) {

            console.error(
                "Error al iniciar sesión:",
                error
            );

            if (
                error.response?.status ===
                401
            ) {

                setError(
                    "Usuario o contraseña incorrectos."
                );

            } else if (
                error.response?.data
            ) {

                const mensaje =
                    typeof error.response.data ===
                    "string"
                        ? error.response.data
                        : error.response.data?.message ||
                          error.response.data?.mensaje;

                setError(
                    mensaje ||
                    "No fue posible iniciar sesión."
                );

            } else {

                setError(
                    "No se pudo conectar con el servidor."
                );
            }

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
                padding: 2
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

                <CardContent
                    sx={{ p: 4 }}
                >

                    <Box
                        sx={{
                            textAlign:
                                "center",
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
                        onSubmit={
                            iniciarSesion
                        }
                    >

                        <TextField
                            fullWidth
                            label="Nombre de usuario"
                            value={
                                nombreUsuario
                            }
                            onChange={(e) =>
                                setNombreUsuario(
                                    e.target.value
                                )
                            }
                            disabled={
                                cargando
                            }
                            autoComplete="username"
                            autoFocus
                            sx={{ mb: 2 }}
                        />

                        <TextField
                            fullWidth
                            label="Contraseña"
                            type="password"
                            value={
                                password
                            }
                            onChange={(e) =>
                                setPassword(
                                    e.target.value
                                )
                            }
                            disabled={
                                cargando
                            }
                            autoComplete="current-password"
                            sx={{ mb: 3 }}
                        />

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            size="large"
                            disabled={
                                cargando
                            }
                            sx={{
                                py: 1.4,
                                fontWeight:
                                    "bold"
                            }}
                        >

                            {cargando ? (
                                <>
                                    <CircularProgress
                                        size={22}
                                        color="inherit"
                                        sx={{
                                            mr: 1
                                        }}
                                    />

                                    Ingresando...
                                </>
                            ) : (
                                "Iniciar sesión"
                            )}

                        </Button>

                    </Box>

                    <Typography
                        variant="caption"
                        color="text.secondary"
                        display="block"
                        textAlign="center"
                        sx={{ mt: 3 }}
                    >
                        Sistema de Inventario y Gestión de UPS
                    </Typography>

                </CardContent>

            </Card>

        </Box>
    );
}
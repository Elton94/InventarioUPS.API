import { useEffect, useState } from "react";

import {
    Box,
    Button,
    InputAdornment,
    TextField,
    Typography
} from "@mui/material";

import AddIcon from "@mui/icons-material/Add";
import SearchIcon from "@mui/icons-material/Search";
import SecurityIcon from "@mui/icons-material/Security";

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


export default function Usuarios() {

    const [usuarios, setUsuarios] =
        useState([]);

    const [busqueda, setBusqueda] =
        useState("");

    const [modalAbierto, setModalAbierto] =
        useState(false);

    const [rolesModalAbierto, setRolesModalAbierto] =
        useState(false);

    const [usuarioSeleccionado, setUsuarioSeleccionado] =
        useState(null);


    // =====================================================
    // PERMISOS
    // =====================================================

    const puedeGestionarUsuarios =
        tienePermiso(
            "GESTIONAR_USUARIOS"
        );

    const puedeGestionarRoles =
        tienePermiso(
            "GESTIONAR_ROLES_PERMISOS"
        );


    // =====================================================
    // CARGAR USUARIOS
    // =====================================================

    const cargarUsuarios = async () => {

        try {

            const datos =
                await obtenerUsuarios();

            setUsuarios(
                datos
            );

        } catch (error) {

            console.error(
                "Error al cargar usuarios:",
                error
            );

        }

    };


    useEffect(() => {

        cargarUsuarios();

    }, []);


    // =====================================================
    // EDITAR USUARIO
    // =====================================================

    const editarUsuario = (
        usuario
    ) => {

        if (!puedeGestionarUsuarios) {
            return;
        }

        setUsuarioSeleccionado(
            usuario
        );

        setModalAbierto(
            true
        );

    };


    // =====================================================
    // CAMBIAR ESTADO
    // =====================================================

    const cambiarEstado = async (
        usuario
    ) => {

        if (!puedeGestionarUsuarios) {
            return;
        }

        const accion =
            usuario.activo
                ? "desactivar"
                : "activar";

        const confirmar =
            window.confirm(
                `¿Está seguro de que desea ${accion} al usuario ${usuario.nombreUsuario}?`
            );

        if (!confirmar) {
            return;
        }

        try {

            await cambiarEstadoUsuario(
                usuario.id,
                !usuario.activo
            );

            await cargarUsuarios();

        } catch (error) {

            console.error(
                "Error al cambiar estado:",
                error
            );

        }

    };


    // =====================================================
    // BUSCAR
    // =====================================================

    const usuariosFiltrados =
        usuarios.filter(
            (usuario) => {

                const texto =
                    busqueda
                        .toLowerCase()
                        .trim();

                if (!texto) {
                    return true;
                }

                return (

                    usuario.nombreUsuario
                        ?.toLowerCase()
                        .includes(texto)

                    ||

                    usuario.nombreCompleto
                        ?.toLowerCase()
                        .includes(texto)

                    ||

                    usuario.correo
                        ?.toLowerCase()
                        .includes(texto)

                    ||

                    usuario.rolNombre
                        ?.toLowerCase()
                        .includes(texto)

                );

            }
        );


    // =====================================================
    // INTERFAZ
    // =====================================================

    return (

        <Box>

            {/* =================================================
                ENCABEZADO
            ================================================= */}

            <Box
                display="flex"
                justifyContent="space-between"
                alignItems="center"
                mb={3}
            >

                <Typography
                    variant="h4"
                    fontWeight="bold"
                >
                    Gestión de Usuarios
                </Typography>


                <Box
                    display="flex"
                    gap={2}
                >

                    {/* GESTIONAR ROLES */}

                    {puedeGestionarRoles && (

                        <Button
                            variant="outlined"
                            startIcon={
                                <SecurityIcon />
                            }
                            onClick={() =>
                                setRolesModalAbierto(
                                    true
                                )
                            }
                        >
                            Gestionar Roles
                        </Button>

                    )}


                    {/* NUEVO USUARIO */}

                    {puedeGestionarUsuarios && (

                        <Button
                            variant="contained"
                            startIcon={
                                <AddIcon />
                            }
                            onClick={() => {

                                setUsuarioSeleccionado(
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


            {/* =================================================
                BUSCADOR
            ================================================= */}

            <Box mb={3}>

                <TextField
                    fullWidth
                    placeholder="Buscar por usuario, nombre, correo o rol..."
                    value={
                        busqueda
                    }
                    onChange={(e) =>
                        setBusqueda(
                            e.target.value
                        )
                    }
                    InputProps={{
                        startAdornment: (

                            <InputAdornment
                                position="start"
                            >

                                <SearchIcon />

                            </InputAdornment>

                        )
                    }}
                />

            </Box>


            {/* =================================================
                TABLA
            ================================================= */}

            <UsuariosTable
                usuarios={usuariosFiltrados}
                onEditar={editarUsuario}
                onCambiarEstado={cambiarEstado}
                puedeGestionarUsuarios={puedeGestionarUsuarios}
            />


            {/* =================================================
                NUEVO / EDITAR USUARIO
            ================================================= */}

            {puedeGestionarUsuarios && (

                <NuevoUsuarioModal
                    open={
                        modalAbierto
                    }
                    onClose={() =>
                        setModalAbierto(
                            false
                        )
                    }
                    onGuardado={
                        cargarUsuarios
                    }
                    usuario={
                        usuarioSeleccionado
                    }
                />

            )}


            {/* =================================================
                GESTIONAR ROLES
            ================================================= */}

            {puedeGestionarRoles && (

                <GestionarRolesModal
                    open={
                        rolesModalAbierto
                    }
                    onClose={() =>
                        setRolesModalAbierto(
                            false
                        )
                    }
                />

            )}

        </Box>

    );
}
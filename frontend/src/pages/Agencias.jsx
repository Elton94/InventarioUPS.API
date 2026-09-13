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

import NuevaAgenciaModal
    from "../components/agencias/NuevaAgenciaModal";

import AgenciasTable
    from "../components/agencias/AgenciasTable";

import {
    obtenerAgencias,
    actualizarAgencia
} from "../services/agenciaService";

import {
    tienePermiso
} from "../services/permisos";


export default function Agencias() {

    // =====================================================
    // ESTADO
    // =====================================================

    const [
        agencias,
        setAgencias
    ] = useState([]);

    const [
        modalAbierto,
        setModalAbierto
    ] = useState(false);

    const [
        agenciaSeleccionada,
        setAgenciaSeleccionada
    ] = useState(null);

    const [
        busqueda,
        setBusqueda
    ] = useState("");


    // =====================================================
    // PERMISOS
    // =====================================================

    const puedeCrear =
        tienePermiso(
            "CREAR_AGENCIAS"
        );

    const puedeEditar =
        tienePermiso(
            "EDITAR_AGENCIAS"
        );


    // =====================================================
    // CARGAR AGENCIAS
    // =====================================================

    const cargarAgencias =
        async () => {

            try {

                const datos =
                    await obtenerAgencias();


                setAgencias(
                    Array.isArray(datos)
                        ? datos
                        : []
                );

            } catch (error) {

                console.error(
                    "Error al cargar agencias:",
                    error
                );

                setAgencias([]);

            }

        };


    // =====================================================
    // EDITAR
    // =====================================================

    const editarAgencia =
        (agencia) => {

            if (!puedeEditar) {
                return;
            }


            setAgenciaSeleccionada(
                agencia
            );

            setModalAbierto(
                true
            );

        };


    // =====================================================
    // DESACTIVAR
    // =====================================================

    const desactivarAgencia =
        async (agencia) => {

            if (!puedeEditar) {
                return;
            }


            const confirmar =
                window.confirm(
                    `¿Está seguro de que desea desactivar la agencia ${agencia.nombre}?`
                );


            if (!confirmar) {
                return;
            }


            try {

                await actualizarAgencia(
                    agencia.id,
                    {
                        ...agencia,
                        activa: false
                    }
                );


                await cargarAgencias();

            } catch (error) {

                console.error(
                    "Error al desactivar agencia:",
                    error
                );

            }

        };


    // =====================================================
    // CARGA INICIAL
    // =====================================================

    useEffect(() => {

        cargarAgencias();

    }, []);


    // =====================================================
    // NORMALIZAR TEXTO PARA BÚSQUEDA
    // =====================================================

    const normalizar =
        (valor) => {

            return String(
                valor ?? ""
            )
                .toLowerCase()
                .trim();

        };


    // =====================================================
    // FILTRAR AGENCIAS
    // =====================================================

    const textoBusqueda =
        normalizar(
            busqueda
        );


    const agenciasFiltradas =
        agencias.filter(
            agencia => {

                if (!textoBusqueda) {
                    return true;
                }


                const codigo =
                    normalizar(
                        agencia?.codigo
                    );

                const nombre =
                    normalizar(
                        agencia?.nombre
                    );

                const departamento =
                    normalizar(
                        agencia?.departamento
                    );

                const ciudad =
                    normalizar(
                        agencia?.ciudad
                    );

                const tipo =
                    normalizar(
                        agencia?.tipoAgencia ??
                        agencia?.tipo
                    );


                return (
                    codigo.includes(
                        textoBusqueda
                    ) ||

                    nombre.includes(
                        textoBusqueda
                    ) ||

                    departamento.includes(
                        textoBusqueda
                    ) ||

                    ciudad.includes(
                        textoBusqueda
                    ) ||

                    tipo.includes(
                        textoBusqueda
                    )
                );

            }
        );


    // =====================================================
    // INTERFAZ
    // =====================================================

    return (

        <Box>

            {/* ================================================= */}
            {/* ENCABEZADO */}
            {/* ================================================= */}

            <Box
                sx={{
                    display:
                        "flex",

                    justifyContent:
                        "space-between",

                    alignItems:
                        "center",

                    mb:
                        3
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
                        startIcon={
                            <AddIcon />
                        }
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


            {/* ================================================= */}
            {/* BUSCADOR */}
            {/* ================================================= */}

            <Box
                sx={{
                    mb:
                        3
                }}
            >

                <TextField
                    fullWidth
                    placeholder="Buscar agencia por código, nombre, departamento, ciudad o tipo..."
                    value={
                        busqueda
                    }
                    onChange={
                        (e) =>
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

            </Box>


            {/* ================================================= */}
            {/* TABLA */}
            {/* ================================================= */}

            <AgenciasTable
                agencias={
                    agenciasFiltradas
                }
                onEditar={
                    editarAgencia
                }
                onCambiarEstado={
                    desactivarAgencia
                }
                puedeEditar={
                    puedeEditar
                }
            />


            {/* ================================================= */}
            {/* MODAL */}
            {/* ================================================= */}

            <NuevaAgenciaModal
                open={
                    modalAbierto
                }
                onClose={() => {

                    setModalAbierto(
                        false
                    );

                    setAgenciaSeleccionada(
                        null
                    );

                }}
                onGuardado={
                    cargarAgencias
                }
                agencia={
                    agenciaSeleccionada
                }
            />

        </Box>

    );
}
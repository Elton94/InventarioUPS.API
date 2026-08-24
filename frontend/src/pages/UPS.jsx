import { useEffect, useState } from "react";

import NuevaUPSModal from "../components/ups/NuevoUPSModal";

import { Box, Button, TextField, Typography } from "@mui/material";

import AddIcon from "@mui/icons-material/Add";
import SearchIcon from "@mui/icons-material/Search";
import InputAdornment from "@mui/material/InputAdornment";

import UPSTable from "../components/ups/UPSTable";

import {
    obtenerUPS,
    cambiarEstadoUPS
} from "../services/upsService";

export default function UPS() {

    const [ups, setUps] = useState([]);

    const [busqueda, setBusqueda] = useState("");

    const [modalAbierto, setModalAbierto] = useState(false);

    const [upsSeleccionado, setUpsSeleccionado] = useState(null);


    // ==============================
    // CARGAR UPS
    // ==============================

    const cargarUPS = async () => {

        try {

            const datos = await obtenerUPS();

            setUps(datos);

        } catch (error) {

            console.error(
                "Error al cargar UPS:",
                error
            );

        }

    };


    // ==============================
    // CAMBIAR ESTADO
    // ==============================

    const manejarCambioEstado = async (
        id,
        estado
    ) => {

        try {

            await cambiarEstadoUPS(
                id,
                estado
            );

            await cargarUPS();

        } catch (error) {

            console.error(
                "Error al cambiar estado del UPS:",
                error
            );

        }

    };


    // ==============================
    // EDITAR UPS
    // ==============================

    const editarUPS = (equipo) => {

        setUpsSeleccionado(equipo);

        setModalAbierto(true);

    };


    // ==============================
    // NUEVO UPS
    // ==============================

    const nuevoUPS = () => {

        setUpsSeleccionado(null);

        setModalAbierto(true);

    };


    // ==============================
    // CARGAR AL ABRIR LA PÁGINA
    // ==============================

    useEffect(() => {

        cargarUPS();

    }, []);


    // ==============================
    // BUSCAR UPS
    // ==============================

    const upsFiltrados = ups.filter(
        (equipo) => {

            const texto =
                busqueda
                    .toLowerCase()
                    .trim();

            if (!texto) {

                return true;

            }

            return (

                equipo.codigo
                    ?.toLowerCase()
                    .includes(texto)

                ||

                equipo.marca
                    ?.toLowerCase()
                    .includes(texto)

                ||

                equipo.modelo
                    ?.toLowerCase()
                    .includes(texto)

                ||

                equipo.numeroSerie
                    ?.toLowerCase()
                    .includes(texto)

                ||

                equipo.agencia?.nombre
                    ?.toLowerCase()
                    .includes(texto)

            );

        }
    );


    // ==============================
    // INTERFAZ
    // ==============================

    return (

        <Box>

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
                    Gestión de UPS
                </Typography>


                <Button
                    variant="contained"
                    startIcon={<AddIcon />}
                    onClick={nuevoUPS}
                >
                    Nuevo UPS
                </Button>

            </Box>


            {/* BUSCADOR */}

            <Box mb={3}>

                <TextField
                    fullWidth
                    placeholder="Buscar UPS por código, marca, modelo, serie o agencia..."
                    value={busqueda}
                    onChange={(e) =>
                        setBusqueda(e.target.value)
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


            {/* TABLA */}

            <UPSTable
                ups={upsFiltrados}
                onEditar={editarUPS}
                onCambiarEstado={
                    manejarCambioEstado
                }
            />


            {/* MODAL */}

            <NuevaUPSModal
                open={modalAbierto}
                onClose={() =>
                    setModalAbierto(false)
                }
                onGuardado={cargarUPS}
                ups={upsSeleccionado}
            />

        </Box>

    );

}
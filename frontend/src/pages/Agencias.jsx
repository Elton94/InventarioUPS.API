import { useEffect, useState } from "react";
import {
    Box,
    Button,
    TextField,
    Typography,
    InputAdornment
} from "@mui/material";

import AddIcon from "@mui/icons-material/Add";
import SearchIcon from "@mui/icons-material/Search";

import NuevaAgenciaModal from "../components/agencias/NuevaAgenciaModal";
import AgenciasTable from "../components/agencias/AgenciasTable";

import {
    obtenerAgencias,
    actualizarAgencia
} from "../services/agenciaService";

export default function Agencias() {

    const [agencias, setAgencias] = useState([]);

    const [modalAbierto, setModalAbierto] = useState(false);

    const [agenciaSeleccionada, setAgenciaSeleccionada] = useState(null);

    const [busqueda, setBusqueda] = useState("");

    const cargarAgencias = async () => {

        try {

            const datos = await obtenerAgencias();

            setAgencias(datos);

        } catch (error) {

            console.error(
                "Error al cargar agencias:",
                error
            );

        }

    };

    const editarAgencia = (agencia) => {

        setAgenciaSeleccionada(agencia);

        setModalAbierto(true);

    };

    const desactivarAgencia = async (agencia) => {

        const confirmar = window.confirm(
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

    useEffect(() => {

        cargarAgencias();

    }, []);

    const agenciasFiltradas = agencias.filter((agencia) => {

        const texto = busqueda.toLowerCase().trim();

        if (!texto) {

            return true;

        }

        return (

            agencia.codigo?.toLowerCase().includes(texto) ||

            agencia.nombre?.toLowerCase().includes(texto) ||

            agencia.departamento?.toLowerCase().includes(texto) ||

            agencia.ciudad?.toLowerCase().includes(texto)

        );

    });

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
                    Gestión de Agencias
                </Typography>

                <Button
                    variant="contained"
                    startIcon={<AddIcon />}
                    onClick={() => {

                        setAgenciaSeleccionada(null);

                        setModalAbierto(true);

                    }}
                >
                    Nueva Agencia
                </Button>

            </Box>

            <Box mb={3}>

                <TextField
                    fullWidth
                    placeholder="Buscar agencia por código, nombre, departamento o ciudad..."
                    value={busqueda}
                    onChange={(e) =>
                        setBusqueda(e.target.value)
                    }
                    InputProps={{
                        startAdornment: (
                            <InputAdornment position="start">
                                <SearchIcon />
                            </InputAdornment>
                        )
                    }}
                />

            </Box>

            <AgenciasTable
                agencias={agenciasFiltradas}
                onEditar={editarAgencia}
                onDesactivar={desactivarAgencia}
            />

            <NuevaAgenciaModal
                open={modalAbierto}
                onClose={() => setModalAbierto(false)}
                onGuardado={cargarAgencias}
                agencia={agenciaSeleccionada}
            />

        </Box>

    );

}
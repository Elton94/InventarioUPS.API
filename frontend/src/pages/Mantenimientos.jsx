import { useEffect, useState } from "react";
import NuevoMantenimientoModal from "../components/mantenimientos/NuevoMantenimientoModal";

import {
    Autocomplete,
    Box,
    Button,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    TextField,
    Typography
} from "@mui/material";

import {
    obtenerMantenimientos
} from "../services/mantenimientoService";

import MantenimientosTable
    from "../components/mantenimientos/MantenimientosTable";

import { tienePermiso } from "../services/permisos";


export default function Mantenimientos() {

    const [mantenimientos, setMantenimientos] = useState([]);

    const [tipoBusqueda, setTipoBusqueda] = useState("agencia");

    const [textoBusqueda, setTextoBusqueda] = useState("");

    const [fechaDesde, setFechaDesde] = useState("");

    const [fechaHasta, setFechaHasta] = useState("");

    const [modalAbierto, setModalAbierto] = useState(false);

    const [mantenimientoEditar, setMantenimientoEditar] = useState(null);


    // =====================================
    // PERMISOS
    // =====================================

    const puedeCrear = tienePermiso("CREAR_MANTENIMIENTO");

    const puedeEditar = tienePermiso("EDITAR_MANTENIMIENTO");


    // =====================================
    // CARGAR MANTENIMIENTOS
    // =====================================

    const cargarMantenimientos = async () => {

        try {

            const datos = await obtenerMantenimientos();

            setMantenimientos(datos);

        } catch (error) {

            console.error(
                "Error al cargar mantenimientos:",
                error
            );

        }

    };


    useEffect(() => {

        cargarMantenimientos();

    }, []);


    // =====================================
    // LIMPIAR BÚSQUEDA
    // =====================================

    const limpiarBusqueda = () => {

        setTextoBusqueda("");
        setFechaDesde("");
        setFechaHasta("");

    };


    // =====================================
    // EDITAR MANTENIMIENTO
    // =====================================

    const editarMantenimiento = (mantenimiento) => {

        if (!puedeEditar) {
            return;
        }

        setMantenimientoEditar(mantenimiento);

        setModalAbierto(true);

    };


    // =====================================
    // NUEVO MANTENIMIENTO
    // =====================================

    const nuevoMantenimiento = () => {

        if (!puedeCrear) {
            return;
        }

        setMantenimientoEditar(null);

        setModalAbierto(true);

    };


    // =====================================
    // CAMBIAR TIPO DE BÚSQUEDA
    // =====================================

    const cambiarTipoBusqueda = (event) => {

        setTipoBusqueda(event.target.value);

        limpiarBusqueda();

    };


    // =====================================
    // OPCIONES DEL AUTOCOMPLETE
    // =====================================

    const obtenerOpciones = () => {

        let valores = [];

        switch (tipoBusqueda) {

            case "agencia":

                valores = mantenimientos.map(
                    m => m.agenciaNombre
                );

                break;


            case "marca":

                valores = [];

                break;


            case "modelo":

                valores = mantenimientos.map(
                    m => m.modeloUPS
                );

                break;


            case "tipo":

                valores = mantenimientos.map(
                    m => m.tipoMantenimiento
                );

                break;


            case "tecnico":

                valores = mantenimientos.map(
                    m => m.tecnicoProveedor
                );

                break;


            case "estado":

                valores = mantenimientos.map(
                    m => m.estado
                );

                break;


            default:

                valores = [];

        }


        return [
            ...new Set(
                valores.filter(Boolean)
            )
        ].sort();

    };


    // =====================================
    // FILTRAR MANTENIMIENTOS
    // =====================================

    const mantenimientosFiltrados =
        mantenimientos.filter(
            (mantenimiento) => {

                // -------------------------
                // FECHA
                // -------------------------

                if (tipoBusqueda === "fecha") {

                    const fecha =
                        mantenimiento
                            .fechaMantenimiento
                            ?.substring(0, 10);

                    if (
                        fechaDesde &&
                        fecha < fechaDesde
                    ) {
                        return false;
                    }

                    if (
                        fechaHasta &&
                        fecha > fechaHasta
                    ) {
                        return false;
                    }

                    return true;
                }


                // -------------------------
                // SIN BÚSQUEDA
                // -------------------------

                const texto =
                    textoBusqueda
                        .toLowerCase()
                        .trim();

                if (!texto) {

                    return true;

                }


                // -------------------------
                // FILTROS
                // -------------------------

                switch (tipoBusqueda) {

                    case "agencia":

                        return (
                            mantenimiento
                                .agenciaNombre
                                ?.toLowerCase()
                                .includes(texto)
                        );


                    case "codigoUPS":

                        return (
                            mantenimiento
                                .codigoUPS
                                ?.toLowerCase()
                                .includes(texto)
                        );


                    case "numeroSerie":

                        return (
                            mantenimiento
                                .numeroSerieUPS
                                ?.toLowerCase()
                                .includes(texto)
                        );


                    case "modelo":

                        return (
                            mantenimiento
                                .modeloUPS
                                ?.toLowerCase()
                                .includes(texto)
                        );


                    case "tipo":

                        return (
                            mantenimiento
                                .tipoMantenimiento
                                ?.toLowerCase()
                                .includes(texto)
                        );


                    case "tecnico":

                        return (
                            mantenimiento
                                .tecnicoProveedor
                                ?.toLowerCase()
                                .includes(texto)
                        );


                    case "estado":

                        return (
                            mantenimiento
                                .estado
                                ?.toLowerCase()
                                .includes(texto)
                        );


                    default:

                        return true;

                }

            }
        );


    // =====================================
    // INTERFAZ
    // =====================================

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
                    Gestión de Mantenimientos
                </Typography>


                {puedeCrear && (

                    <Button
                        variant="contained"
                        onClick={nuevoMantenimiento}
                    >
                        + Nuevo mantenimiento
                    </Button>

                )}

            </Box>


            {/* ========================= */}
            {/* FILTROS */}
            {/* ========================= */}

            <Box
                display="flex"
                gap={2}
                alignItems="center"
                mb={3}
            >

                <FormControl
                    sx={{
                        minWidth: 230
                    }}
                >

                    <InputLabel>
                        Buscar por
                    </InputLabel>

                    <Select
                        value={tipoBusqueda}
                        label="Buscar por"
                        onChange={cambiarTipoBusqueda}
                    >

                        <MenuItem value="agencia">
                            Agencia
                        </MenuItem>

                        <MenuItem value="codigoUPS">
                            Código UPS
                        </MenuItem>

                        <MenuItem value="numeroSerie">
                            Número de serie
                        </MenuItem>

                        <MenuItem value="marca">
                            Marca
                        </MenuItem>

                        <MenuItem value="modelo">
                            Modelo
                        </MenuItem>

                        <MenuItem value="tipo">
                            Tipo de mantenimiento
                        </MenuItem>

                        <MenuItem value="tecnico">
                            Técnico / proveedor
                        </MenuItem>

                        <MenuItem value="estado">
                            Estado
                        </MenuItem>

                        <MenuItem value="fecha">
                            Fecha de mantenimiento
                        </MenuItem>

                    </Select>

                </FormControl>


                {/* ========================= */}
                {/* FECHAS */}
                {/* ========================= */}

                {tipoBusqueda === "fecha" ? (

                    <>

                        <TextField
                            type="date"
                            label="Desde"
                            value={fechaDesde}
                            onChange={(e) =>
                                setFechaDesde(
                                    e.target.value
                                )
                            }
                            InputLabelProps={{
                                shrink: true
                            }}
                        />

                        <TextField
                            type="date"
                            label="Hasta"
                            value={fechaHasta}
                            onChange={(e) =>
                                setFechaHasta(
                                    e.target.value
                                )
                            }
                            InputLabelProps={{
                                shrink: true
                            }}
                        />

                    </>

                ) : (

                    [
                        "agencia",
                        "marca",
                        "modelo",
                        "tipo",
                        "tecnico",
                        "estado"
                    ].includes(tipoBusqueda)

                ) ? (

                    <Autocomplete
                        fullWidth
                        freeSolo
                        options={obtenerOpciones()}
                        value={textoBusqueda || null}
                        onChange={(
                            event,
                            nuevoValor
                        ) => {

                            setTextoBusqueda(
                                nuevoValor || ""
                            );

                        }}
                        onInputChange={(
                            event,
                            nuevoValor
                        ) => {

                            setTextoBusqueda(
                                nuevoValor
                            );

                        }}
                        renderInput={(params) => (

                            <TextField
                                {...params}
                                label={`Buscar ${tipoBusqueda}`}
                            />

                        )}
                    />

                ) : (

                    <TextField
                        fullWidth
                        label={`Buscar ${tipoBusqueda}`}
                        value={textoBusqueda}
                        onChange={(e) =>
                            setTextoBusqueda(
                                e.target.value
                            )
                        }
                    />

                )}


                <Button
                    variant="outlined"
                    onClick={limpiarBusqueda}
                >
                    Limpiar
                </Button>

            </Box>


            {/* ========================= */}
            {/* RESULTADOS */}
            {/* ========================= */}

            <Typography
                variant="body1"
                mb={2}
            >

                Resultados encontrados:{" "}

                <strong>
                    {
                        mantenimientosFiltrados.length
                    }
                </strong>

            </Typography>


            {/* ========================= */}
            {/* TABLA */}
            {/* ========================= */}

            <MantenimientosTable
                mantenimientos={
                    mantenimientosFiltrados
                }
                onEditar={
                    editarMantenimiento
                }
                puedeEditar={puedeEditar}
            />


            {/* ========================= */}
            {/* MODAL */}
            {/* ========================= */}

            <NuevoMantenimientoModal
                open={modalAbierto}
                onClose={() => {

                    setModalAbierto(false);

                    setMantenimientoEditar(null);

                }}
                onGuardado={async () => {

                    await cargarMantenimientos();

                    setMantenimientoEditar(null);

                }}
                mantenimientoEditar={
                    mantenimientoEditar
                }
            />

        </Box>

    );

}
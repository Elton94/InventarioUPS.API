import {
    useEffect,
    useMemo,
    useState
} from "react";

import {
    Alert,
    Autocomplete,
    Box,
    Button,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    Snackbar,
    TextField,
    Typography
} from "@mui/material";

import {
    obtenerMantenimientos
} from "../services/mantenimientoService";
import {
    tienePermiso
} from "../services/permisos";

import MantenimientosTable
    from "../components/mantenimientos/MantenimientosTable";
import NuevoMantenimientoModal
    from "../components/mantenimientos/NuevoMantenimientoModal";

const normalizar = (valor) =>
    String(valor ?? "")
        .toLowerCase()
        .trim();

export default function Mantenimientos() {
    const [mantenimientos, setMantenimientos] =
        useState([]);
    const [tipoBusqueda, setTipoBusqueda] =
        useState("agencia");
    const [textoBusqueda, setTextoBusqueda] =
        useState("");
    const [fechaDesde, setFechaDesde] =
        useState("");
    const [fechaHasta, setFechaHasta] =
        useState("");
    const [modalAbierto, setModalAbierto] =
        useState(false);
    const [seleccionado, setSeleccionado] =
        useState(null);
    const [mensaje, setMensaje] =
        useState("");
    const [error, setError] =
        useState("");

    const puedeCrear =
        tienePermiso(
            "CREAR_MANTENIMIENTO"
        );

    const puedeEditar =
        tienePermiso(
            "EDITAR_MANTENIMIENTO"
        );

    const cargar = async () => {
        try {
            const datos =
                await obtenerMantenimientos();

            setMantenimientos(
                Array.isArray(datos)
                    ? datos
                    : []
            );
        } catch (e) {
            setError(
                e.response?.data?.mensaje ||
                "No fue posible cargar los mantenimientos."
            );
        }
    };

    useEffect(() => {
        cargar();
    }, []);

    const campoPorTipo = {
        agencia: "agenciaNombre",
        codigoUPS: "codigoUPS",
        numeroSerie: "numeroSerieUPS",
        marca: "marcaUPS",
        modelo: "modeloUPS",
        tipo: "tipoMantenimiento",
        tecnico: "tecnicoProveedor",
        estado: "estado"
    };

    const opciones =
        useMemo(() => {
            const campo =
                campoPorTipo[
                    tipoBusqueda
                ];

            if (!campo) return [];

            return [
                ...new Set(
                    mantenimientos
                        .map((m) =>
                            m[campo]
                        )
                        .filter(Boolean)
                )
            ].sort();
        }, [
            mantenimientos,
            tipoBusqueda
        ]);

    const filtrados =
        mantenimientos.filter((m) => {
            if (
                tipoBusqueda ===
                "fecha"
            ) {
                const valor =
                    m.fechaMantenimiento
                        ?.substring(0, 10);

                if (
                    fechaDesde &&
                    valor < fechaDesde
                ) {
                    return false;
                }

                if (
                    fechaHasta &&
                    valor > fechaHasta
                ) {
                    return false;
                }

                return true;
            }

            const campo =
                campoPorTipo[
                    tipoBusqueda
                ];

            if (!campo) return true;

            const texto =
                normalizar(
                    textoBusqueda
                );

            if (!texto) return true;

            return normalizar(
                m[campo]
            ).includes(texto);
        });

    const limpiar = () => {
        setTextoBusqueda("");
        setFechaDesde("");
        setFechaHasta("");
    };

    return (
        <Box>
            <Box
                sx={{
                    display: "flex",
                    justifyContent:
                        "space-between",
                    alignItems: "center",
                    mb: 3
                }}
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
                        onClick={() => {
                            setSeleccionado(
                                null
                            );
                            setModalAbierto(
                                true
                            );
                        }}
                    >
                        + Nuevo mantenimiento
                    </Button>
                )}
            </Box>

            <Box
                sx={{
                    display: "flex",
                    gap: 2,
                    alignItems: "center",
                    flexWrap: "wrap",
                    mb: 3
                }}
            >
                <FormControl
                    sx={{ minWidth: 230 }}
                >
                    <InputLabel>
                        Buscar por
                    </InputLabel>
                    <Select
                        value={tipoBusqueda}
                        label="Buscar por"
                        onChange={(e) => {
                            setTipoBusqueda(
                                e.target.value
                            );
                            limpiar();
                        }}
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
                            Fecha
                        </MenuItem>
                    </Select>
                </FormControl>

                {tipoBusqueda ===
                "fecha" ? (
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
                            slotProps={{
                                inputLabel: {
                                    shrink: true
                                }
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
                            slotProps={{
                                inputLabel: {
                                    shrink: true
                                }
                            }}
                        />
                    </>
                ) : [
                    "agencia",
                    "marca",
                    "modelo",
                    "tipo",
                    "tecnico",
                    "estado"
                ].includes(tipoBusqueda) ? (
                    <Autocomplete
                        freeSolo
                        sx={{
                            minWidth: 320,
                            flex: 1
                        }}
                        options={opciones}
                        inputValue={
                            textoBusqueda
                        }
                        onInputChange={(
                            _,
                            valor
                        ) =>
                            setTextoBusqueda(
                                valor
                            )
                        }
                        renderInput={(
                            params
                        ) => (
                            <TextField
                                {...params}
                                label="Buscar"
                            />
                        )}
                    />
                ) : (
                    <TextField
                        sx={{
                            minWidth: 320,
                            flex: 1
                        }}
                        label="Buscar"
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
                    onClick={limpiar}
                >
                    Limpiar
                </Button>
            </Box>

            <Typography
                sx={{ mb: 2 }}
            >
                Resultados encontrados:{" "}
                <strong>
                    {filtrados.length}
                </strong>
            </Typography>

            <MantenimientosTable
                mantenimientos={filtrados}
                puedeEditar={puedeEditar}
                onEditar={(m) => {
                    setSeleccionado(m);
                    setModalAbierto(true);
                }}
            />

            <NuevoMantenimientoModal
                open={modalAbierto}
                mantenimientoEditar={
                    seleccionado
                }
                onClose={() => {
                    setModalAbierto(false);
                    setSeleccionado(null);
                }}
                onGuardado={async () => {
                    await cargar();

                    setMensaje(
                        seleccionado
                            ? "Mantenimiento actualizado correctamente."
                            : "Mantenimiento registrado correctamente."
                    );

                    setSeleccionado(null);
                }}
            />

            <Snackbar
                open={Boolean(mensaje)}
                autoHideDuration={4000}
                onClose={() =>
                    setMensaje("")
                }
            >
                <Alert
                    severity="success"
                    onClose={() =>
                        setMensaje("")
                    }
                >
                    {mensaje}
                </Alert>
            </Snackbar>

            <Snackbar
                open={Boolean(error)}
                autoHideDuration={6000}
                onClose={() =>
                    setError("")
                }
            >
                <Alert
                    severity="error"
                    onClose={() =>
                        setError("")
                    }
                >
                    {error}
                </Alert>
            </Snackbar>
        </Box>
    );
}

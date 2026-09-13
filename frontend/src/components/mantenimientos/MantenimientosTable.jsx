import {
    Box,
    Chip,
    IconButton,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Tooltip,
    Typography
} from "@mui/material";

import EditIcon from "@mui/icons-material/Edit";
import BatteryChargingFullIcon from "@mui/icons-material/BatteryChargingFull";


export default function MantenimientosTable({
    mantenimientos = [],
    onEditar,
    puedeEditar = false
}) {

    // =====================================================
    // ASEGURAR LISTA VÁLIDA
    // =====================================================

    const listaMantenimientos =
        Array.isArray(mantenimientos)
            ? mantenimientos
            : [];


    // =====================================================
    // COLOR DEL ESTADO
    // =====================================================

    const obtenerColorEstado = (
        estado
    ) => {

        const nombre =
            estado
                ?.trim()
                .toLowerCase();

        if (
            nombre === "completado" ||
            nombre === "realizado"
        ) {
            return "success";
        }

        if (
            nombre === "en proceso"
        ) {
            return "warning";
        }

        if (
            nombre === "programado"
        ) {
            return "info";
        }

        if (
            nombre === "cancelado"
        ) {
            return "error";
        }

        return "default";
    };


    // =====================================================
    // FECHA
    // =====================================================

    const formatearFecha = (
        fecha
    ) => {

        if (!fecha) {
            return "-";
        }

        const fechaObjeto =
            new Date(fecha);

        if (
            Number.isNaN(
                fechaObjeto.getTime()
            )
        ) {
            return "-";
        }

        return fechaObjeto
            .toLocaleDateString(
                "es-HN"
            );
    };


    // =====================================================
    // MONEDA
    // =====================================================

    const formatearMoneda = (
        valor
    ) => {

        const numero =
            Number(valor || 0);

        return numero
            .toLocaleString(
                "es-HN",
                {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }
            );
    };


    // =====================================================
    // COSTO TOTAL
    // =====================================================

    const obtenerCostoTotal = (
        mantenimiento
    ) => {

        const costoServicio =
            Number(
                mantenimiento?.costo ||
                0
            );

        const costoBaterias =
            mantenimiento
                ?.incluyeCambioBaterias
                ? Number(
                    mantenimiento
                        ?.costoBaterias ||
                    0
                )
                : 0;

        return (
            costoServicio +
            costoBaterias
        );
    };


    // =====================================================
    // VALIDAR PROGRAMADO
    // =====================================================

    const esProgramado = (
        mantenimiento
    ) => {

        return (
            mantenimiento
                ?.estado
                ?.trim()
                .toLowerCase() ===
            "programado"
        );
    };


    return (

        <TableContainer
            component={Paper}
        >

            <Table>

                <TableHead>

                    <TableRow>

                        <TableCell>
                            <strong>
                                Agencia
                            </strong>
                        </TableCell>

                        <TableCell>
                            <strong>
                                UPS
                            </strong>
                        </TableCell>

                        <TableCell>
                            <strong>
                                Número de serie
                            </strong>
                        </TableCell>

                        <TableCell>
                            <strong>
                                Tipo
                            </strong>
                        </TableCell>

                        <TableCell>
                            <strong>
                                Fecha
                            </strong>
                        </TableCell>

                        <TableCell>
                            <strong>
                                Técnico / proveedor
                            </strong>
                        </TableCell>

                        <TableCell>
                            <strong>
                                Costos
                            </strong>
                        </TableCell>

                        <TableCell>
                            <strong>
                                Baterías
                            </strong>
                        </TableCell>

                        <TableCell>
                            <strong>
                                Estado
                            </strong>
                        </TableCell>

                        <TableCell
                            align="center"
                        >
                            <strong>
                                Acciones
                            </strong>
                        </TableCell>

                    </TableRow>

                </TableHead>


                <TableBody>

                    {listaMantenimientos.map(
                        mantenimiento => {

                            const costoServicio =
                                Number(
                                    mantenimiento
                                        ?.costo ||
                                    0
                                );


                            const costoBaterias =
                                mantenimiento
                                    ?.incluyeCambioBaterias
                                    ? Number(
                                        mantenimiento
                                            ?.costoBaterias ||
                                        0
                                    )
                                    : 0;


                            const costoTotal =
                                obtenerCostoTotal(
                                    mantenimiento
                                );


                            return (

                                <TableRow
                                    key={
                                        mantenimiento
                                            ?.id
                                    }
                                    hover
                                >

                                    {/* AGENCIA */}

                                    <TableCell>

                                        {
                                            mantenimiento
                                                ?.agenciaNombre ||
                                            "-"
                                        }

                                    </TableCell>


                                    {/* UPS */}

                                    <TableCell>

                                        {
                                            mantenimiento
                                                ?.codigoUPS ||
                                            "-"
                                        }

                                    </TableCell>


                                    {/* SERIE */}

                                    <TableCell>

                                        {
                                            mantenimiento
                                                ?.numeroSerieUPS ||
                                            "-"
                                        }

                                    </TableCell>


                                    {/* TIPO */}

                                    <TableCell>

                                        {
                                            mantenimiento
                                                ?.tipoMantenimiento ||
                                            "-"
                                        }

                                    </TableCell>


                                    {/* FECHA */}

                                    <TableCell>

                                        {
                                            formatearFecha(
                                                mantenimiento
                                                    ?.fechaMantenimiento
                                            )
                                        }

                                    </TableCell>


                                    {/* TÉCNICO */}

                                    <TableCell>

                                        {
                                            mantenimiento
                                                ?.tecnicoProveedor ||
                                            "-"
                                        }

                                    </TableCell>


                                    {/* COSTOS */}

                                    <TableCell>

                                        <Box
                                            sx={{
                                                display:
                                                    "flex",

                                                flexDirection:
                                                    "column",

                                                gap:
                                                    0.35
                                            }}
                                        >

                                            <Typography
                                                variant="body2"
                                            >
                                                Servicio:{" "}

                                                <strong>
                                                    L{" "}
                                                    {
                                                        formatearMoneda(
                                                            costoServicio
                                                        )
                                                    }
                                                </strong>
                                            </Typography>


                                            {mantenimiento
                                                ?.incluyeCambioBaterias && (

                                                <Typography
                                                    variant="body2"
                                                >
                                                    Baterías:{" "}

                                                    <strong>
                                                        L{" "}
                                                        {
                                                            formatearMoneda(
                                                                costoBaterias
                                                            )
                                                        }
                                                    </strong>
                                                </Typography>

                                            )}


                                            <Typography
                                                variant="body2"
                                                sx={{
                                                    mt:
                                                        0.3,

                                                    fontWeight:
                                                        700
                                                }}
                                            >
                                                Total: L{" "}
                                                {
                                                    formatearMoneda(
                                                        costoTotal
                                                    )
                                                }
                                            </Typography>

                                        </Box>

                                    </TableCell>


                                    {/* BATERÍAS */}

                                    <TableCell>

                                        {mantenimiento
                                            ?.incluyeCambioBaterias ? (

                                            <Box
                                                sx={{
                                                    display:
                                                        "flex",

                                                    flexDirection:
                                                        "column",

                                                    gap:
                                                        0.6,

                                                    alignItems:
                                                        "flex-start"
                                                }}
                                            >

                                                <Chip
                                                    icon={
                                                        <BatteryChargingFullIcon />
                                                    }
                                                    label={
                                                        `${mantenimiento?.cantidadBateriasReemplazadas || 0} baterías`
                                                    }
                                                    size="small"
                                                    color={
                                                        esProgramado(
                                                            mantenimiento
                                                        )
                                                            ? "info"
                                                            : "success"
                                                    }
                                                    variant="outlined"
                                                />


                                                <Typography
                                                    variant="caption"
                                                    color="text.secondary"
                                                >

                                                    {
                                                        esProgramado(
                                                            mantenimiento
                                                        )
                                                            ? "Cambio programado"
                                                            : "Cambio realizado"
                                                    }

                                                </Typography>

                                            </Box>

                                        ) : (

                                            <Typography
                                                variant="body2"
                                                color="text.secondary"
                                            >
                                                No
                                            </Typography>

                                        )}

                                    </TableCell>


                                    {/* ESTADO */}

                                    <TableCell>

                                        <Chip
                                            label={
                                                mantenimiento
                                                    ?.estado ||
                                                "-"
                                            }
                                            size="small"
                                            color={
                                                obtenerColorEstado(
                                                    mantenimiento
                                                        ?.estado
                                                )
                                            }
                                        />

                                    </TableCell>


                                    {/* ACCIONES */}

                                    <TableCell
                                        align="center"
                                    >

                                        {puedeEditar && (

                                            <Tooltip
                                                title="Editar mantenimiento"
                                            >

                                                <IconButton
                                                    color="primary"
                                                    onClick={() =>
                                                        onEditar?.(
                                                            mantenimiento
                                                        )
                                                    }
                                                >

                                                    <EditIcon />

                                                </IconButton>

                                            </Tooltip>

                                        )}

                                    </TableCell>

                                </TableRow>

                            );

                        }
                    )}


                    {listaMantenimientos.length === 0 && (

                        <TableRow>

                            <TableCell
                                colSpan={10}
                                align="center"
                            >
                                No se encontraron mantenimientos.
                            </TableCell>

                        </TableRow>

                    )}

                </TableBody>

            </Table>

        </TableContainer>

    );
}
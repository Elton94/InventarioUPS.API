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

import EditIcon
    from "@mui/icons-material/Edit";
import BatteryChargingFullIcon
    from "@mui/icons-material/BatteryChargingFull";

const fecha = (valor) =>
    valor
        ? new Date(
            valor
        ).toLocaleDateString("es-HN")
        : "—";

const colorEstado = (estado) => {
    switch (
        String(estado || "")
            .toLowerCase()
    ) {
        case "completado":
            return "success";
        case "en proceso":
            return "warning";
        case "programado":
            return "info";
        case "cancelado":
            return "error";
        default:
            return "default";
    }
};

const textoBateria = (
    mantenimiento
) => {
    const estado =
        String(
            mantenimiento.estado || ""
        ).toLowerCase();

    if (estado === "programado") {
        return "Cambio programado";
    }

    if (estado === "completado") {
        return "Cambio realizado";
    }

    if (estado === "cancelado") {
        return "Cambio cancelado";
    }

    return "Cambio en proceso";
};

export default function MantenimientosTable({
    mantenimientos = [],
    onEditar,
    puedeEditar = false
}) {
    return (
        <TableContainer component={Paper}>
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell><strong>Agencia</strong></TableCell>
                        <TableCell><strong>Inventario</strong></TableCell>
                        <TableCell><strong>Serie</strong></TableCell>
                        <TableCell><strong>Fecha</strong></TableCell>
                        <TableCell><strong>Técnico / proveedor</strong></TableCell>
                        <TableCell><strong>Baterías</strong></TableCell>
                        <TableCell><strong>Estado</strong></TableCell>
                        <TableCell align="center"><strong>Acciones</strong></TableCell>
                    </TableRow>
                </TableHead>

                <TableBody>
                    {mantenimientos.map(
                        (m) => {
                            return (
                                <TableRow
                                    key={m.id}
                                    hover
                                >
                                    <TableCell>
                                        {
                                            m.agenciaNombre ||
                                            "—"
                                        }
                                    </TableCell>
                                    <TableCell>
                                        {
                                            m.numeroInventarioUPS ||
                                            "—"
                                        }
                                    </TableCell>
                                    <TableCell>
                                        {
                                            m.numeroSerieUPS ||
                                            "—"
                                        }
                                    </TableCell>
                                    <TableCell>
                                        {
                                            fecha(
                                                m.fechaMantenimiento
                                            )
                                        }
                                    </TableCell>
                                    <TableCell>
                                        {
                                            m.tecnicoProveedor ||
                                            "—"
                                        }
                                    </TableCell>
                                    <TableCell>
                                        {(
                                            <Box
                                                sx={{
                                                    display: "flex",
                                                    flexDirection: "column",
                                                    gap: 0.5,
                                                    alignItems: "flex-start"
                                                }}
                                            >
                                                <Chip
                                                    icon={<BatteryChargingFullIcon />}
                                                    label={`${m.cantidadBateriasReemplazadas || 0} baterías`}
                                                    size="small"
                                                    variant="outlined"
                                                />
                                                <Typography
                                                    variant="caption"
                                                    color="text.secondary"
                                                >
                                                    {m.tipoBateria || "Sin identificar"} · {m.modeloBateria || "Sin identificar"}<br />{textoBateria(m)}
                                                </Typography>
                                            </Box>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        <Chip
                                            label={
                                                m.estado ||
                                                "—"
                                            }
                                            size="small"
                                            color={
                                                colorEstado(
                                                    m.estado
                                                )
                                            }
                                        />
                                    </TableCell>
                                    <TableCell align="center">
                                        {puedeEditar && (
                                            <Tooltip title="Editar cambio de baterías">
                                                <IconButton
                                                    color="primary"
                                                    onClick={() =>
                                                        onEditar(
                                                            m
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

                    {mantenimientos.length === 0 && (
                        <TableRow>
                            <TableCell
                                colSpan={8}
                                align="center"
                            >
                                No se encontraron cambios de baterías.
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
        </TableContainer>
    );
}

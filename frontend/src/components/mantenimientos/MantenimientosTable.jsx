import {
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Chip,
    IconButton,
    Tooltip
} from "@mui/material";

import EditIcon from "@mui/icons-material/Edit";

export default function MantenimientosTable({
    mantenimientos,
    onEditar
}) {

    return (

        <TableContainer component={Paper}>

            <Table>

                <TableHead>

                    <TableRow>

                        <TableCell>
                            <strong>Agencia</strong>
                        </TableCell>

                        <TableCell>
                            <strong>UPS</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Número de serie</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Tipo</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Fecha</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Técnico / proveedor</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Costo</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Estado</strong>
                        </TableCell>

                        <TableCell align="center">
                            <strong>Acciones</strong>
                        </TableCell>

                    </TableRow>

                </TableHead>

                <TableBody>

                    {mantenimientos.map(
                        (mantenimiento) => (

                            <TableRow
                                key={
                                    mantenimiento.id
                                }
                            >

                                <TableCell>
                                    {
                                        mantenimiento
                                            .agenciaNombre
                                    }
                                </TableCell>

                                <TableCell>
                                    {
                                        mantenimiento
                                            .codigoUPS
                                    }
                                </TableCell>

                                <TableCell>
                                    {
                                        mantenimiento
                                            .numeroSerieUPS
                                    }
                                </TableCell>

                                <TableCell>
                                    {
                                        mantenimiento
                                            .tipoMantenimiento
                                    }
                                </TableCell>

                                <TableCell>
                                    {
                                        new Date(
                                            mantenimiento
                                                .fechaMantenimiento
                                        ).toLocaleDateString()
                                    }
                                </TableCell>

                                <TableCell>
                                    {
                                        mantenimiento
                                            .tecnicoProveedor
                                    }
                                </TableCell>

                                <TableCell>
                                    L{" "}
                                    {
                                        Number(
                                            mantenimiento.costo
                                        ).toLocaleString(
                                            "es-HN",
                                            {
                                                minimumFractionDigits: 2
                                            }
                                        )
                                    }
                                </TableCell>

                                <TableCell>

                                    <Chip
                                        label={
                                            mantenimiento
                                                .estado
                                        }
                                        size="small"
                                        color={
                                            mantenimiento
                                                .estado ===
                                            "Completado"
                                                ? "success"
                                                : mantenimiento
                                                    .estado ===
                                                  "En proceso"
                                                    ? "warning"
                                                    : "default"
                                        }
                                    />

                                </TableCell>

                                <TableCell align="center">

                                    <Tooltip title="Editar mantenimiento">

                                        <IconButton
                                            onClick={() =>
                                                onEditar(
                                                    mantenimiento
                                                )
                                            }
                                        >

                                            <EditIcon />

                                        </IconButton>

                                    </Tooltip>

                                </TableCell>

                            </TableRow>

                        )
                    )}

                    {mantenimientos.length === 0 && (

                        <TableRow>

                            <TableCell
                                colSpan={9}
                                align="center"
                            >
                                No se encontraron
                                mantenimientos.
                            </TableCell>

                        </TableRow>

                    )}

                </TableBody>

            </Table>

        </TableContainer>

    );
}
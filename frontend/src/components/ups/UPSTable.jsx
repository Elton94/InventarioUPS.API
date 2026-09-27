import {
    Chip,
    IconButton,
    MenuItem,
    Paper,
    Select,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Tooltip
} from "@mui/material";

import EditIcon
    from "@mui/icons-material/Edit";

export default function UPSTable({
    ups = [],
    estados = [],
    onEditar,
    onCambiarEstado,
    puedeEditar
}) {
    return (
        <TableContainer component={Paper}>
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell><strong>Inventario</strong></TableCell>
                        <TableCell><strong>Marca</strong></TableCell>
                        <TableCell><strong>Modelo</strong></TableCell>
                        <TableCell><strong>Número de serie</strong></TableCell>
                        <TableCell><strong>Capacidad VA</strong></TableCell>
                        <TableCell><strong>Baterías: tipo / modelo</strong></TableCell>
                        <TableCell><strong>Agencia</strong></TableCell>
                        <TableCell><strong>Estado</strong></TableCell>
                        <TableCell align="center"><strong>Acciones</strong></TableCell>
                    </TableRow>
                </TableHead>

                <TableBody>
                    {ups.map((equipo) => {
                        const esBaja =
                            equipo.estado
                                ?.toLowerCase() ===
                            "de baja";

                        return (
                            <TableRow
                                key={equipo.id}
                                hover
                            >
                                <TableCell>{equipo.numeroInventario || "—"}</TableCell>
                                <TableCell>{equipo.marca}</TableCell>
                                <TableCell>{equipo.modelo}</TableCell>
                                <TableCell>{equipo.numeroSerie}</TableCell>
                                <TableCell>{equipo.capacidadVA} VA</TableCell>
                                <TableCell>{equipo.tipoBateria || "Sin identificar"}<br />{equipo.modeloBateria || "Sin identificar"} · {equipo.cantidadBaterias} unidades</TableCell>
                                <TableCell>{equipo.agenciaNombre || "Sin agencia"}</TableCell>
                                <TableCell>
                                    {puedeEditar ? (
                                        <Select
                                            size="small"
                                            value={equipo.estado || ""}
                                            disabled={esBaja}
                                            onChange={(e) =>
                                                onCambiarEstado(
                                                    equipo,
                                                    e.target.value
                                                )
                                            }
                                        >
                                            {estados.map(
                                                (estado) => (
                                                    <MenuItem
                                                        key={estado.id}
                                                        value={estado.nombre}
                                                    >
                                                        {estado.nombre}
                                                    </MenuItem>
                                                )
                                            )}
                                        </Select>
                                    ) : (
                                        <Chip
                                            label={equipo.estado || "Sin estado"}
                                            size="small"
                                        />
                                    )}
                                </TableCell>
                                <TableCell align="center">
                                    {puedeEditar && (
                                        <Tooltip title="Editar UPS">
                                            <IconButton
                                                onClick={() =>
                                                    onEditar(equipo)
                                                }
                                            >
                                                <EditIcon />
                                            </IconButton>
                                        </Tooltip>
                                    )}
                                </TableCell>
                            </TableRow>
                        );
                    })}

                    {ups.length === 0 && (
                        <TableRow>
                            <TableCell
                                colSpan={9}
                                align="center"
                            >
                                No hay UPS registrados.
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
        </TableContainer>
    );
}

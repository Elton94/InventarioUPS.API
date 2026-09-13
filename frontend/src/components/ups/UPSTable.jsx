import {
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    IconButton,
    Tooltip,
    Select,
    MenuItem
} from "@mui/material";

import EditIcon from "@mui/icons-material/Edit";


export default function UPSTable({
    ups,
    onEditar,
    onCambiarEstado,
    puedeEditar
}) {

    const obtenerColorEstado = (estado) => {

        if (estado === "Operativo") {
            return "success";
        }

        if (estado === "En mantenimiento") {
            return "warning";
        }

        if (estado === "Fuera de servicio") {
            return "error";
        }

        return "primary";
    };


    return (

        <TableContainer component={Paper}>

            <Table>

                <TableHead>

                    <TableRow>

                        <TableCell>
                            <strong>Código</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Marca</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Modelo</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Número de serie</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Capacidad VA</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Agencia</strong>
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

                    {ups.map((equipo) => (

                        <TableRow key={equipo.id}>

                            <TableCell>
                                {equipo.codigo}
                            </TableCell>

                            <TableCell>
                                {equipo.marca}
                            </TableCell>

                            <TableCell>
                                {equipo.modelo}
                            </TableCell>

                            <TableCell>
                                {equipo.numeroSerie}
                            </TableCell>

                            <TableCell>
                                {equipo.capacidadVA} VA
                            </TableCell>

                            <TableCell>
                                {equipo.agencia?.nombre || "Sin agencia"}
                            </TableCell>


                            <TableCell>

                                {puedeEditar ? (

                                    <Select
                                        size="small"
                                        value={equipo.estado}
                                        onChange={(e) =>
                                            onCambiarEstado(
                                                equipo.id,
                                                e.target.value
                                            )
                                        }
                                        color={obtenerColorEstado(
                                            equipo.estado
                                        )}
                                    >

                                        <MenuItem value="Operativo">
                                            Operativo
                                        </MenuItem>

                                        <MenuItem value="En mantenimiento">
                                            En mantenimiento
                                        </MenuItem>

                                        <MenuItem value="Fuera de servicio">
                                            Fuera de servicio
                                        </MenuItem>

                                    </Select>

                                ) : (

                                    equipo.estado

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

                    ))}

                </TableBody>

            </Table>

        </TableContainer>

    );

}
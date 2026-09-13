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
import PowerSettingsNewIcon from "@mui/icons-material/PowerSettingsNew";


export default function AgenciasTable({
    agencias,
    onEditar,
    onCambiarEstado,
    puedeEditar
}) {

    if (!agencias || agencias.length === 0) {

        return (
            <Paper
                elevation={2}
                sx={{
                    p: 4,
                    textAlign: "center"
                }}
            >

                <Typography
                    variant="body1"
                    color="text.secondary"
                >
                    No hay agencias registradas.
                </Typography>

            </Paper>
        );

    }


    return (

        <TableContainer
            component={Paper}
            elevation={2}
        >

            <Table>

                <TableHead>

                    <TableRow>

                        <TableCell>
                            <strong>Código</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Nombre</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Ciudad</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Departamento</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Tipo</strong>
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

                    {agencias.map((agencia) => (

                        <TableRow
                            key={agencia.id}
                            hover
                        >

                            <TableCell>
                                {agencia.codigo || "-"}
                            </TableCell>


                            <TableCell>
                                {agencia.nombre || "-"}
                            </TableCell>


                            <TableCell>
                                {agencia.ciudad?.nombre ||
                                    agencia.ciudadNombre ||
                                    "-"}
                            </TableCell>


                            <TableCell>
                                {agencia.ciudad?.departamento?.nombre ||
                                    agencia.departamento?.nombre ||
                                    agencia.departamentoNombre ||
                                    "-"}
                            </TableCell>


                            <TableCell>
                                {agencia.tipoAgencia?.nombre ||
                                    agencia.tipoAgenciaNombre ||
                                    "-"}
                            </TableCell>


                            <TableCell>

                                <Chip
                                    label={
                                        agencia.activa
                                            ? "Activa"
                                            : "Inactiva"
                                    }
                                    color={
                                        agencia.activa
                                            ? "success"
                                            : "default"
                                    }
                                    size="small"
                                />

                            </TableCell>


                            <TableCell align="center">

                                {puedeEditar && (

                                    <>

                                        <Tooltip title="Editar agencia">

                                            <IconButton
                                                color="primary"
                                                onClick={() =>
                                                    onEditar(agencia)
                                                }
                                            >

                                                <EditIcon />

                                            </IconButton>

                                        </Tooltip>


                                        <Tooltip
                                            title={
                                                agencia.activa
                                                    ? "Desactivar agencia"
                                                    : "Activar agencia"
                                            }
                                        >

                                            <IconButton
                                                color={
                                                    agencia.activa
                                                        ? "error"
                                                        : "success"
                                                }
                                                onClick={() =>
                                                    onCambiarEstado(
                                                        agencia.id,
                                                        !agencia.activa
                                                    )
                                                }
                                            >

                                                <PowerSettingsNewIcon />

                                            </IconButton>

                                        </Tooltip>

                                    </>

                                )}

                            </TableCell>

                        </TableRow>

                    ))}

                </TableBody>

            </Table>

        </TableContainer>

    );

}
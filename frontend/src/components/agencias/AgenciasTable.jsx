import {
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    IconButton,
    Tooltip
} from "@mui/material";

import EditIcon from "@mui/icons-material/Edit";
import BlockIcon from "@mui/icons-material/Block";

export default function AgenciasTable({
    agencias,
    onEditar,
    onDesactivar
}) {

    return (

        <TableContainer component={Paper}>

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
                            <strong>Departamento</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Ciudad</strong>
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

                        <TableRow key={agencia.id}>

                            <TableCell>
                                {agencia.codigo}
                            </TableCell>

                            <TableCell>
                                {agencia.nombre}
                            </TableCell>

                            <TableCell>
                                {agencia.departamento}
                            </TableCell>

                            <TableCell>
                                {agencia.ciudad}
                            </TableCell>

                            <TableCell>
                                {agencia.activa
                                    ? "Activa"
                                    : "Inactiva"
                                }
                            </TableCell>

                            <TableCell align="center">

                                <Tooltip title="Editar agencia">

                                    <IconButton
                                        onClick={() => onEditar(agencia)}
                                    >

                                        <EditIcon />

                                    </IconButton>

                                </Tooltip>

                                {agencia.activa && (

                                    <Tooltip title="Desactivar agencia">

                                        <IconButton
                                            onClick={() =>
                                                onDesactivar(agencia)
                                            }
                                        >

                                            <BlockIcon />

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
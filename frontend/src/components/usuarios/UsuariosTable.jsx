import {
    IconButton,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Tooltip,
    Chip
} from "@mui/material";

import EditIcon from "@mui/icons-material/Edit";
import BlockIcon from "@mui/icons-material/Block";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";

export default function UsuariosTable({
    usuarios,
    onEditar,
    onCambiarEstado,
    puedeGestionarUsuarios = false
}) {
    return (
        <TableContainer component={Paper}>
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell>
                            <strong>Usuario</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Nombre completo</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Correo</strong>
                        </TableCell>

                        <TableCell>
                            <strong>Rol</strong>
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
                    {usuarios.map((usuario) => (
                        <TableRow key={usuario.id}>
                            <TableCell>
                                {usuario.nombreUsuario}
                            </TableCell>

                            <TableCell>
                                {usuario.nombreCompleto}
                            </TableCell>

                            <TableCell>
                                {usuario.correo}
                            </TableCell>

                            <TableCell>
                                <Chip
                                    label={usuario.rolNombre}
                                    size="small"
                                />
                            </TableCell>

                            <TableCell>
                                <Chip
                                    label={
                                        usuario.activo
                                            ? "Activo"
                                            : "Inactivo"
                                    }
                                    color={
                                        usuario.activo
                                            ? "success"
                                            : "default"
                                    }
                                    size="small"
                                />
                            </TableCell>

                            <TableCell align="center">

                                {puedeGestionarUsuarios && (
                                    <>
                                        <Tooltip title="Editar usuario">
                                            <IconButton
                                                color="primary"
                                                onClick={() =>
                                                    onEditar(usuario)
                                                }
                                            >
                                                <EditIcon />
                                            </IconButton>
                                        </Tooltip>

                                        <Tooltip
                                            title={
                                                usuario.activo
                                                    ? "Desactivar usuario"
                                                    : "Activar usuario"
                                            }
                                        >
                                            <IconButton
                                                color={
                                                    usuario.activo
                                                        ? "error"
                                                        : "success"
                                                }
                                                onClick={() =>
                                                    onCambiarEstado(
                                                        usuario
                                                    )
                                                }
                                            >
                                                {usuario.activo ? (
                                                    <BlockIcon />
                                                ) : (
                                                    <CheckCircleIcon />
                                                )}
                                            </IconButton>
                                        </Tooltip>
                                    </>
                                )}

                            </TableCell>
                        </TableRow>
                    ))}

                    {usuarios.length === 0 && (
                        <TableRow>
                            <TableCell
                                colSpan={6}
                                align="center"
                            >
                                No hay usuarios registrados.
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
        </TableContainer>
    );
}
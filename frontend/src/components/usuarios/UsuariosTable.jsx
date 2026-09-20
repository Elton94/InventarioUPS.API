import {
    Chip,
    IconButton,
    Paper,
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
import BlockIcon
    from "@mui/icons-material/Block";
import CheckCircleIcon
    from "@mui/icons-material/CheckCircle";

export default function UsuariosTable({
    usuarios = [],
    onEditar,
    onCambiarEstado,
    puedeGestionarUsuarios = false,
    puedeGestionarRoles = false,
    usuarioActualId = 0
}) {
    return (
        <TableContainer component={Paper}>
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell><strong>Usuario</strong></TableCell>
                        <TableCell><strong>Nombre completo</strong></TableCell>
                        <TableCell><strong>Correo</strong></TableCell>
                        <TableCell><strong>Rol</strong></TableCell>
                        <TableCell><strong>Estado</strong></TableCell>
                        <TableCell align="center"><strong>Acciones</strong></TableCell>
                    </TableRow>
                </TableHead>

                <TableBody>
                    {usuarios.map((usuario) => {
                        const esActual =
                            Number(usuario.id) ===
                            Number(
                                usuarioActualId
                            );

                        const esControlTotal =
                            String(
                                usuario.rolNombre ||
                                ""
                            )
                                .trim()
                                .toLowerCase() ===
                            "control total";

                        const protegido =
                            esControlTotal &&
                            !puedeGestionarRoles;

                        return (
                            <TableRow
                                key={usuario.id}
                                hover
                            >
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
                                                <span>
                                                    <IconButton
                                                        color="primary"
                                                        disabled={
                                                            protegido
                                                        }
                                                        onClick={() =>
                                                            onEditar(
                                                                usuario
                                                            )
                                                        }
                                                    >
                                                        <EditIcon />
                                                    </IconButton>
                                                </span>
                                            </Tooltip>

                                            <Tooltip
                                                title={
                                                    protegido
                                                        ? "Solo quien gestiona roles puede modificar un usuario Control Total"
                                                        : esActual &&
                                                          usuario.activo
                                                            ? "No puede desactivar su propio usuario"
                                                            : usuario.activo
                                                            ? "Desactivar usuario"
                                                            : "Activar usuario"
                                                }
                                            >
                                                <span>
                                                    <IconButton
                                                        disabled={
                                                            protegido ||
                                                            (
                                                                esActual &&
                                                                usuario.activo
                                                            )
                                                        }
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
                                                </span>
                                            </Tooltip>
                                        </>
                                    )}
                                </TableCell>
                            </TableRow>
                        );
                    })}

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

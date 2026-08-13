import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow
} from "@mui/material";

export default function AgenciasTable() {

    return (

        <TableContainer component={Paper}>

            <Table>

                <TableHead>

                    <TableRow>

                        <TableCell><strong>Código</strong></TableCell>

                        <TableCell><strong>Nombre</strong></TableCell>

                        <TableCell><strong>Departamento</strong></TableCell>

                        <TableCell><strong>Ciudad</strong></TableCell>

                        <TableCell><strong>Estado</strong></TableCell>

                    </TableRow>

                </TableHead>

                <TableBody>

                    <TableRow>

                        <TableCell>SPS01</TableCell>

                        <TableCell>Agencia San Pedro Sula</TableCell>

                        <TableCell>Cortés</TableCell>

                        <TableCell>San Pedro Sula</TableCell>

                        <TableCell>Activa</TableCell>

                    </TableRow>

                </TableBody>

            </Table>

        </TableContainer>

    );

}
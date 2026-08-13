import { Box, Button, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import AgenciasTable from "../components/agencias/AgenciasTable";

export default function Agencias() {

    return (

        <Box>

            <Box
                display="flex"
                justifyContent="space-between"
                alignItems="center"
                mb={3}
            >

                <Typography variant="h4" fontWeight="bold">

                    Gestión de Agencias

                </Typography>

                <Button
                    variant="contained"
                    startIcon={<AddIcon />}
                >

                    Nueva Agencia

                </Button>

            </Box>

            <AgenciasTable />

        </Box>

    );

}
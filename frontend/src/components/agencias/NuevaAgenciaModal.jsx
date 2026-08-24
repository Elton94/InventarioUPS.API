import { useEffect, useState } from "react";

import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Grid,
    FormControlLabel,
    Checkbox
} from "@mui/material";

import {
    crearAgencia,
    actualizarAgencia
} from "../../services/agenciaService";

export default function NuevaAgenciaModal({
    open,
    onClose,
    onGuardado,
    agencia
}) {

    const [formulario, setFormulario] = useState({

        codigo: "",
        nombre: "",
        departamento: "",
        ciudad: "",
        direccion: "",
        tipoAgencia: "",
        activa: true

    });

    useEffect(() => {

        if (agencia) {

            setFormulario({

                codigo: agencia.codigo || "",
                nombre: agencia.nombre || "",
                departamento: agencia.departamento || "",
                ciudad: agencia.ciudad || "",
                direccion: agencia.direccion || "",
                tipoAgencia: agencia.tipoAgencia || "",
                activa: agencia.activa ?? true

            });

        } else {

            setFormulario({

                codigo: "",
                nombre: "",
                departamento: "",
                ciudad: "",
                direccion: "",
                tipoAgencia: "",
                activa: true

            });

        }

    }, [agencia]);

    const cambiarValor = (e) => {

        const { name, value } = e.target;

        setFormulario({

            ...formulario,
            [name]: value

        });

    };

    const guardar = async () => {

    try {

        if (agencia) {

            await actualizarAgencia(
                agencia.id,
                formulario
            );

        } else {

            await crearAgencia(formulario);

        }

        await onGuardado();

        onClose();

    } catch (error) {

        console.error("Error al guardar agencia:", error);

    }

};

    return (

        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="md"
            fullWidth
        >

            <DialogTitle>

                {agencia
                    ? "Editar Agencia"
                    : "Nueva Agencia"
                }

            </DialogTitle>

            <DialogContent>

                <Grid
                    container
                    spacing={2}
                    sx={{ mt: 1 }}
                >

                    <Grid item xs={6}>

                        <TextField
                            fullWidth
                            label="Código"
                            name="codigo"
                            value={formulario.codigo}
                            onChange={cambiarValor}
                        />

                    </Grid>

                    <Grid item xs={6}>

                        <TextField
                            fullWidth
                            label="Nombre"
                            name="nombre"
                            value={formulario.nombre}
                            onChange={cambiarValor}
                        />

                    </Grid>

                    <Grid item xs={6}>

                        <TextField
                            fullWidth
                            label="Departamento"
                            name="departamento"
                            value={formulario.departamento}
                            onChange={cambiarValor}
                        />

                    </Grid>

                    <Grid item xs={6}>

                        <TextField
                            fullWidth
                            label="Ciudad"
                            name="ciudad"
                            value={formulario.ciudad}
                            onChange={cambiarValor}
                        />

                    </Grid>

                    <Grid item xs={12}>

                        <TextField
                            fullWidth
                            label="Dirección"
                            name="direccion"
                            value={formulario.direccion}
                            onChange={cambiarValor}
                        />

                    </Grid>

                    <Grid item xs={6}>

                        <TextField
                            fullWidth
                            label="Tipo de Agencia"
                            name="tipoAgencia"
                            value={formulario.tipoAgencia}
                            onChange={cambiarValor}
                        />

                    </Grid>

                    <Grid item xs={6}>

                        <FormControlLabel
                            control={
                                <Checkbox
                                    checked={formulario.activa}
                                    onChange={(e) =>
                                        setFormulario({
                                            ...formulario,
                                            activa: e.target.checked
                                        })
                                    }
                                />
                            }
                            label="Activa"
                        />

                    </Grid>

                </Grid>

            </DialogContent>

            <DialogActions>

                <Button onClick={onClose}>

                    Cancelar

                </Button>

                <Button
                    variant="contained"
                    onClick={guardar}
                >

                    {agencia
                        ? "Guardar cambios"
                        : "Guardar"
                    }

                </Button>

            </DialogActions>

        </Dialog>

    );

}
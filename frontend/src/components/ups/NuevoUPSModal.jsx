import { useEffect, useState } from "react";

import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Button,
    MenuItem,
    Autocomplete
} from "@mui/material";

import { obtenerAgencias } from "../../services/agenciaService";

import {
    crearUPS,
    actualizarUPS
} from "../../services/upsService";

export default function NuevoUPSModal({
    open,
    onClose,
    onGuardado,
    ups
}) {

    const [agencias, setAgencias] = useState([]);

    const [formulario, setFormulario] = useState({
        codigo: "",
        marca: "",
        modelo: "",
        numeroSerie: "",
        capacidadVA: "",
        agenciaId: "",
        fechaInstalacion: "",
        estado: "Operativo",
        cantidadBaterias: "",
        fechaUltimoCambioBaterias: ""
    });

    const [guardando, setGuardando] = useState(false);

    useEffect(() => {

        if (!open) {
            return;
        }

        cargarAgencias();

        if (ups) {

            setFormulario({
                codigo: ups.codigo || "",
                marca: ups.marca || "",
                modelo: ups.modelo || "",
                numeroSerie: ups.numeroSerie || "",
                capacidadVA: ups.capacidadVA || "",
                agenciaId: ups.agenciaId || "",
                fechaInstalacion: ups.fechaInstalacion
                    ? ups.fechaInstalacion.substring(0, 10)
                    : "",
                estado: ups.estado || "Operativo",
                cantidadBaterias: ups.cantidadBaterias ?? "",
                fechaUltimoCambioBaterias:
                    ups.fechaUltimoCambioBaterias
                        ? ups.fechaUltimoCambioBaterias.substring(0, 10)
                        : ""
            });

        } else {

            setFormulario({
                codigo: "",
                marca: "",
                modelo: "",
                numeroSerie: "",
                capacidadVA: "",
                agenciaId: "",
                fechaInstalacion: "",
                estado: "Operativo",
                cantidadBaterias: "",
                fechaUltimoCambioBaterias: ""
            });

        }

    }, [open, ups]);

    const cargarAgencias = async () => {

        try {

            const datos = await obtenerAgencias();

            setAgencias(
                datos.filter(
                    (agencia) => agencia.activa
                )
            );

        } catch (error) {

            console.error(
                "Error al cargar agencias:",
                error
            );

        }

    };

    const manejarCambio = (e) => {

        const { name, value } = e.target;

        setFormulario({
            ...formulario,
            [name]: value
        });

    };

    const guardar = async () => {

        try {

            setGuardando(true);

            const datosUPS = {

                codigo: formulario.codigo.trim(),

                marca: formulario.marca.trim(),

                modelo: formulario.modelo.trim(),

                numeroSerie: formulario.numeroSerie.trim(),

                capacidadVA: Number(
                    formulario.capacidadVA
                ),

                agenciaId: Number(
                    formulario.agenciaId
                ),

                fechaInstalacion:
                    formulario.fechaInstalacion,

                estado: formulario.estado,

                cantidadBaterias:
                    formulario.cantidadBaterias === ""
                        ? 0
                        : Number(
                            formulario.cantidadBaterias
                        ),

                fechaUltimoCambioBaterias:
                    formulario.fechaUltimoCambioBaterias
                        ? formulario.fechaUltimoCambioBaterias
                        : null
            };

            if (ups) {

                await actualizarUPS(
                    ups.id,
                    {
                        id: ups.id,
                        ...datosUPS
                    }
                );

            } else {

                await crearUPS(datosUPS);

            }

            onClose();

            await onGuardado();

        } catch (error) {

            console.error(
                "Error al guardar UPS:",
                error
            );

        } finally {

            setGuardando(false);

        }

    };

    return (

        <Dialog
            open={open}
            onClose={onClose}
            fullWidth
            maxWidth="sm"
        >

            <DialogTitle>

                {ups
                    ? "Editar UPS"
                    : "Nuevo UPS"
                }

            </DialogTitle>

            <DialogContent>

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Código"
                    name="codigo"
                    value={formulario.codigo}
                    onChange={manejarCambio}
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Número de serie"
                    name="numeroSerie"
                    value={formulario.numeroSerie}
                    onChange={manejarCambio}
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Marca"
                    name="marca"
                    value={formulario.marca}
                    onChange={manejarCambio}
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Modelo"
                    name="modelo"
                    value={formulario.modelo}
                    onChange={manejarCambio}
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Capacidad (VA)"
                    name="capacidadVA"
                    type="number"
                    value={formulario.capacidadVA}
                    onChange={manejarCambio}
                    inputProps={{
                        min: 1
                    }}
                />

                <Autocomplete
                    fullWidth
                    options={agencias}
                    getOptionLabel={(agencia) =>
                        `${agencia.codigo} - ${agencia.nombre} - ${agencia.ciudad}`
                    }
                    value={
                        agencias.find(
                            (agencia) =>
                                agencia.id ===
                                Number(formulario.agenciaId)
                        ) || null
                    }
                    onChange={(event, nuevaAgencia) => {

                        setFormulario({
                            ...formulario,
                            agenciaId: nuevaAgencia
                                ? nuevaAgencia.id
                                : ""
                        });

                    }}
                    isOptionEqualToValue={
                        (option, value) =>
                            option.id === value.id
                    }
                    renderInput={(params) => (

                        <TextField
                            {...params}
                            required
                            margin="normal"
                            label="Agencia"
                            placeholder="Buscar agencia..."
                        />

                    )}
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Fecha de instalación"
                    name="fechaInstalacion"
                    type="date"
                    value={formulario.fechaInstalacion}
                    onChange={manejarCambio}
                    InputLabelProps={{
                        shrink: true
                    }}
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    label="Cantidad de baterías"
                    name="cantidadBaterias"
                    type="number"
                    value={formulario.cantidadBaterias}
                    onChange={manejarCambio}
                    inputProps={{
                        min: 0
                    }}
                />

                <TextField
                    fullWidth
                    margin="normal"
                    label="Último cambio de baterías"
                    name="fechaUltimoCambioBaterias"
                    type="date"
                    value={
                        formulario.fechaUltimoCambioBaterias
                    }
                    onChange={manejarCambio}
                    InputLabelProps={{
                        shrink: true
                    }}
                />

                <TextField
                    fullWidth
                    required
                    margin="normal"
                    select
                    label="Estado"
                    name="estado"
                    value={formulario.estado}
                    onChange={manejarCambio}
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

                </TextField>

            </DialogContent>

            <DialogActions>

                <Button
                    onClick={onClose}
                    disabled={guardando}
                >
                    Cancelar
                </Button>

                <Button
                    variant="contained"
                    onClick={guardar}
                    disabled={guardando}
                >

                    {guardando
                        ? "Guardando..."
                        : ups
                            ? "Actualizar"
                            : "Guardar"
                    }

                </Button>

            </DialogActions>

        </Dialog>

    );

}
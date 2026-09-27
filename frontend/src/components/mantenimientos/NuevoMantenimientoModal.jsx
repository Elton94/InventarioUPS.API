import { useEffect, useState } from "react";
import {
    Alert, Autocomplete, Box, Button, Dialog, DialogActions,
    DialogContent, DialogTitle, MenuItem, TextField
} from "@mui/material";
import { obtenerUPS } from "../../services/upsService";
import { crearMantenimiento, actualizarMantenimiento } from "../../services/mantenimientoService";
import api from "../../services/api";
import { obtenerEstadosMantenimiento } from "../../services/catalogosService";

import SelectorBateria from "../baterias/SelectorBateria";

const inicial = {
    modeloBateriaId: null,
    fechaMantenimiento: "",
    descripcion: "",
    responsableServicioId: null,
    estadoMantenimientoId: "",
    observaciones: ""
};

const fechaActual = () => {
    const hoy = new Date();
    return `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, "0")}-${String(hoy.getDate()).padStart(2, "0")}`;
};

export default function NuevoMantenimientoModal({ open, onClose, onGuardado, mantenimientoEditar = null }) {
    const [equipos, setEquipos] = useState([]);
    const [upsSeleccionado, setUpsSeleccionado] = useState(null);
    const [estados, setEstados] = useState([]);
    const [responsables, setResponsables] = useState([]);
    const [formulario, setFormulario] = useState(inicial);
    const [cargando, setCargando] = useState(false);
    const [guardando, setGuardando] = useState(false);
    const [error, setError] = useState("");

    useEffect(() => {
        if (!open) return;
        let vigente = true;
        const cargar = async () => {
            setCargando(true);
            setError("");
            try {
                const [ups, estadosData, responsablesData] = await Promise.all([
                    obtenerUPS(), obtenerEstadosMantenimiento(),
                    api.get("/catalogos/responsables-servicio", { params: { actualId: mantenimientoEditar?.responsableServicioId } }).then(r => r.data)
                ]);
                if (!vigente) return;
                setEquipos((ups || []).filter(u => String(u.estado || "").toLowerCase() !== "de baja"));
                setEstados(estadosData || []);
                setResponsables(responsablesData || []);
                if (mantenimientoEditar) {
                    setUpsSeleccionado((ups || []).find(u => Number(u.id) === Number(mantenimientoEditar.upsId)) || null);
                    setFormulario({
                        modeloBateriaId: mantenimientoEditar.modeloBateriaId || null,
                        fechaMantenimiento: mantenimientoEditar.fechaMantenimiento?.substring(0, 10) || "",
                        descripcion: mantenimientoEditar.descripcion || "",
                        responsableServicioId: mantenimientoEditar.responsableServicioId || null,
                        estadoMantenimientoId: mantenimientoEditar.estadoMantenimientoId || "",
                        observaciones: mantenimientoEditar.observaciones || ""
                    });
                } else {
                    setUpsSeleccionado(null);
                    setFormulario(inicial);
                }
            } catch (e) {
                if (vigente) setError(e.response?.data?.mensaje || "No fue posible cargar los datos necesarios.");
            } finally {
                if (vigente) setCargando(false);
            }
        };
        cargar();
        return () => { vigente = false; };
    }, [open, mantenimientoEditar]);

    const cambiar = (campo, valor) => setFormulario(actual => ({ ...actual, [campo]: valor }));
    const estadoNombre = estados.find(e => Number(e.id) === Number(formulario.estadoMantenimientoId))?.nombre || "";
    const esProgramado = estadoNombre.toLowerCase() === "programado";
    const conservaCantidad = mantenimientoEditar?.estado?.toLowerCase() === "completado" && estadoNombre.toLowerCase() === "completado";
    const cantidadBaterias = Number(conservaCantidad
        ? mantenimientoEditar.cantidadBateriasReemplazadas
        : upsSeleccionado?.cantidadBaterias || 0);

    const guardar = async () => {
        setError("");
        if (!formulario.modeloBateriaId) return setError("Seleccione el tipo y modelo de las baterías del cambio.");
        if (!upsSeleccionado) return setError("Debe seleccionar un UPS.");
        if (!formulario.estadoMantenimientoId) return setError("Seleccione el estado del cambio.");
        if (!formulario.fechaMantenimiento) return setError("Seleccione la fecha del cambio.");
        if (formulario.fechaMantenimiento > fechaActual() && !esProgramado)
            return setError("Una fecha futura solamente puede utilizarse con estado Programado.");
        if (!formulario.responsableServicioId) return setError("Seleccione el responsable del servicio.");
        if (cantidadBaterias <= 0) return setError("El UPS no tiene una cantidad válida de baterías.");

        const datos = {
            ...formulario,
            upsId: Number(upsSeleccionado.id),
            estadoMantenimientoId: Number(formulario.estadoMantenimientoId),
            fechaMantenimiento: `${formulario.fechaMantenimiento}T00:00:00`
        };
        try {
            setGuardando(true);
            if (mantenimientoEditar) await actualizarMantenimiento(mantenimientoEditar.id, datos);
            else await crearMantenimiento(datos);
            await onGuardado();
            onClose();
        } catch (e) {
            setError(e.response?.data?.mensaje || "No fue posible guardar el cambio de baterías.");
        } finally {
            setGuardando(false);
        }
    };

    return (
        <Dialog open={open} onClose={guardando ? undefined : onClose} fullWidth maxWidth="md">
            <DialogTitle>{mantenimientoEditar ? "Editar cambio de baterías" : "Nuevo cambio de baterías"}</DialogTitle>
            <DialogContent>
                {error && <Alert severity="error" sx={{ mt: 1, mb: 2 }}>{error}</Alert>}
                <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", sm: "repeat(2, minmax(0, 1fr))" }, gap: 2, mt: 1 }}>
                    <Autocomplete
                        sx={{ gridColumn: "1 / -1" }}
                        options={equipos}
                        value={upsSeleccionado}
                        disabled={cargando || guardando || Boolean(mantenimientoEditar)}
                        getOptionLabel={u => [u.numeroInventario, u.numeroSerie, u.marca, u.modelo, u.agenciaCodigo, u.agenciaNombre, u.estado].filter(Boolean).join(" - ")}
                        isOptionEqualToValue={(a, b) => Number(a.id) === Number(b.id)}
                        onChange={(_, valor) => { setUpsSeleccionado(valor); cambiar("modeloBateriaId", valor?.modeloBateriaId || null); }}
                        renderInput={params => <TextField {...params} required label="UPS" placeholder="Inventario, serie o agencia" />}
                    />
                    <TextField select required label="Estado" value={formulario.estadoMantenimientoId}
                        onChange={e => cambiar("estadoMantenimientoId", e.target.value)}>
                        {estados.map(estado => <MenuItem key={estado.id} value={estado.id}>{estado.nombre}</MenuItem>)}
                    </TextField>
                    <TextField required type="date" label="Fecha del cambio" value={formulario.fechaMantenimiento}
                        onChange={e => cambiar("fechaMantenimiento", e.target.value)}
                        slotProps={{ inputLabel: { shrink: true }, htmlInput: esProgramado ? {} : { max: fechaActual() } }} />
                    <Autocomplete options={responsables}
                        value={responsables.find(r => r.id === Number(formulario.responsableServicioId)) || null}
                        disabled={cargando || guardando}
                        getOptionLabel={r => `${r.nombre}${r.activo === false ? ' (Inactivo)' : ''}`}
                        isOptionEqualToValue={(a, b) => a.id === b.id}
                        onChange={(_, valor) => cambiar("responsableServicioId", valor?.id || null)}
                        renderInput={params => <TextField {...params} required label="Responsable del servicio"
                            helperText="Seleccione un técnico o proveedor. El administrador agrega opciones en Configuración." />} />
                    <TextField label="Cantidad de baterías" value={cantidadBaterias || ""} disabled
                        helperText={conservaCantidad ? "Cantidad registrada al completar el cambio." : "Se reemplaza el banco completo."} />
                    <SelectorBateria key={upsSeleccionado?.id || "nuevo"} value={formulario.modeloBateriaId} onChange={id => cambiar("modeloBateriaId", id)} disabled={cargando || guardando} />
                    <TextField sx={{ gridColumn: "1 / -1" }} multiline rows={3} label="Detalle del trabajo (opcional)" slotProps={{ htmlInput: { maxLength: 1000 } }}
                        value={formulario.descripcion} onChange={e => cambiar("descripcion", e.target.value)} />
                    <TextField sx={{ gridColumn: "1 / -1" }} multiline rows={2} label="Observaciones" slotProps={{ htmlInput: { maxLength: 1000 } }}
                        value={formulario.observaciones} onChange={e => cambiar("observaciones", e.target.value)} />
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} disabled={guardando}>Cancelar</Button>
                <Button variant="contained" onClick={guardar} disabled={cargando || guardando}>
                    {guardando ? "Guardando..." : "Guardar"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}

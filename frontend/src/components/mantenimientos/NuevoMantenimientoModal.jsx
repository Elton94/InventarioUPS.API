import { useEffect, useState } from "react";
import {
    Alert, Autocomplete, Box, Button, Dialog, DialogActions,
    DialogContent, DialogTitle, MenuItem, TextField
} from "@mui/material";
import { obtenerUPS } from "../../services/upsService";
import { crearMantenimiento, actualizarMantenimiento } from "../../services/mantenimientoService";
import api from "../../services/api";
import { tienePermiso } from "../../services/permisos";
import NuevoUsuarioModal from "../usuarios/NuevoUsuarioModal";
import { obtenerEstadosMantenimiento } from "../../services/catalogosService";

import SelectorBateria from "../baterias/SelectorBateria";
import AccesoCatalogo from "../catalogos/AccesoCatalogo";

const inicial = {
    tipoBateriaId: null,
    voltajeBateria: "",
    capacidadBateriaAh: "",
    fechaMantenimiento: "",
    responsableUsuarioId: null,
    estadoMantenimientoId: "",
    observaciones: ""
};

const fechaActual = () => {
    const hoy = new Date();
    return `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, "0")}-${String(hoy.getDate()).padStart(2, "0")}`;
};

export default function NuevoMantenimientoModal({ open, onClose, onGuardado, mantenimientoEditar = null }) {
    const puedeElegirResponsable = ['administrador', 'control total'].includes((localStorage.getItem('rol') || '').trim().toLowerCase());
    const usuarioActualId = Number(localStorage.getItem('usuarioId'));
    const [nuevoUsuario, setNuevoUsuario] = useState(false);
    const cargarResponsables = async () => (await api.get('/Usuarios/responsables-mantenimiento', { params: { actualId: mantenimientoEditar?.responsableUsuarioId } })).data;
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
                    cargarResponsables()
                ]);
                if (!vigente) return;
                setEquipos((ups || []).filter(u => String(u.estado || "").toLowerCase() !== "de baja"));
                setEstados(estadosData || []);
                setResponsables(responsablesData || []);
                if (mantenimientoEditar) {
                    setUpsSeleccionado((ups || []).find(u => Number(u.id) === Number(mantenimientoEditar.upsId)) || null);
                    setFormulario({
                        tipoBateriaId: mantenimientoEditar.tipoBateriaId || null,
                        voltajeBateria: mantenimientoEditar.voltajeBateria ?? "",
                        capacidadBateriaAh: mantenimientoEditar.capacidadBateriaAh ?? "",
                        fechaMantenimiento: mantenimientoEditar.fechaMantenimiento?.substring(0, 10) || "",
                        responsableUsuarioId: mantenimientoEditar.responsableUsuarioId || null,
                        estadoMantenimientoId: mantenimientoEditar.estadoMantenimientoId || "",
                        observaciones: mantenimientoEditar.observaciones || ""
                    });
                } else {
                    setUpsSeleccionado(null);
                    setFormulario({ ...inicial, responsableUsuarioId: usuarioActualId });
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
    const actualizarEquipos = async () => {
        setCargando(true);
        try {
            const datos = (await obtenerUPS() || []).filter(u => String(u.estado || '').toLowerCase() !== 'de baja');
            setEquipos(datos);
            if (upsSeleccionado) {
                const actualizado = datos.find(u => Number(u.id) === Number(upsSeleccionado.id)) || null;
                setUpsSeleccionado(actualizado);
                if (actualizado) setFormulario(actual => {
                    const cambios = {};
                    for (const campo of ['tipoBateriaId', 'voltajeBateria', 'capacidadBateriaAh']) {
                        if (String(actual[campo] ?? '') === String(upsSeleccionado[campo] ?? '')) cambios[campo] = actualizado[campo] ?? (campo === 'tipoBateriaId' ? null : '');
                    }
                    return { ...actual, ...cambios };
                });
                if (!actualizado) setError('El UPS seleccionado ya no está disponible. Seleccione otro equipo.');
            }
        } catch { setError('No se pudo actualizar la lista de UPS. Abra nuevamente el listado y regrese para reintentar.'); }
        finally { setCargando(false); }
    };
    const actualizarResponsables = async () => {
        setCargando(true);
        try {
            const datos = await cargarResponsables();
            setResponsables(datos);
            if (!datos.some(r => Number(r.id) === Number(formulario.responsableUsuarioId) && r.activo !== false)) cambiar('responsableUsuarioId', null);
        } catch { setError('No se pudo actualizar la lista de responsables. Abra nuevamente Usuarios y regrese para reintentar.'); }
        finally { setCargando(false); }
    };
    const estadoNombre = estados.find(e => Number(e.id) === Number(formulario.estadoMantenimientoId))?.nombre || "";
    const esProgramado = estadoNombre.toLowerCase() === "programado";
    const conservaCantidad = mantenimientoEditar?.estado?.toLowerCase() === "completado" && estadoNombre.toLowerCase() === "completado";
    const cantidadBaterias = Number(conservaCantidad
        ? mantenimientoEditar.cantidadBateriasReemplazadas
        : upsSeleccionado?.cantidadBaterias || 0);

    const guardar = async () => {
        setError("");
        if (!formulario.tipoBateriaId) return setError("Seleccione el tipo de batería.");
        if (!upsSeleccionado) return setError("Debe seleccionar un UPS.");
        if (!formulario.estadoMantenimientoId) return setError("Seleccione el estado del cambio.");
        if (!formulario.fechaMantenimiento) return setError("Seleccione la fecha del cambio.");
        if (formulario.fechaMantenimiento > fechaActual() && !esProgramado)
            return setError("Una fecha futura solamente puede utilizarse con estado Programado.");
        if (!formulario.responsableUsuarioId) return setError("Seleccione el responsable del servicio.");
        if (cantidadBaterias <= 0) return setError("El UPS no tiene una cantidad válida de baterías.");

        const datos = {
            ...formulario,
            responsableUsuarioId: puedeElegirResponsable ? formulario.responsableUsuarioId : usuarioActualId,
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
            setError(e.response?.status === 403 ? "Solo puede modificar cambios asignados a su usuario. La selección de otro responsable corresponde al administrador." : e.response?.data?.mensaje || "No fue posible guardar el cambio de baterías.");
        } finally {
            setGuardando(false);
        }
    };

    return (
        <><Dialog open={open} onClose={guardando ? undefined : onClose} fullWidth maxWidth="md">
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
                        onChange={(_, valor) => { setUpsSeleccionado(valor); setFormulario(f => ({ ...f, tipoBateriaId: valor?.tipoBateriaId || null, voltajeBateria: valor?.voltajeBateria ?? "", capacidadBateriaAh: valor?.capacidadBateriaAh ?? "" })); }}
                        renderInput={params => <TextField {...params} required label="UPS" placeholder="Inventario, serie o agencia" />}
                    />
                    {!mantenimientoEditar && <Box sx={{ gridColumn: '1 / -1', mt: -1 }}>
                        <AccesoCatalogo href="/ups" permisos={['VER_UPS']} alguno={['CREAR_UPS', 'EDITAR_UPS']} disabled={!open || cargando || guardando} onRegresar={actualizarEquipos}>Crear o editar UPS</AccesoCatalogo>
                    </Box>}
                    <Box>
                    <TextField fullWidth select required label="Estado" value={estados.some(e => Number(e.id) === Number(formulario.estadoMantenimientoId)) ? formulario.estadoMantenimientoId : ""}
                        onChange={e => cambiar("estadoMantenimientoId", e.target.value)}>
                        {estados.map(estado => <MenuItem key={estado.id} value={estado.id}>{estado.nombre}</MenuItem>)}
                    </TextField>
                    <AccesoCatalogo href="/configuracion?catalogo=estados-mantenimiento" disabled={!open || cargando || guardando}>Consultar estados protegidos</AccesoCatalogo>
                    </Box>
                    <TextField required type="date" label="Fecha del cambio" value={formulario.fechaMantenimiento}
                        onChange={e => cambiar("fechaMantenimiento", e.target.value)}
                        slotProps={{ inputLabel: { shrink: true }, htmlInput: esProgramado ? {} : { max: fechaActual() } }} />
                    {puedeElegirResponsable ? <Box>
                        <Autocomplete options={responsables}
                            value={responsables.find(r => r.id === Number(formulario.responsableUsuarioId)) || null}
                            disabled={cargando || guardando}
                            getOptionLabel={r => `${r.nombreCompleto} (${r.nombreUsuario})${r.activo === false ? ' · Inactivo' : ''}`}
                            isOptionEqualToValue={(a, b) => a.id === b.id}
                            onChange={(_, valor) => cambiar("responsableUsuarioId", valor?.id || null)}
                            renderInput={params => <TextField {...params} required label="Usuario responsable" />} />
                        {tienePermiso('GESTIONAR_USUARIOS') && <Button disabled={cargando || guardando} onClick={() => setNuevoUsuario(true)}>Crear técnico</Button>}
                        <AccesoCatalogo href="/usuarios" permisos={['GESTIONAR_USUARIOS']} disabled={!open || cargando || guardando} onRegresar={actualizarResponsables}>Editar usuarios / técnicos</AccesoCatalogo>
                    </Box> : <TextField label="Usuario responsable" value={localStorage.getItem('nombreCompleto') || localStorage.getItem('nombreUsuario') || ''} disabled helperText="Se registra automáticamente con su usuario." />}
                    <TextField label="Cantidad de baterías" value={cantidadBaterias || ""} disabled
                        helperText={conservaCantidad ? "Cantidad registrada al completar el cambio." : "Se reemplaza el banco completo."} />
                    <SelectorBateria tipo={formulario.tipoBateriaId} voltaje={formulario.voltajeBateria} capacidad={formulario.capacidadBateriaAh} onChange={cambiar} disabled={cargando || guardando} />
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
        <NuevoUsuarioModal open={nuevoUsuario} onClose={() => setNuevoUsuario(false)} rolPredeterminado="Técnico"
            onGuardado={async () => { setResponsables(await cargarResponsables()); }} />
        </>
    );
}

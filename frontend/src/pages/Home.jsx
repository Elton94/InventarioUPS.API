import { useEffect, useState } from 'react';
import { Alert, Box, Button, Card, CardContent, Chip, CircularProgress, MenuItem, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Typography } from '@mui/material';
import { obtenerAgencias } from '../services/agenciaService';
import { obtenerUPS } from '../services/upsService';
import { obtenerMantenimientos } from '../services/mantenimientoService';
import { obtenerReemplazos } from '../services/bateriasService';
import { tienePermiso } from '../services/permisos';
const fecha = v => v ? new Date(`${v.slice(0, 10)}T12:00:00`).toLocaleDateString('es-HN') : '—';
const diaLocal = () => { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`; };

function Lista({ titulo, filas, rojo = false, realizados = false }) {
    return <Card variant="outlined" sx={{ mb: 3, borderColor: rojo && filas.length ? 'error.main' : 'divider' }}><CardContent>
        <Typography variant="h6" fontWeight={700} color={rojo && filas.length ? 'error.main' : 'text.primary'}>{titulo} · {filas.length}</Typography>
        <TableContainer sx={{ maxHeight: 420, mt: 2 }}><Table size="small" stickyHeader sx={{ minWidth: 680 }}>
            <TableHead><TableRow>{['UPS / agencia', 'Fecha', 'Batería', 'Cantidad', realizados ? 'Resultado' : 'Seguimiento'].map(c => <TableCell key={c}>{c}</TableCell>)}</TableRow></TableHead>
            <TableBody>{filas.map(f => <TableRow key={f.id || f.upsId} sx={{ bgcolor: rojo ? '#fff5f5' : undefined }}>
                <TableCell><Typography fontWeight={600}>{f.numeroInventario || f.numeroInventarioUPS}</Typography><Typography variant="caption">{f.agencia || f.agenciaNombre}</Typography></TableCell>
                <TableCell>{fecha(f.fecha || f.fechaMantenimiento)}</TableCell>
                <TableCell>{f.voltajeBateria ?? '—'} V · {f.capacidadBateriaAh ?? '—'} Ah<Typography variant="caption" display="block">{f.tipoBateria || ''}</Typography></TableCell>
                <TableCell>{f.cantidadBaterias || f.cantidadBateriasReemplazadas}</TableCell>
                <TableCell>{realizados ? <Chip size="small" color="success" label="Completado" /> : <><Chip size="small" color={rojo ? 'error' : 'info'} label={f.motivo || (f.dias === 0 ? 'Vence hoy' : f.dias < 0 ? `${-f.dias} días de atraso` : `En ${f.dias} días`)} />{f.trabajo && <Typography variant="caption" display="block" sx={{ mt: 1 }}>{f.trabajo}</Typography>}</>}</TableCell>
            </TableRow>)}{!filas.length && <TableRow><TableCell colSpan={5}>No hay registros en este período.</TableCell></TableRow>}</TableBody>
        </Table></TableContainer>
    </CardContent></Card>;
}

export default function Home() {
    const [agencias, setAgencias] = useState([]), [ups, setUps] = useState([]), [cambios, setCambios] = useState([]);
    const [agenda, setAgenda] = useState(null), [errores, setErrores] = useState([]), [cargando, setCargando] = useState(true);
    const [dias, setDias] = useState(90), [version, setVersion] = useState(0);
    const verAgencias = tienePermiso('VER_AGENCIAS'), verUPS = tienePermiso('VER_UPS'), verCambios = tienePermiso('VER_MANTENIMIENTO');
    useEffect(() => {
        let vigente = true;
        setCargando(true); setErrores([]); setAgenda(null); setAgencias([]); setUps([]); setCambios([]);
        const tareas = [
            verAgencias && ['agencias', obtenerAgencias, setAgencias],
            verUPS && ['inventario', obtenerUPS, setUps],
            verUPS && ['vencimientos', obtenerReemplazos, setAgenda],
            verCambios && ['cambios de baterías', obtenerMantenimientos, setCambios]
        ].filter(Boolean);
        Promise.allSettled(tareas.map(async ([nombre, cargar, guardar]) => {
            try { const data = await cargar(); if (vigente) guardar(data); }
            catch (e) { if (vigente) setErrores(v => [...v, `No se pudo cargar ${nombre}. ${e.response?.data?.mensaje || ''}`]); }
        })).then(() => { if (vigente) setCargando(false); });
        return () => { vigente = false; };
    }, [version, verAgencias, verUPS, verCambios]);
    const hoy = agenda?.fechaConsulta?.slice(0, 10) || diaLocal();
    const abiertos = cambios.filter(c => ['Programado', 'En proceso'].includes(c.estado));
    const vencidos = (agenda?.equipos || []).filter(u => u.dias < 0).map(u => ({ ...u }));
    const proximos = (agenda?.equipos || []).filter(u => u.dias >= 0 && u.dias <= dias).map(u => ({ ...u }));
    // Una fila por UPS; una orden atrasada sigue visible aunque otro cambio haya sido completado.
    for (const c of abiertos) {
        const fechaTrabajo = c.fechaMantenimiento.slice(0, 10);
        const fila = [...vencidos, ...proximos].find(u => u.upsId === c.upsId);
        if (fechaTrabajo < hoy) {
            if (fila && vencidos.includes(fila)) fila.trabajo = `${c.estado}: ${fecha(c.fechaMantenimiento)} sin completar`;
            else {
                const indice = proximos.findIndex(u => u.upsId === c.upsId);
                if (indice >= 0) proximos.splice(indice, 1);
                vencidos.push({ upsId: c.upsId, numeroInventario: c.numeroInventarioUPS, agencia: c.agenciaNombre,
                    fecha: c.fechaMantenimiento, cantidadBaterias: c.cantidadBateriasReemplazadas, voltajeBateria: c.voltajeBateria, capacidadBateriaAh: c.capacidadBateriaAh,
                    tipoBateria: c.tipoBateria, motivo: 'Trabajo sin completar', trabajo: c.estado });
            }
        } else if (fila) fila.trabajo = `${c.estado}: ${fecha(c.fechaMantenimiento)}`;
    }
    vencidos.sort((a,b) => a.fecha.localeCompare(b.fecha)); proximos.sort((a,b) => a.fecha.localeCompare(b.fecha));
    const desde = new Date(`${hoy}T12:00:00`); desde.setDate(desde.getDate() - 30); desde.setHours(0, 0, 0, 0);
    const recientes = cambios.filter(c => c.estado === 'Completado' && c.fechaMantenimiento.slice(0,10) <= hoy && new Date(c.fechaMantenimiento) >= desde)
        .sort((a,b) => b.fechaMantenimiento.localeCompare(a.fechaMantenimiento));
    const tarjetas = [verAgencias && ['Agencias activas', agencias.filter(a => a.activa).length], verUPS && ['UPS Nivel Nacional', ups.length],
        verUPS && ['UPS Fuera de servicio o de Baja', ups.filter(u => ['Fuera de servicio', 'De baja'].includes(u.estado)).length]].filter(Boolean);
    return <Box sx={{ maxWidth: 1500, mx: 'auto' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, mb: 3 }}><Box><Typography variant="h4" fontWeight={700}>Inicio</Typography><Typography color="text.secondary">Inventario nacional y seguimiento de reemplazos.</Typography></Box><Button onClick={() => setVersion(v => v + 1)} disabled={cargando}>Actualizar</Button></Box>
        {cargando ? <CircularProgress aria-label="Cargando inicio" /> : <>
            {errores.map(e => <Alert key={e} severity="warning" sx={{ mb: 2 }}>{e}</Alert>)}
            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' }, gap: 2, mb: 3 }}>{tarjetas.map(([titulo, valor]) => <Card key={titulo} variant="outlined"><CardContent><Typography color="text.secondary">{titulo}</Typography><Typography variant="h4" fontWeight={700}>{valor}</Typography></CardContent></Card>)}</Box>
            {(verUPS || verCambios) && <>
                {vencidos.length > 0 && <Alert severity="error" sx={{ mb: 2 }}><strong>{vencidos.length} UPS requieren atención.</strong> Hay reemplazos vencidos o trabajos con fecha pasada sin completar. Programarlos no elimina el atraso.</Alert>}
                <Lista titulo="Reemplazos vencidos y trabajos atrasados" filas={vencidos} rojo />
            </>}
            {verUPS && <>
                {!!agenda?.incompletos && <Alert severity="warning" sx={{ mb: 2 }}>{agenda.incompletos} UPS necesitan corregir fechas o cantidades para calcular su vencimiento.</Alert>}
                <TextField select size="small" label="Próximos reemplazos" value={dias} onChange={e => setDias(Number(e.target.value))} sx={{ mb: 2, minWidth: 220 }}>{[30,90,180,365].map(d => <MenuItem key={d} value={d}>Próximos {d} días</MenuItem>)}</TextField>
                <Lista titulo="Próximos reemplazos necesarios" filas={proximos} />
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>Vencimientos según la última sustitución y la política vigente. Se excluyen agencias inactivas y UPS fuera de servicio o de baja. No requieren costos configurados.</Typography>
            </>}
            {verCambios && <Lista titulo="Reemplazos realizados en los últimos 30 días" filas={recientes} realizados />}
        </>}
    </Box>;
}

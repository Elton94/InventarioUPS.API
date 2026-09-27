import { useEffect, useState } from 'react';
import { Alert, Box, Button, Card, CardContent, Checkbox, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, MenuItem, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Typography } from '@mui/material';
import api from '../services/api';
const catalogos = [
    { id: 'departamentos', nombre: 'Departamentos', max: 100 },
    { id: 'ciudades', nombre: 'Ciudades', padre: 'departamentos', etiquetaPadre: 'Departamento', max: 100 },
    { id: 'tipos-bateria', nombre: 'Tipos de batería', max: 80 },
    { id: 'modelos-bateria', nombre: 'Modelos de batería', padre: 'tipos-bateria', etiquetaPadre: 'Tipo de batería', max: 150 },
    { id: 'marcas-ups', nombre: 'Marcas de UPS', max: 100 },
    { id: 'modelos-ups', nombre: 'Modelos de UPS', padre: 'marcas-ups', etiquetaPadre: 'Marca', max: 100 },
    { id: 'tipos-agencia', nombre: 'Tipos de agencia', protegido: true },
    { id: 'estados-ups', nombre: 'Estados de UPS', protegido: true },
    { id: 'estados-mantenimiento', nombre: 'Estados de cambios', protegido: true }
];
const leer = async id => (await api.get(`/configuracion/catalogos/${id}`)).data;
function detalleHistorial(valor) {
    if (!valor) return '—';
    try { const r = JSON.parse(valor); return `${r.Nombre} · ${r.Activo ? 'Activo' : 'Inactivo'}${r.PadreId ? ` · Referencia ${r.PadreId}` : ''}`; }
    catch { return valor; }
}
export default function Configuracion() {
    const [catalogo, setCatalogo] = useState('departamentos'), [filas, setFilas] = useState([]), [padres, setPadres] = useState([]);
    const [busqueda, setBusqueda] = useState(''), [estado, setEstado] = useState('todos'), [cargando, setCargando] = useState(true);
    const [error, setError] = useState(''), [mensaje, setMensaje] = useState(''), [version, setVersion] = useState(0);
    const [edicion, setEdicion] = useState(null), [form, setForm] = useState({ nombre: '', activo: true, padreId: '' });
    const [guardando, setGuardando] = useState(false), [errorDialogo, setErrorDialogo] = useState('');
    const [historial, setHistorial] = useState(false), [pagina, setPagina] = useState(1), [auditoria, setAuditoria] = useState({ total: 0, registros: [] });
    const definicion = catalogos.find(c => c.id === catalogo);
    useEffect(() => {
        let vigente = true;
        setCargando(true); setError(''); setFilas([]); setPadres([]);
        const cargar = historial
            ? api.get('/configuracion/historial', { params: { pagina } }).then(r => { if (vigente) setAuditoria(r.data); })
            : Promise.all([leer(catalogo), definicion.padre ? leer(definicion.padre) : Promise.resolve([])]).then(([datos, superiores]) => { if (vigente) { setFilas(datos); setPadres(superiores); } });
        cargar.catch(e => { if (vigente) setError(e.response?.data?.mensaje || 'No se pudo cargar la configuración.'); })
            .finally(() => { if (vigente) setCargando(false); });
        return () => { vigente = false; };
    }, [catalogo, definicion.padre, historial, pagina, version]);
    const abrir = fila => {
        setEdicion(fila || { id: 0 }); setErrorDialogo('');
        setForm({ nombre: fila?.nombre || '', activo: fila?.activo ?? true, padreId: fila?.padreId ?? '' });
    };
    const guardar = async () => {
        if (!form.nombre.trim() || (definicion.padre && !form.padreId)) return setErrorDialogo('Complete el nombre y la opción superior.');
        setGuardando(true); setErrorDialogo('');
        const datos = { nombre: form.nombre.trim(), activo: form.activo, padreId: definicion.padre ? Number(form.padreId) : null, anterior: edicion.id ? edicion : null };
        try {
            if (edicion.id) await api.put(`/configuracion/catalogos/${catalogo}/${edicion.id}`, datos);
            else await api.post(`/configuracion/catalogos/${catalogo}`, datos);
            setEdicion(null); setMensaje('Cambio guardado y registrado en el historial.'); setVersion(v => v + 1);
        } catch (e) { setErrorDialogo(e.response?.data?.mensaje || 'No se pudo guardar la opción.'); }
        finally { setGuardando(false); }
    };
    const visibles = filas.filter(f => f.nombre.toLocaleLowerCase().includes(busqueda.toLocaleLowerCase().trim()) &&
        (estado === 'todos' || f.activo === (estado === 'activos')));
    return <Box sx={{ maxWidth: 1300, mx: 'auto' }}>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'space-between', gap: 2, mb: 3 }}>
            <Box><Typography variant="h4" fontWeight={700}>Configuración</Typography><Typography color="text.secondary">Administre las opciones disponibles en los formularios.</Typography></Box>
            <Button variant="outlined" onClick={() => { setHistorial(v => !v); setMensaje(''); setPagina(1); }}>{historial ? 'Volver a catálogos' : 'Historial de cambios'}</Button>
        </Box>
        {error && <Alert severity="error" sx={{ mb: 2 }} action={<Button onClick={() => setVersion(v => v + 1)}>Reintentar</Button>}>{error}</Alert>}
        {mensaje && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setMensaje('')}>{mensaje}</Alert>}
        {!historial && <>
            <Card variant="outlined" sx={{ mb: 2 }}><CardContent><Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
                <TextField select label="Catálogo" value={catalogo} sx={{ minWidth: 230 }} onChange={e => { setCatalogo(e.target.value); setBusqueda(''); setEstado('todos'); setMensaje(''); }}>{catalogos.map(c => <MenuItem key={c.id} value={c.id}>{c.nombre}</MenuItem>)}</TextField>
                <TextField label="Buscar nombre" value={busqueda} onChange={e => setBusqueda(e.target.value)} sx={{ flex: 1, minWidth: 180 }} />
                <TextField select label="Estado" value={estado} onChange={e => setEstado(e.target.value)} sx={{ minWidth: 150 }}><MenuItem value="todos">Todos</MenuItem><MenuItem value="activos">Activos</MenuItem><MenuItem value="inactivos">Inactivos</MenuItem></TextField>
                {!definicion.protegido && <Button variant="contained" disabled={cargando || Boolean(error)} onClick={() => abrir(null)}>Agregar opción</Button>}
            </Box></CardContent></Card>
            <Alert severity="info" sx={{ mb: 2 }}>{definicion.protegido ? 'Catálogo protegido: sus valores controlan reglas del sistema y solo se pueden consultar.' : 'Desactive las opciones que ya no se utilizarán. Los registros existentes se conservan; no se eliminan opciones ni se trasladan equipos automáticamente.'}</Alert>
            {catalogo === 'ciudades' && <Alert severity="info" sx={{ mb: 2 }}>La clasificación Local/Foránea está protegida. Corregir el nombre conserva la clasificación. Las ciudades nuevas se registran como Foráneas.</Alert>}
        </>}
        {cargando ? <CircularProgress aria-label="Cargando configuración" /> : !error && (historial ? <>
            <TableContainer component={Card} variant="outlined"><Table size="small" sx={{ minWidth: 850 }}><TableHead><TableRow>{['Fecha', 'Usuario', 'Catálogo / registro', 'Acción', 'Antes', 'Después'].map(t => <TableCell key={t}>{t}</TableCell>)}</TableRow></TableHead><TableBody>
                {auditoria.registros.map(r => <TableRow key={r.id}><TableCell>{new Date(r.fecha.endsWith('Z') ? r.fecha : `${r.fecha}Z`).toLocaleString('es-HN')}</TableCell><TableCell>{r.usuario}</TableCell><TableCell>{catalogos.find(c => c.id === r.catalogo)?.nombre || r.catalogo} · {r.registroId}</TableCell><TableCell>{r.accion}</TableCell><TableCell>{detalleHistorial(r.antes)}</TableCell><TableCell>{detalleHistorial(r.despues)}</TableCell></TableRow>)}
                {!auditoria.registros.length && <TableRow><TableCell colSpan={6}>Todavía no hay modificaciones de catálogos registradas.</TableCell></TableRow>}
            </TableBody></Table></TableContainer>
            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mt: 2 }}><Button disabled={pagina === 1} onClick={() => setPagina(p => p - 1)}>Anterior</Button><Typography>Página {pagina} · {auditoria.total} cambios</Typography><Button disabled={pagina * 25 >= auditoria.total} onClick={() => setPagina(p => p + 1)}>Siguiente</Button></Box>
        </> : <TableContainer component={Card} variant="outlined"><Table sx={{ minWidth: 620 }}><TableHead><TableRow><TableCell>Nombre</TableCell>{definicion.padre && <TableCell>{definicion.etiquetaPadre}</TableCell>}{catalogo === 'ciudades' && <TableCell>Clasificación</TableCell>}<TableCell>Estado</TableCell><TableCell>Acciones</TableCell></TableRow></TableHead><TableBody>
            {visibles.map(f => <TableRow key={f.id}><TableCell>{f.nombre}</TableCell>{definicion.padre && <TableCell>{padres.find(p => p.id === f.padreId)?.nombre}{padres.find(p => p.id === f.padreId)?.activo === false && ' (Inactivo)'}</TableCell>}{catalogo === 'ciudades' && <TableCell>{f.esLocal ? 'Local' : 'Foránea'}</TableCell>}<TableCell><Chip size="small" color={f.activo ? 'success' : 'default'} label={f.activo ? 'Activo' : 'Inactivo'} /></TableCell><TableCell>{definicion.protegido ? 'Protegido' : <Button onClick={() => abrir(f)}>Editar / estado</Button>}</TableCell></TableRow>)}
            {!visibles.length && <TableRow><TableCell colSpan={5}>No hay opciones que coincidan con los filtros.</TableCell></TableRow>}
        </TableBody></Table></TableContainer>)}
        <Dialog open={Boolean(edicion)} onClose={guardando ? undefined : () => setEdicion(null)} fullWidth maxWidth="sm">
            <DialogTitle>{edicion?.id ? 'Editar opción' : 'Agregar opción'} · {definicion.nombre}</DialogTitle>
            <DialogContent><Box sx={{ display: 'grid', gap: 3, pt: 1 }}>
                {errorDialogo && <Alert severity="error">{errorDialogo}</Alert>}
                <TextField autoFocus required label="Nombre" value={form.nombre} disabled={guardando} onChange={e => setForm(f => ({ ...f, nombre: e.target.value }))} slotProps={{ htmlInput: { maxLength: definicion.max } }} />
                {definicion.padre && <TextField select required label={definicion.etiquetaPadre} value={form.padreId} disabled={guardando} onChange={e => setForm(f => ({ ...f, padreId: e.target.value }))}>{padres.filter(p => p.activo || p.id === Number(form.padreId)).map(p => <MenuItem key={p.id} value={p.id}>{p.nombre}{!p.activo && ' (Inactivo)'}</MenuItem>)}</TextField>}
                <FormControlLabel control={<Checkbox checked={form.activo} disabled={guardando} onChange={e => setForm(f => ({ ...f, activo: e.target.checked }))} />} label="Registro activo" />
                {definicion.padre && padres.find(p => p.id === Number(form.padreId))?.activo === false && <Alert severity="warning">La opción superior está inactiva. Este registro no estará disponible para nuevas selecciones hasta reactivarla.</Alert>}
                {!form.activo && <Alert severity="warning">Se conservará el historial. Esta opción y las opciones que dependan de ella dejarán de estar disponibles para nuevas selecciones.</Alert>}
                {edicion?.id > 0 && <Typography variant="body2" color="text.secondary">Corregir el nombre actualiza su presentación en los registros relacionados. No cambie su significado; agregue otra opción si representa algo diferente.</Typography>}
            </Box></DialogContent>
            <DialogActions sx={{ px: 3, pb: 2 }}><Button disabled={guardando} onClick={() => setEdicion(null)}>Cancelar</Button><Button variant="contained" disabled={guardando} onClick={guardar}>{guardando ? 'Guardando…' : 'Guardar'}</Button></DialogActions>
        </Dialog>
    </Box>;
}

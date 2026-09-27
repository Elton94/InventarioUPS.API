import { useEffect, useState } from 'react';
import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Typography } from '@mui/material';
import { obtenerCostosTipo, guardarCostosTipo } from '../../services/bateriasService';

export default function CostosTipo({ anio, referencia, onGuardado }) {
    const [open, setOpen] = useState(false), [filas, setFilas] = useState([]), [error, setError] = useState('');
    const [cargando, setCargando] = useState(false), [guardando, setGuardando] = useState(false);
    useEffect(() => {
        if (!open) return;
        let vigente = true;
        setError(''); setFilas([]); setCargando(true);
        obtenerCostosTipo(anio).then(datos => { if (vigente) setFilas(datos); })
            .catch(e => { if (vigente) setError(e.response?.data?.mensaje || 'No se pudieron cargar los costos.'); })
            .finally(() => { if (vigente) setCargando(false); });
        return () => { vigente = false; };
    }, [open, anio]);
    const guardar = async () => {
        if (filas.some(f => !f.usaReferencia && (!/^\d+(\.\d{1,2})?$/.test(String(f.costoUnitario)) || Number(f.costoUnitario) <= 0 || Number(f.costoUnitario) >= 1e16)))
            return setError('Todos los costos deben ser positivos y tener hasta dos decimales.');
        setError(''); setGuardando(true);
        try {
            await guardarCostosTipo(anio, filas.map(f => ({ tipoBateriaId: f.tipoBateriaId, costoUnitario: f.usaReferencia ? null : Number(f.costoUnitario) })));
            await onGuardado(); setOpen(false);
        } catch (e) { setError(e.response?.data?.mensaje || 'No se guardaron los costos.'); }
        finally { setGuardando(false); }
    };
    const cambiar = (id, valor) => setFilas(filas => filas.map(f => f.tipoBateriaId === id ? { ...f, costoUnitario: valor, usaReferencia: false } : f));
    return <><Button variant="outlined" disabled={!referencia} onClick={() => setOpen(true)}>Costos por tipo</Button>
        <Dialog open={open} onClose={guardando ? undefined : () => setOpen(false)} fullWidth maxWidth="md">
            <DialogTitle>Costos por tipo de batería · {anio}</DialogTitle>
            <DialogContent>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                <Typography sx={{ mb: 2 }}>Cada tipo empieza con el valor de referencia del año: L {Number(referencia || 0).toFixed(2)}. Edite únicamente los valores que necesite diferenciar.</Typography>
                <TableContainer><Table><TableHead><TableRow><TableCell>Tipo de batería</TableCell><TableCell>Costo unitario (L)</TableCell><TableCell>Origen</TableCell></TableRow></TableHead><TableBody>
                    {filas.map(f => <TableRow key={f.tipoBateriaId}>
                        <TableCell>{f.nombre}{!f.activo && ' (Inactivo)'}</TableCell>
                        <TableCell><TextField size="small" type="number" value={f.costoUnitario} disabled={cargando || guardando} onChange={e => cambiar(f.tipoBateriaId, e.target.value)} slotProps={{ htmlInput: { min: 0.01, step: 0.01, 'aria-label': `Costo de ${f.nombre}` } }} /></TableCell>
                        <TableCell>{f.usaReferencia ? 'Referencia del año' : <Button disabled={guardando} onClick={() => setFilas(filas => filas.map(x => x.tipoBateriaId === f.tipoBateriaId ? { ...x, costoUnitario: referencia, usaReferencia: true } : x))}>Restablecer referencia</Button>}</TableCell>
                    </TableRow>)}
                    {cargando && <TableRow><TableCell colSpan={3}>Cargando costos…</TableCell></TableRow>}
                </TableBody></Table></TableContainer>
                <Box sx={{ mt: 2 }}><Alert severity="info">El precio por tipo se aplica a todas sus capacidades y voltajes. Revise las especificaciones antes de realizar una compra.</Alert></Box>
            </DialogContent>
            <DialogActions><Button disabled={guardando} onClick={() => setOpen(false)}>Cancelar</Button><Button variant="contained" disabled={cargando || guardando || !filas.length} onClick={guardar}>{guardando ? 'Guardando…' : 'Guardar costos'}</Button></DialogActions>
        </Dialog></>;
}

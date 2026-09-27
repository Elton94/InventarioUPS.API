import { useEffect, useState } from 'react';
import { Alert, Autocomplete, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField, Typography } from '@mui/material';
import { obtenerBaterias, obtenerCostosModelo, guardarCostoModelo } from '../../services/bateriasService';

export default function CostosModelo({ anio, onGuardado }) {
    const [open, setOpen] = useState(false), [modelos, setModelos] = useState([]), [costos, setCostos] = useState([]);
    const [modelo, setModelo] = useState(null), [costo, setCosto] = useState(''), [error, setError] = useState('');
    const [cargando, setCargando] = useState(false), [guardando, setGuardando] = useState(false);
    useEffect(() => {
        if (!open) return;
        let vigente = true;
        setError(''); setModelo(null); setCosto(''); setCargando(true);
        Promise.all([obtenerBaterias(), obtenerCostosModelo(anio)]).then(([catalogo, precios]) => {
            if (vigente) { setModelos(catalogo.modelos); setCostos(precios); }
        }).catch(() => { if (vigente) setError('No se pudieron cargar los costos por modelo.'); })
            .finally(() => { if (vigente) setCargando(false); });
        return () => { vigente = false; };
    }, [open, anio]);
    const guardar = async () => {
        if (!modelo || !/^\d+(\.\d{1,2})?$/.test(String(costo)) || Number(costo) <= 0)
            return setError('Seleccione un modelo y escriba un costo positivo con hasta dos decimales.');
        setError(''); setGuardando(true);
        try { await guardarCostoModelo(anio, modelo.id, costo); onGuardado(); setOpen(false); }
        catch (e) { setError(e.response?.data?.mensaje || 'No se pudo guardar el costo.'); }
        finally { setGuardando(false); }
    };
    return <><Button variant="outlined" onClick={() => setOpen(true)}>Costos por modelo</Button>
        <Dialog open={open} onClose={guardando ? undefined : () => setOpen(false)} fullWidth maxWidth="sm">
            <DialogTitle>Costos por modelo · {anio}</DialogTitle>
            <DialogContent><Box sx={{ display: 'grid', gap: 3, pt: 1 }}>
                {error && <Alert severity="error">{error}</Alert>}
                <Typography color="text.secondary">El costo específico reemplaza al costo general para este modelo durante el año seleccionado.</Typography>
                <Autocomplete options={modelos} value={modelo} disabled={cargando || guardando} getOptionLabel={m => `${m.tipo} · ${m.nombre}`}
                    onChange={(_, m) => { setModelo(m); setCosto(costos.find(c => c.modeloBateriaId === m?.id)?.costoUnitario ?? ''); }}
                    renderInput={params => <TextField {...params} label="Modelo de batería" required />} />
                <TextField required label="Costo unitario (L)" type="number" value={costo} disabled={cargando || guardando} onChange={e => setCosto(e.target.value)} slotProps={{ htmlInput: { min: 0.01, step: 0.01 } }} />
                {!cargando && !modelos.length && <Alert severity="info">Agregue los modelos desde el formulario de UPS o de cambios de baterías.</Alert>}
            </Box></DialogContent>
            <DialogActions><Button disabled={guardando} onClick={() => setOpen(false)}>Cerrar</Button><Button variant="contained" disabled={cargando || guardando || !modelo} onClick={guardar}>{guardando ? 'Guardando…' : 'Guardar costo'}</Button></DialogActions>
        </Dialog></>;
}

import { useEffect, useState } from 'react';
import { Alert, Box, MenuItem, TextField } from '@mui/material';
import { obtenerBaterias } from '../../services/bateriasService';

export default function SelectorBateria({ tipo, voltaje, capacidad, onChange, disabled = false }) {
    const [tipos, setTipos] = useState([]), [error, setError] = useState(''), [cargando, setCargando] = useState(true);
    useEffect(() => {
        let vigente = true;
        obtenerBaterias().then(data => { if (vigente) setTipos(data.tipos); })
            .catch(() => { if (vigente) setError('No se pudo cargar el catálogo de tipos. Cierre y vuelva a abrir el formulario.'); })
            .finally(() => { if (vigente) setCargando(false); });
        return () => { vigente = false; };
    }, []);
    return <Box sx={{ display: 'grid', gap: 2, gridColumn: '1 / -1', gridTemplateColumns: { xs: '1fr', sm: '2fr 1fr 1fr' } }}>
        {error && <Alert sx={{ gridColumn: '1 / -1' }} severity="error">{error}</Alert>}
        <TextField select required label="Tipo de batería" value={tipo || ''} disabled={disabled || cargando}
            onChange={e => onChange('tipoBateriaId', Number(e.target.value))}>
            {tipos.filter(t => t.activo !== false || t.id === Number(tipo)).map(t => <MenuItem key={t.id} value={t.id}>{t.nombre}{t.activo === false ? ' (Inactivo)' : ''}</MenuItem>)}
        </TextField>
        <TextField required type="number" label="Voltaje (V)" value={voltaje ?? ''} disabled={disabled}
            helperText="Por batería" onChange={e => onChange('voltajeBateria', e.target.value)} slotProps={{ htmlInput: { min: 0.001, max: 999999.999, step: 0.001 } }} />
        <TextField required type="number" label="Capacidad (Ah)" value={capacidad ?? ''} disabled={disabled}
            helperText="Por batería" onChange={e => onChange('capacidadBateriaAh', e.target.value)} slotProps={{ htmlInput: { min: 0.001, max: 999999.999, step: 0.001 } }} />
    </Box>;
}

import { useEffect, useState } from 'react';
import { Alert, Autocomplete, Box, MenuItem, TextField } from '@mui/material';
import { obtenerBaterias } from '../../services/bateriasService';

export default function SelectorBateria({ value, onChange, tipoValue, onTipoChange, modeloOpcional = false, disabled = false }) {
    const [catalogo, setCatalogo] = useState({ tipos: [], modelos: [] });
    const [tipoInterno, setTipoInterno] = useState('');
    const [error, setError] = useState('');
    const [cargando, setCargando] = useState(true);
    useEffect(() => {
        let vigente = true;
        obtenerBaterias().then(data => { if (vigente) setCatalogo(data); })
            .catch(() => { if (vigente) setError('No se pudo cargar el catálogo de baterías. Cierre y vuelva a abrir el formulario.'); })
            .finally(() => { if (vigente) setCargando(false); });
        return () => { vigente = false; };
    }, []);
    const seleccionado = catalogo.modelos.find(m => m.id === Number(value)) || null;
    const tipo = tipoValue !== undefined ? tipoValue : tipoInterno;
    useEffect(() => {
        if (seleccionado) setTipoInterno(seleccionado.tipoBateriaId);
    }, [seleccionado]);
    const modelos = catalogo.modelos.filter(m => m.tipoBateriaId === Number(tipo) && (m.activo !== false || m.id === Number(value)));
    return <Box sx={{ display: 'grid', gap: 2, gridColumn: '1 / -1' }}>
        {error && <Alert severity="error">{error}</Alert>}
        {seleccionado?.activo === false && <Alert severity="warning">El modelo está inactivo. Puede conservarlo en su registro existente; para nuevas selecciones utilice un modelo activo.</Alert>}
        <TextField select required label="Tipo de batería" value={tipo || ''} disabled={disabled || cargando}
            onChange={e => { onChange(null); setTipoInterno(e.target.value); onTipoChange?.(Number(e.target.value)); }}>
            {catalogo.tipos.filter(t => t.activo !== false || t.id === Number(tipo)).map(t => <MenuItem key={t.id} value={t.id}>{t.nombre}{t.activo === false ? ' (Inactivo)' : ''}</MenuItem>)}
        </TextField>
        <Autocomplete options={modelos} value={seleccionado} disabled={disabled || !tipo || cargando}
            getOptionLabel={m => m.nombre} isOptionEqualToValue={(a, b) => a.id === b.id}
            onChange={(_, m) => { onChange(m?.id || null); if (m) onTipoChange?.(m.tipoBateriaId); }}
            renderInput={params => <TextField {...params} required={!modeloOpcional}
                label={modeloOpcional ? 'Modelo / referencia de batería (opcional)' : 'Modelo / referencia de batería'}
                helperText={modeloOpcional ? 'Si todavía no lo conoce, déjelo vacío. La proyección usará el costo general del año.' : 'Seleccione el repuesto que se instalará. Las nuevas referencias se agregan en Configuración.'} />} />
        <Alert severity="info">El tipo clasifica la batería; la selección del catálogo no certifica compatibilidad. Confirme el repuesto con el manual del UPS.</Alert>
    </Box>;
}

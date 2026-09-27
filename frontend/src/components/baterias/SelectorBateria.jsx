import { useEffect, useState } from 'react';
import { Alert, Autocomplete, Box, Button, MenuItem, TextField } from '@mui/material';
import { obtenerBaterias, crearModeloBateria } from '../../services/bateriasService';

export default function SelectorBateria({ value, onChange, disabled = false }) {
    const [catalogo, setCatalogo] = useState({ tipos: [], modelos: [] });
    const [tipo, setTipo] = useState('');
    const [texto, setTexto] = useState('');
    const [error, setError] = useState('');
    const [guardando, setGuardando] = useState(false);
    const [cargando, setCargando] = useState(true);
    useEffect(() => {
        let vigente = true;
        obtenerBaterias().then(data => { if (vigente) setCatalogo(data); })
            .catch(() => { if (vigente) setError('No se pudo cargar el catálogo de baterías. Cierre y vuelva a abrir el formulario.'); })
            .finally(() => { if (vigente) setCargando(false); });
        return () => { vigente = false; };
    }, []);
    const seleccionado = catalogo.modelos.find(m => m.id === Number(value)) || null;
    useEffect(() => {
        if (seleccionado) { setTipo(seleccionado.tipoBateriaId); setTexto(seleccionado.nombre); }
    }, [seleccionado]);
    const modelos = catalogo.modelos.filter(m => m.tipoBateriaId === Number(tipo));
    const crear = async () => {
        setError(''); setGuardando(true);
        try {
            const modelo = await crearModeloBateria(tipo, texto.trim());
            setCatalogo(c => ({ ...c, modelos: [...c.modelos.filter(m => m.id !== modelo.id), modelo] }));
            onChange(modelo.id);
        } catch (e) { setError(e.response?.data?.mensaje || 'No se pudo guardar el modelo.'); }
        finally { setGuardando(false); }
    };
    return <Box sx={{ display: 'grid', gap: 2, gridColumn: '1 / -1' }}>
        {error && <Alert severity="error">{error}</Alert>}
        <TextField select required label="Tipo de batería" value={tipo} disabled={disabled || cargando || guardando}
            onChange={e => { onChange(null); setTipo(e.target.value); setTexto(''); }}>
            {catalogo.tipos.map(t => <MenuItem key={t.id} value={t.id}>{t.nombre}</MenuItem>)}
        </TextField>
        <Autocomplete freeSolo options={modelos} value={seleccionado} inputValue={texto}
            disabled={disabled || !tipo || cargando || guardando}
            getOptionLabel={m => typeof m === 'string' ? m : m.nombre}
            isOptionEqualToValue={(a, b) => a.id === b.id}
            onInputChange={(_, text, reason) => { if (reason === 'input') { setTexto(text); if (seleccionado) onChange(null); } }}
            onChange={(_, m) => { if (m && typeof m !== 'string') onChange(m.id); else { onChange(null); setTexto(m || ''); } }}
            renderInput={params => <TextField {...params} required label="Modelo de batería" helperText="Seleccione un modelo o escriba la referencia completa del fabricante y agréguela al catálogo." />} />
        {!seleccionado && tipo && texto.trim() && <Button disabled={disabled || guardando} variant="outlined" onClick={crear}>
            {guardando ? 'Agregando…' : 'Agregar este modelo al catálogo'}
        </Button>}
        <Alert severity="info">El tipo clasifica la batería; confirme la compatibilidad del modelo con el UPS antes de instalarlo.</Alert>
    </Box>;
}

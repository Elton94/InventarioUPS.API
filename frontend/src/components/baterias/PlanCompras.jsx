import { Alert, Box, Button, Card, CardContent, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from '@mui/material';
import { resumirCompras, csvProyeccion } from '../../utils/planCompras';
const moneda = n => new Intl.NumberFormat('es-HN', { style: 'currency', currency: 'HNL' }).format(n);
export default function PlanCompras({ equipos, anio }) {
    const resumen = resumirCompras(equipos, anio, new Date().getMonth() + 1);
    const referencia = equipos.filter(u => u.usaCostoGeneral).length;
    const exportar = () => {
        const url = URL.createObjectURL(new Blob([csvProyeccion(equipos, anio)], { type: 'text/csv;charset=utf-8;' }));
        const a = document.createElement('a'); a.href = url; a.download = `Presupuesto-baterias-${anio}.csv`; a.click();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    };
    return <Box sx={{ mb: 3 }}>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'space-between', alignItems: 'center', gap: 1, mb: 2 }}><Typography variant="h6" fontWeight={700}>Plan de compras · {anio}</Typography><Button variant="outlined" onClick={exportar} disabled={!equipos.length}>Exportar detalle CSV</Button></Box>
        {!!referencia && <Alert severity="warning" sx={{ mb: 2 }}>{referencia} UPS utilizan el costo general de referencia. Configure costos por modelo para una estimación más precisa. Los equipos sin modelo identificado requieren completar su ficha.</Alert>}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: '1fr 1fr' }, gap: 2 }}>
            <Card variant="outlined"><CardContent><Typography fontWeight={700}>Distribución mensual</Typography>
                <Typography variant="caption" color="text.secondary">Los vencidos del año actual se asignan al mes actual para planificar su atención.</Typography>
                <TableContainer><Table size="small"><TableHead><TableRow><TableCell>Mes</TableCell><TableCell align="right">Cambios</TableCell><TableCell align="right">Baterías</TableCell><TableCell align="right">Estimado</TableCell></TableRow></TableHead><TableBody>
                    {resumen.meses.map(m => <TableRow key={m.mes}><TableCell>{new Date(anio,m.mes-1,1).toLocaleDateString('es-HN',{month:'long'})}</TableCell><TableCell align="right">{m.cambios}</TableCell><TableCell align="right">{m.baterias}</TableCell><TableCell align="right">{moneda(m.costo)}</TableCell></TableRow>)}
                </TableBody></Table></TableContainer>
            </CardContent></Card>
            <Card variant="outlined"><CardContent><Typography fontWeight={700}>Baterías por tipo y modelo</Typography>
                <TableContainer><Table size="small"><TableHead><TableRow><TableCell>Modelo / tipo</TableCell><TableCell align="right">Unidades</TableCell><TableCell align="right">Estimado</TableCell></TableRow></TableHead><TableBody>
                    {resumen.modelos.map((m,i) => <TableRow key={i}><TableCell>{m.modelo}<Typography variant="caption" display="block">{m.tipo} · {m.general ? 'Costo general' : 'Costo específico'}</Typography></TableCell><TableCell align="right">{m.baterias}</TableCell><TableCell align="right">{moneda(m.costo)}</TableCell></TableRow>)}
                    {!resumen.modelos.length && <TableRow><TableCell colSpan={3}>No hay compras previstas con estos filtros.</TableCell></TableRow>}
                </TableBody></Table></TableContainer>
                <Alert severity="info" sx={{ mt: 2 }}>Estimación basada en el inventario y la política actuales. No incluye ampliaciones futuras, mano de obra, transporte ni variaciones de precios no configuradas.</Alert>
            </CardContent></Card>
        </Box>
    </Box>;
}

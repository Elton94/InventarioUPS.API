import test from 'node:test';
import assert from 'node:assert/strict';
import { resumirCompras, csvProyeccion } from '../src/utils/planCompras.js';
const equipo = { agencia: 'Centro', numeroInventario: '001', modeloBateriaId: 1, modeloBateria: 'AGM', tipoBateria: 'Plomo',
    cantidadBaterias: 4, costoUnitarioBateria: 750, cantidadCambios: 3, bateriasProyectadas: 12, costoEstimado: 9000,
    fechasReemplazo: ['2026-10-10T00:00:00', '2026-12-10T00:00:00'], estaVencido: true, fechaVencimientoPendiente: '2026-08-10', usaCostoGeneral: false };
test('La distribución mensual incluye una vez el vencido y reconcilia compras y total', () => {
    const r = resumirCompras([equipo], 2026, 9);
    assert.equal(r.meses[8].cambios, 1);
    assert.equal(r.meses.reduce((n,m) => n + m.cambios, 0), 3);
    assert.equal(r.meses.reduce((n,m) => n + m.baterias, 0), 12);
    assert.equal(r.meses.reduce((n,m) => n + m.costo, 0), 9000);
    assert.equal(r.modelos[0].costo, 9000);
});
test('Un mismo modelo agrupa compras de varias agencias', () => {
    const r = resumirCompras([equipo, {...equipo, agencia: 'Otra'}], 2026, 9);
    assert.equal(r.modelos.length, 1);
    assert.equal(r.modelos[0].baterias, 24);
});
test('Exportación conserva ceros de inventario, escapa comillas y neutraliza fórmulas', () => {
    const csv = csvProyeccion([{...equipo, agencia: '=SUM(1)', modeloBateria: 'Modelo "A"'}], 2026);
    assert.ok(csv.includes('"\'=SUM(1)"'));
    assert.ok(csv.includes('"001"'));
    assert.ok(csv.includes('"Modelo ""A"""'));
    assert.ok(csv.includes('Específico por modelo'));
});

test('Sin modelo, las compras se separan por tipo de batería', () => {
    const base = { ...equipo, modeloBateriaId: null, modeloBateria: 'Sin identificar', usaCostoGeneral: true };
    const r = resumirCompras([base, { ...base, tipoBateria: 'Litio' }, { ...base, agencia: 'Otra' }], 2026, 9);
    assert.equal(r.modelos.length, 2);
    assert.equal(r.modelos.find(m => m.tipo === 'Plomo').baterias, 24);
    assert.equal(r.modelos.find(m => m.tipo === 'Litio').baterias, 12);
});

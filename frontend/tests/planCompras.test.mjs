import test from 'node:test';
import assert from 'node:assert/strict';
import { resumirCompras, csvProyeccion } from '../src/utils/planCompras.js';
const equipo = { agencia: 'Centro', numeroInventario: '001', tipoBateriaId: 1, voltajeBateria: 12, capacidadBateriaAh: 9, tipoBateria: 'Plomo',
    cantidadBaterias: 4, costoUnitarioBateria: 750, cantidadCambios: 3, bateriasProyectadas: 12, costoEstimado: 9000,
    fechasReemplazo: ['2026-10-10T00:00:00', '2026-12-10T00:00:00'], estaVencido: true, fechaVencimientoPendiente: '2026-08-10', usaCostoGeneral: false };
test('La distribución mensual incluye una vez el vencido y reconcilia compras y total', () => {
    const r = resumirCompras([equipo], 2026, 9);
    assert.equal(r.meses[8].cambios, 1);
    assert.equal(r.meses.reduce((n,m) => n + m.cambios, 0), 3);
    assert.equal(r.meses.reduce((n,m) => n + m.baterias, 0), 12);
    assert.equal(r.meses.reduce((n,m) => n + m.costo, 0), 9000);
    assert.equal(r.tipos[0].costo, 9000);
});
test('Una misma especificación agrupa compras de varias agencias', () => {
    const r = resumirCompras([equipo, {...equipo, agencia: 'Otra'}], 2026, 9);
    assert.equal(r.tipos.length, 1);
    assert.equal(r.tipos[0].baterias, 24);
});
test('Exportación conserva ceros de inventario, escapa comillas y neutraliza fórmulas', () => {
    const csv = csvProyeccion([{...equipo, agencia: '=SUM(1)', tipoBateria: 'Modelo "A"'}], 2026);
    assert.ok(csv.includes('"\'=SUM(1)"'));
    assert.ok(csv.includes('"001"'));
    assert.ok(csv.includes('"Modelo ""A"""'));
    assert.ok(csv.includes('Específico por tipo'));
});

test('Las compras sin tipo identificado se separan por su información disponible', () => {
    const base = { ...equipo, tipoBateriaId: null, usaCostoGeneral: true };
    const r = resumirCompras([base, { ...base, tipoBateria: 'Litio' }, { ...base, agencia: 'Otra' }], 2026, 9);
    assert.equal(r.tipos.length, 2);
    assert.equal(r.tipos.find(m => m.tipo === 'Plomo').baterias, 24);
    assert.equal(r.tipos.find(m => m.tipo === 'Litio').baterias, 12);
});


test('Un mismo tipo con voltaje o capacidad diferentes no mezcla sus compras', () => {
  const r = resumirCompras([equipo, {...equipo, voltajeBateria: 24}, {...equipo, capacidadBateriaAh: 20}], 2026, 9);
  assert.equal(r.tipos.length, 3);
  assert.equal(r.tipos.reduce((n, t) => n + t.costo, 0), 27000);
  const csv = csvProyeccion([equipo], 2026, 'tipo');
  assert.ok(csv.includes('"Voltaje (V)"'));
  assert.ok(csv.includes('"Capacidad (Ah)"'));
  assert.ok(csv.includes('"tipo"'));
});

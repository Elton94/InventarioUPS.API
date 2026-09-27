import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import axios from 'axios';

const requests = [];
const api = axios.create({
  baseURL: 'http://localhost/api',
  adapter: async (config) => {
    requests.push(config);
    return { data: {}, status: 200, statusText: 'OK', headers: {}, config };
  },
});
const adapterKey = Symbol.for('ups.tests.api');
globalThis[adapterKey] = api;
async function loadService(name) {
  const source = (await readFile(new URL(`../src/services/${name}.js`, import.meta.url), 'utf8'))
    .replace('import api from "./api";', 'const api = globalThis[Symbol.for("ups.tests.api")];');
  return import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
}
const ups = await loadService('upsService');
const mantenimientos = await loadService('mantenimientoService');
const inventario = '0001/A +#&';

function lastRequest() { return requests.at(-1); }
test('editar usa Id interno y permite corregir inventario sin perder ceros ni caracteres', async () => {
  await ups.obtenerUPSPorId(7);
  assert.equal(lastRequest().url, '/UPS/7');
  await ups.actualizarUPS(7, { numeroInventario: inventario, numeroSerie: 'SERIE-1', modeloBateriaId: '5' });
  assert.equal(lastRequest().url, '/UPS/7');
  const data = JSON.parse(lastRequest().data);
  assert.equal(data.id, 7);
  assert.equal(data.modeloBateriaId, 5);
  assert.equal(data.numeroInventario, inventario);
  assert.equal('codigo' in data, false);
  await ups.cambiarEstadoUPS(7, 'En resguardo');
  assert.equal(lastRequest().url, '/UPS/7/estado');
  const url = new URL(api.getUri(lastRequest()));
  assert.equal(url.searchParams.get('estado'), 'En resguardo');
});

test('cambios de baterías usan UPSId y omiten costos, tipo e indicador opcional', async () => {
  const datos = { modeloBateriaId: "5", upsId: 7, costo: 500, costoBaterias: 1200, incluyeCambioBaterias: false, tipoMantenimientoId: 3, cantidadBateriasReemplazadas: 999 };
  await mantenimientos.crearMantenimiento(datos);
  assert.equal(JSON.parse(lastRequest().data).upsId, 7);
  assert.equal(JSON.parse(lastRequest().data).modeloBateriaId, 5);
  assert.equal('costo' in JSON.parse(lastRequest().data), false);
  assert.equal('costoBaterias' in JSON.parse(lastRequest().data), false);
  for (const campo of ['cantidadBateriasReemplazadas', 'incluyeCambioBaterias', 'tipoMantenimientoId'])
    assert.equal(campo in JSON.parse(lastRequest().data), false);
  assert.equal('numeroInventarioUPS' in JSON.parse(lastRequest().data), false);
  await mantenimientos.actualizarMantenimiento(12, datos);
  assert.equal(JSON.parse(lastRequest().data).upsId, 7);
  assert.equal(JSON.parse(lastRequest().data).modeloBateriaId, 5);
  assert.equal('costo' in JSON.parse(lastRequest().data), false);
  assert.equal('costoBaterias' in JSON.parse(lastRequest().data), false);
  for (const campo of ['cantidadBateriasReemplazadas', 'incluyeCambioBaterias', 'tipoMantenimientoId'])
    assert.equal(campo in JSON.parse(lastRequest().data), false);
  await mantenimientos.obtenerMantenimientosPorUPS(7);
  assert.equal(lastRequest().url, '/Mantenimiento/ups/7');
});

test('UPS permite enviar tipo sin modelo y el responsable se envía por ID', async () => {
  await ups.crearUPS({ tipoBateriaId: '3', modeloBateriaId: null });
  let data = JSON.parse(lastRequest().data);
  assert.equal(data.tipoBateriaId, 3);
  assert.equal(data.modeloBateriaId, null);
  await mantenimientos.crearMantenimiento({ upsId: 7, responsableServicioId: '8', tecnicoProveedor: 'Texto libre' });
  data = JSON.parse(lastRequest().data);
  assert.equal(data.responsableServicioId, 8);
  assert.equal('tecnicoProveedor' in data, false);
});

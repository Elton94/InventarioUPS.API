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
function assertQuery(name, value) {
  const url = new URL(api.getUri(lastRequest()));
  assert.equal(url.searchParams.get(name), value);
}

test('consultar, editar y cambiar estado conservan inventario con caracteres especiales', async () => {
  await ups.obtenerUPSPorInventario(inventario);
  assertQuery('numeroInventario', inventario);
  await ups.actualizarUPS(inventario, { numeroInventario: inventario, numeroSerie: 'SERIE-1' });
  assertQuery('numeroInventario', inventario);
  const data = JSON.parse(lastRequest().data);
  assert.equal(data.numeroInventario, inventario);
  assert.equal('id' in data, false);
  assert.equal('codigo' in data, false);
  await ups.cambiarEstadoUPS(inventario, 'En resguardo');
  assertQuery('numeroInventario', inventario);
  assertQuery('estado', 'En resguardo');
});

test('mantenimientos transmiten la clave de inventario como texto al crear y editar', async () => {
  const datos = { numeroInventarioUPS: inventario };
  await mantenimientos.crearMantenimiento(datos);
  assert.equal(JSON.parse(lastRequest().data).numeroInventarioUPS, inventario);
  assert.equal('upsId' in JSON.parse(lastRequest().data), false);
  await mantenimientos.actualizarMantenimiento(7, datos);
  assert.equal(JSON.parse(lastRequest().data).numeroInventarioUPS, inventario);
  await mantenimientos.obtenerMantenimientosPorUPS(inventario);
  assertQuery('numeroInventarioUPS', inventario);
});

import api from './api';
export const obtenerBaterias = async () => (await api.get('/baterias')).data;
export const obtenerCostosTipo = async anio => (await api.get(`/baterias/costos-tipo/${anio}`)).data;
export const guardarCostosTipo = async (anio, costos) => (await api.put(`/baterias/costos-tipo/${anio}`, costos)).data;
export const obtenerReemplazos = async () => (await api.get('/reemplazos')).data;

import api from './api';
export const obtenerBaterias = async () => (await api.get('/baterias')).data;
export const crearModeloBateria = async (tipoBateriaId, nombre) =>
    (await api.post('/baterias/modelos', { tipoBateriaId: Number(tipoBateriaId), nombre })).data;
export const obtenerCostosModelo = async anio => (await api.get(`/baterias/costos/${anio}`)).data;
export const guardarCostoModelo = async (anio, modeloId, costoUnitario) =>
    (await api.put(`/baterias/costos/${anio}/${modeloId}`, { costoUnitario: Number(costoUnitario) })).data;
export const obtenerReemplazos = async () => (await api.get('/reemplazos')).data;

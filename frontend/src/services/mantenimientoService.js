import api from "./api";

const API_URL = "/Mantenimiento";

const prepararDatos = (cambio) => ({
    upsId: Number(cambio.upsId),
    modeloBateriaId: cambio.modeloBateriaId ? Number(cambio.modeloBateriaId) : null,
    estadoMantenimientoId: Number(cambio.estadoMantenimientoId),
    fechaMantenimiento: cambio.fechaMantenimiento,
    descripcion: cambio.descripcion?.trim() || "",
    responsableServicioId: cambio.responsableServicioId ? Number(cambio.responsableServicioId) : null,
    observaciones: cambio.observaciones?.trim() || ""
});

export const obtenerMantenimientos = async () => (await api.get(API_URL)).data;
export const obtenerMantenimientoPorId = async (id) => (await api.get(`${API_URL}/${id}`)).data;
export const obtenerMantenimientosPorUPS = async (upsId) => (await api.get(`${API_URL}/ups/${upsId}`)).data;
export const crearMantenimiento = async (cambio) => (await api.post(API_URL, prepararDatos(cambio))).data;
export const actualizarMantenimiento = async (id, cambio) =>
    (await api.put(`${API_URL}/${id}`, { ...prepararDatos(cambio), id: Number(id) })).data;

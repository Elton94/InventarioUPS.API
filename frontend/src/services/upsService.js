import api from "./api";

const API_URL = "/UPS";

export const obtenerUPS = async () =>
    (await api.get(API_URL)).data;

export const obtenerUPSPorId =
    async (id) =>
        (
            await api.get(
                `${API_URL}/${id}`
            )
        ).data;

const mapear = (ups, id = 0) => ({
    id: Number(id || ups.id || 0),
    voltajeBateria: ups.voltajeBateria === "" || ups.voltajeBateria == null ? null : Number(ups.voltajeBateria),
    capacidadBateriaAh: ups.capacidadBateriaAh === "" || ups.capacidadBateriaAh == null ? null : Number(ups.capacidadBateriaAh),
    tipoBateriaId: ups.tipoBateriaId ? Number(ups.tipoBateriaId) : null,
    numeroInventario:
        ups.numeroInventario?.trim(),
    marca: ups.marca?.trim(),
    modelo: ups.modelo?.trim(),
    numeroSerie:
        ups.numeroSerie?.trim(),
    capacidadVA:
        Number(ups.capacidadVA),
    agenciaId:
        Number(ups.agenciaId),
    estadoUPSId:
        Number(ups.estadoUPSId),
    fechaInstalacion:
        ups.fechaInstalacion,
    cantidadBaterias:
        Number(ups.cantidadBaterias),
    fechaUltimoCambioBaterias:
        ups.fechaUltimoCambioBaterias ||
        null
});

export const crearUPS = async (ups) =>
    (
        await api.post(
            API_URL,
            mapear(ups)
        )
    ).data;

export const actualizarUPS =
    async (id, ups) =>
        (
            await api.put(
                `${API_URL}/${id}`,
                mapear(ups, id)
            )
        ).data;

export const cambiarEstadoUPS =
    async (id, estado) =>
        (
            await api.patch(
                `${API_URL}/${id}/estado`,
                null,
                {
                    params: {
                        estado
                    }
                }
            )
        ).data;

import api from "./api";

const API_URL = "/UPS";

export const obtenerUPS = async () => {
    const response = await api.get(API_URL);

    return response.data;
};

export const obtenerUPSPorId = async (id) => {
    const response = await api.get(
        `${API_URL}/${id}`
    );

    return response.data;
};

export const crearUPS = async (ups) => {
    const datos = {
        codigo: ups.codigo?.trim(),
        marca: ups.marca?.trim(),
        modelo: ups.modelo?.trim(),
        numeroSerie: ups.numeroSerie?.trim(),
        capacidadVA: Number(ups.capacidadVA),
        agenciaId: Number(ups.agenciaId),
        estadoUPSId: Number(ups.estadoUPSId),
        fechaInstalacion: ups.fechaInstalacion,
        cantidadBaterias: Number(
            ups.cantidadBaterias || 0
        ),
        fechaUltimoCambioBaterias:
            ups.fechaUltimoCambioBaterias || null
    };

    const response = await api.post(
        API_URL,
        datos
    );

    return response.data;
};

export const actualizarUPS = async (
    id,
    ups
) => {
    const datos = {
        id: Number(id),
        codigo: ups.codigo?.trim(),
        marca: ups.marca?.trim(),
        modelo: ups.modelo?.trim(),
        numeroSerie: ups.numeroSerie?.trim(),
        capacidadVA: Number(ups.capacidadVA),
        agenciaId: Number(ups.agenciaId),
        estadoUPSId: Number(ups.estadoUPSId),
        fechaInstalacion: ups.fechaInstalacion,
        cantidadBaterias: Number(
            ups.cantidadBaterias || 0
        ),
        fechaUltimoCambioBaterias:
            ups.fechaUltimoCambioBaterias || null
    };

    const response = await api.put(
        `${API_URL}/${id}`,
        datos
    );

    return response.data;
};

export const cambiarEstadoUPS = async (
    id,
    estado
) => {
    const response = await api.patch(
        `${API_URL}/${id}/estado`,
        null,
        {
            params: {
                estado
            }
        }
    );

    return response.data;
};
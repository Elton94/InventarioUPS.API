import api from "./api";

const API_URL = "/Agencias";

export const obtenerAgencias = async () => {
    const response = await api.get(API_URL);

    return response.data;
};

export const obtenerAgenciaPorId = async (id) => {
    const response = await api.get(`${API_URL}/${id}`);

    return response.data;
};

export const crearAgencia = async (agencia) => {
    const datos = {
        codigo: agencia.codigo?.trim(),
        nombre: agencia.nombre?.trim(),
        ciudadId: Number(
            agencia.ciudadId ?? agencia.ciudad
        ),
        direccion: agencia.direccion?.trim() || "",
        tipoAgenciaId: Number(
            agencia.tipoAgenciaId ?? agencia.tipoAgencia
        ),
        activa: true
    };

    const response = await api.post(
        API_URL,
        datos
    );

    return response.data;
};

export const actualizarAgencia = async (
    id,
    agencia
) => {
    const datos = {
        id: Number(id),
        codigo: agencia.codigo?.trim(),
        nombre: agencia.nombre?.trim(),
        ciudadId: Number(
            agencia.ciudadId ?? agencia.ciudad
        ),
        direccion: agencia.direccion?.trim() || "",
        tipoAgenciaId: Number(
            agencia.tipoAgenciaId ?? agencia.tipoAgencia
        ),
        activa:
            agencia.activa !== undefined
                ? agencia.activa
                : true
    };

    const response = await api.put(
        `${API_URL}/${id}`,
        datos
    );

    return response.data;
};

export const desactivarAgencia = async (id) => {
    const response = await api.delete(
        `${API_URL}/${id}`
    );

    return response.data;
};
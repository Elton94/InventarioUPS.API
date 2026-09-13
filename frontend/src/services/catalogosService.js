import api from "./api";

export const obtenerCatalogos = async () => {
    const response = await api.get(
        "/Catalogos/todos"
    );

    return response.data;
};

export const obtenerDepartamentos = async () => {
    const response = await api.get(
        "/Catalogos/departamentos"
    );

    return response.data;
};

export const obtenerCiudades = async (
    departamentoId
) => {
    const response = await api.get(
        "/Catalogos/ciudades",
        {
            params: {
                departamentoId
            }
        }
    );

    return response.data;
};

export const obtenerTiposAgencia = async () => {
    const response = await api.get(
        "/Catalogos/tipos-agencia"
    );

    return response.data;
};

export const obtenerEstadosUPS = async () => {
    const response = await api.get(
        "/Catalogos/estados-ups"
    );

    return response.data;
};

export const obtenerTiposMantenimiento =
    async () => {
        const response = await api.get(
            "/Catalogos/tipos-mantenimiento"
        );

        return response.data;
    };

export const obtenerEstadosMantenimiento =
    async () => {
        const response = await api.get(
            "/Catalogos/estados-mantenimiento"
        );

        return response.data;
    };
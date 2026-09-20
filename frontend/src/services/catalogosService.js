import api from "./api";

export const obtenerCatalogos = async () =>
    (await api.get("/Catalogos/todos"))
        .data;

export const obtenerDepartamentos = async () =>
    (
        await api.get(
            "/Catalogos/departamentos"
        )
    ).data;

export const obtenerCiudades = async (
    departamentoId
) =>
    (
        await api.get(
            "/Catalogos/ciudades",
            {
                params: {
                    departamentoId
                }
            }
        )
    ).data;

export const obtenerTiposAgencia =
    async () =>
        (
            await api.get(
                "/Catalogos/tipos-agencia"
            )
        ).data;

export const obtenerEstadosUPS =
    async () =>
        (
            await api.get(
                "/Catalogos/estados-ups"
            )
        ).data;

export const obtenerTiposMantenimiento =
    async () =>
        (
            await api.get(
                "/Catalogos/tipos-mantenimiento"
            )
        ).data;

export const obtenerEstadosMantenimiento =
    async () =>
        (
            await api.get(
                "/Catalogos/estados-mantenimiento"
            )
        ).data;

export const obtenerMarcasUPS =
    async () =>
        (
            await api.get(
                "/Catalogos/marcas-ups"
            )
        ).data;

export const obtenerModelosUPS =
    async (marca = "") =>
        (
            await api.get(
                "/Catalogos/modelos-ups",
                {
                    params: { marca }
                }
            )
        ).data;

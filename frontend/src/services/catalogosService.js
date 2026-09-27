import api from "./api";

export const obtenerCatalogos = async () =>
    (await api.get("/Catalogos/todos"))
        .data;

export const obtenerDepartamentos = async (actualId) =>
    (
        await api.get(
            "/Catalogos/departamentos", { params: { actualId } }
        )
    ).data;

export const obtenerCiudades = async (
    departamentoId, actualId
) =>
    (
        await api.get(
            "/Catalogos/ciudades",
            {
                params: {
                    departamentoId, actualId
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

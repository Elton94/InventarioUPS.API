import api from "./api";

const API_URL = "/UPS";

export const obtenerUPS = async () =>
    (await api.get(API_URL)).data;

export const obtenerUPSPorInventario =
    async (numeroInventario) =>
        (
            await api.get(
                `${API_URL}/detalle`,
                { params: { numeroInventario } }
            )
        ).data;

const mapear = (ups) => ({
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
    async (numeroInventario, ups) =>
        (
            await api.put(
                API_URL,
                mapear(ups),
                { params: { numeroInventario } }
            )
        ).data;

export const cambiarEstadoUPS =
    async (numeroInventario, estado) =>
        (
            await api.patch(
                `${API_URL}/estado`,
                null,
                {
                    params: {
                        numeroInventario,
                        estado
                    }
                }
            )
        ).data;

import api from "./api";

const API_URL = "/Mantenimiento";

export const obtenerMantenimientos =
    async () => {

        const response =
            await api.get(
                API_URL
            );

        return response.data;
    };

export const obtenerMantenimientoPorId =
    async (id) => {

        const response =
            await api.get(
                `${API_URL}/${id}`
            );

        return response.data;
    };

export const obtenerMantenimientosPorUPS =
    async (upsId) => {

        const response =
            await api.get(
                `${API_URL}/ups/${upsId}`
            );

        return response.data;
    };

export const crearMantenimiento =
    async (mantenimiento) => {

        const datos = {

            upsId:
                Number(
                    mantenimiento.upsId
                ),

            tipoMantenimientoId:
                Number(
                    mantenimiento
                        .tipoMantenimientoId
                ),

            estadoMantenimientoId:
                Number(
                    mantenimiento
                        .estadoMantenimientoId
                ),

            fechaMantenimiento:
                mantenimiento
                    .fechaMantenimiento,

            descripcion:
                mantenimiento
                    .descripcion
                    ?.trim() ||
                "",

            tecnicoProveedor:
                mantenimiento
                    .tecnicoProveedor
                    ?.trim() ||
                "",

            costo:
                Number(
                    mantenimiento.costo ||
                    0
                ),

            observaciones:
                mantenimiento
                    .observaciones
                    ?.trim() ||
                "",

            // CAMBIO DE BATERÍAS
            incluyeCambioBaterias:
                mantenimiento
                    .incluyeCambioBaterias ===
                true,

            cantidadBateriasReemplazadas:
                mantenimiento
                    .incluyeCambioBaterias
                    ? Number(
                        mantenimiento
                            .cantidadBateriasReemplazadas ||
                        0
                    )
                    : 0,

            costoBaterias:
                mantenimiento
                    .incluyeCambioBaterias
                    ? Number(
                        mantenimiento
                            .costoBaterias ||
                        0
                    )
                    : 0

        };

        const response =
            await api.post(
                API_URL,
                datos
            );

        return response.data;
    };

export const actualizarMantenimiento =
    async (
        id,
        mantenimiento
    ) => {

        const datos = {

            id:
                Number(id),

            upsId:
                Number(
                    mantenimiento.upsId
                ),

            tipoMantenimientoId:
                Number(
                    mantenimiento
                        .tipoMantenimientoId
                ),

            estadoMantenimientoId:
                Number(
                    mantenimiento
                        .estadoMantenimientoId
                ),

            fechaMantenimiento:
                mantenimiento
                    .fechaMantenimiento,

            descripcion:
                mantenimiento
                    .descripcion
                    ?.trim() ||
                "",

            tecnicoProveedor:
                mantenimiento
                    .tecnicoProveedor
                    ?.trim() ||
                "",

            costo:
                Number(
                    mantenimiento.costo ||
                    0
                ),

            observaciones:
                mantenimiento
                    .observaciones
                    ?.trim() ||
                "",

            // CAMBIO DE BATERÍAS
            incluyeCambioBaterias:
                mantenimiento
                    .incluyeCambioBaterias ===
                true,

            cantidadBateriasReemplazadas:
                mantenimiento
                    .incluyeCambioBaterias
                    ? Number(
                        mantenimiento
                            .cantidadBateriasReemplazadas ||
                        0
                    )
                    : 0,

            costoBaterias:
                mantenimiento
                    .incluyeCambioBaterias
                    ? Number(
                        mantenimiento
                            .costoBaterias ||
                        0
                    )
                    : 0

        };

        const response =
            await api.put(
                `${API_URL}/${id}`,
                datos
            );

        return response.data;
    };

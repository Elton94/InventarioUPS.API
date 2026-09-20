import api from "./api";

const API_URL = "/Agencias";

export const obtenerAgencias = async () =>
    (await api.get(API_URL)).data;

export const obtenerAgenciaPorId = async (
    id
) =>
    (
        await api.get(
            `${API_URL}/${id}`
        )
    ).data;

export const crearAgencia = async (
    agencia
) =>
    (
        await api.post(
            API_URL,
            {
                codigo:
                    agencia.codigo?.trim(),
                nombre:
                    agencia.nombre?.trim(),
                ciudadId:
                    Number(
                        agencia.ciudadId
                    ),
                direccion:
                    agencia.direccion?.trim(),
                tipoAgenciaId:
                    Number(
                        agencia
                            .tipoAgenciaId ||
                        0
                    ),
                activa: true
            }
        )
    ).data;

export const actualizarAgencia = async (
    id,
    agencia
) =>
    (
        await api.put(
            `${API_URL}/${id}`,
            {
                id: Number(id),
                codigo:
                    agencia.codigo?.trim(),
                nombre:
                    agencia.nombre?.trim(),
                ciudadId:
                    Number(
                        agencia.ciudadId
                    ),
                direccion:
                    agencia.direccion?.trim(),
                tipoAgenciaId:
                    Number(
                        agencia
                            .tipoAgenciaId ||
                        0
                    ),
                activa:
                    agencia.activa !==
                    false
            }
        )
    ).data;

export const cambiarEstadoAgencia =
    async (id, activa) =>
        (
            await api.patch(
                `${API_URL}/${id}/estado`,
                null,
                { params: { activa } }
            )
        ).data;

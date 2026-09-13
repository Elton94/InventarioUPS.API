import api from "./api";


// =====================================================
// CONFIGURACIÓN ANUAL DE PRESUPUESTO
// =====================================================

export const obtenerConfiguracionesPresupuesto =
    async () => {

        const response =
            await api.get(
                "/Presupuesto/configuracion"
            );

        return response.data;
    };


export const obtenerConfiguracionPresupuesto =
    async (anio) => {

        const response =
            await api.get(
                `/Presupuesto/configuracion/${anio}`
            );

        return response.data;
    };


export const crearConfiguracionPresupuesto =
    async (configuracion) => {

        const response =
            await api.post(
                "/Presupuesto/configuracion",
                configuracion
            );

        return response.data;
    };


export const actualizarConfiguracionPresupuesto =
    async (
        id,
        configuracion
    ) => {

        const response =
            await api.put(
                `/Presupuesto/configuracion/${id}`,
                configuracion
            );

        return response.data;
    };


// =====================================================
// POLÍTICA GLOBAL DE REEMPLAZO
// =====================================================

export const obtenerPoliticaReemplazo =
    async () => {

        const response =
            await api.get(
                "/Presupuesto/politica"
            );

        return response.data;
    };


export const actualizarPoliticaReemplazo =
    async (politica) => {

        const response =
            await api.put(
                "/Presupuesto/politica",
                {
                    mesesReemplazoLocal:
                        Number(
                            politica
                                .mesesReemplazoLocal
                        ),

                    mesesReemplazoForanea:
                        Number(
                            politica
                                .mesesReemplazoForanea
                        )
                }
            );

        return response.data;
    };


// =====================================================
// PROYECCIÓN
// =====================================================

export const obtenerProyeccionPresupuesto =
    async (anio) => {

        const response =
            await api.get(
                `/Presupuesto/proyeccion/${anio}`
            );

        return response.data;
    };
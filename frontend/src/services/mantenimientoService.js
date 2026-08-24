import axios from "axios";

const API_URL =
    "https://localhost:7295/api/Mantenimiento";


export const obtenerMantenimientos = async () => {

    const response = await axios.get(
        API_URL
    );

    return response.data;

};


export const obtenerMantenimientoPorId = async (id) => {

    const response = await axios.get(
        `${API_URL}/${id}`
    );

    return response.data;

};


export const obtenerMantenimientosPorUPS = async (upsId) => {

    const response = await axios.get(
        `${API_URL}/ups/${upsId}`
    );

    return response.data;

};


export const crearMantenimiento = async (
    mantenimiento
) => {

    const response = await axios.post(
        API_URL,
        mantenimiento
    );

    return response.data;

};


export const actualizarMantenimiento = async (
    id,
    datos
) => {

    const response = await fetch(
        `https://localhost:7295/api/Mantenimiento/${id}`,
        {
            method: "PUT",
            headers: {
                "Content-Type":
                    "application/json"
            },
            body: JSON.stringify(datos)
        }
    );

    if (!response.ok) {

        throw new Error(
            "Error al actualizar mantenimiento"
        );

    }

    return response;
};
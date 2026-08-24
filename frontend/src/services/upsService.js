import axios from "axios";

const API_URL = "https://localhost:7295/api/UPS";

export const obtenerUPS = async () => {

    const response = await axios.get(API_URL);

    return response.data;

};

export const crearUPS = async (ups) => {

    const response = await axios.post(
        API_URL,
        ups
    );

    return response.data;

};

export const actualizarUPS = async (id, ups) => {

    const response = await axios.put(
        `${API_URL}/${id}`,
        ups
    );

    return response.data;

};

export const cambiarEstadoUPS = async (id, estado) => {

    const response = await axios.patch(
        `${API_URL}/${id}/estado`,
        null,
        {
            params: {
                estado: estado
            }
        }
    );

    return response.data;

};
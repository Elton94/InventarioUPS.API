import axios from "axios";

const API_URL = "https://localhost:7295/api/Agencias";

export const obtenerAgencias = async () => {

    const response = await axios.get(API_URL);

    return response.data;

};

export const crearAgencia = async (agencia) => {

    const response = await axios.post(API_URL, agencia);

    return response.data;

};

export const actualizarAgencia = async (id, agencia) => {

    const response = await axios.put(
        `${API_URL}/${id}`,
        {
            id: id,
            codigo: agencia.codigo,
            nombre: agencia.nombre,
            departamento: agencia.departamento,
            ciudad: agencia.ciudad,
            direccion: agencia.direccion,
            tipoAgencia: agencia.tipoAgencia,
            activa: agencia.activa
        }
    );

    return response.data;

};
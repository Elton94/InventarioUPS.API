import api from "./api";

const API_URL = "/Usuarios";

// USUARIOS
export const obtenerUsuarios = async () => {
    const response = await api.get(API_URL);

    return response.data;
};

export const crearUsuario = async (usuario) => {
    const response = await api.post(
        API_URL,
        usuario
    );

    return response.data;
};

export const actualizarUsuario = async (
    id,
    usuario
) => {
    const response = await api.put(
        `${API_URL}/${id}`,
        usuario
    );

    return response.data;
};

export const cambiarEstadoUsuario = async (
    id,
    activo
) => {
    const response = await api.patch(
        `${API_URL}/${id}/estado`,
        null,
        {
            params: {
                activo
            }
        }
    );

    return response.data;
};

// ROLES
export const obtenerRoles = async () => {
    const response = await api.get(
        `${API_URL}/roles`
    );

    return response.data;
};

export const obtenerRolPorId = async (
    id
) => {
    const response = await api.get(
        `${API_URL}/roles/${id}`
    );

    return response.data;
};

export const crearRol = async (
    rol
) => {
    const response = await api.post(
        `${API_URL}/roles`,
        rol
    );

    return response.data;
};

export const actualizarRol = async (
    id,
    rol
) => {
    const response = await api.put(
        `${API_URL}/roles/${id}`,
        rol
    );

    return response.data;
};

export const cambiarEstadoRol = async (
    id,
    activo
) => {
    const response = await api.patch(
        `${API_URL}/roles/${id}/estado`,
        null,
        {
            params: {
                activo
            }
        }
    );

    return response.data;
};

// PERMISOS
export const obtenerPermisos = async () => {
    const response = await api.get(
        `${API_URL}/permisos`
    );

    return response.data;
};

export const obtenerPermisosRol = async (
    rolId
) => {
    const response = await api.get(
        `${API_URL}/roles/${rolId}/permisos`
    );

    return response.data;
};

export const actualizarPermisosRol = async (
    rolId,
    permisoIds
) => {
    const response = await api.put(
        `${API_URL}/roles/${rolId}/permisos`,
        {
            permisoIds
        }
    );

    return response.data;
};

// RESPONSABLES DE MANTENIMIENTO
export const obtenerResponsablesMantenimiento =
    async () => {
        const response = await api.get(
            `${API_URL}/responsables-mantenimiento`
        );

        return response.data;
    };

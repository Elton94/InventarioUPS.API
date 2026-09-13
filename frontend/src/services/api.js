import axios from "axios";

const api = axios.create({
    baseURL: "https://localhost:7295/api"
});

// Agregar automáticamente el JWT
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("token");

        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }

        return config;
    },
    (error) => Promise.reject(error)
);

// Si el token expira, cerrar sesión
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem("token");
            localStorage.removeItem("rol");
            localStorage.removeItem("usuarioId");
            localStorage.removeItem("nombreUsuario");
            localStorage.removeItem("nombreCompleto");

            window.location.href = "/login";
        }

        return Promise.reject(error);
    }
);

export default api;
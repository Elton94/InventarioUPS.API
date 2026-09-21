import axios from "axios";

const api = axios.create({
    baseURL:
        import.meta.env.DEV
            ? "/api"
            : (import.meta.env.VITE_API_URL || "/api")
});

export const limpiarSesion = () => {
    [
        "token",
        "rol",
        "usuarioId",
        "nombreUsuario",
        "nombreCompleto",
        "permisos"
    ].forEach((clave) =>
        localStorage.removeItem(clave)
    );
};

api.interceptors.request.use(
    (config) => {
        const token =
            localStorage.getItem("token");

        if (token) {
            config.headers.Authorization =
                `Bearer ${token}`;
        }

        return config;
    },
    (error) => Promise.reject(error)
);

api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            limpiarSesion();

            if (
                window.location.pathname !==
                "/login"
            ) {
                window.location.href =
                    "/login";
            }
        }

        return Promise.reject(error);
    }
);

export default api;

import {
    Navigate,
    Outlet,
    useLocation
} from "react-router-dom";

import {
    tienePermiso,
    tieneAlgunPermiso,
    tieneTodosLosPermisos,
    obtenerRutaInicial
} from "../../services/permisos";

export default function ProtectedRoute({
    permiso,
    permisos,
    modo = "alguno"
}) {
    const location = useLocation();
    const token =
        localStorage.getItem("token");

    if (!token) {
        return (
            <Navigate
                to="/login"
                replace
                state={{ from: location }}
            />
        );
    }

    let autorizado = true;

    if (permiso) {
        autorizado =
            tienePermiso(permiso);
    }

    if (
        Array.isArray(permisos) &&
        permisos.length > 0
    ) {
        autorizado =
            modo === "todos"
                ? tieneTodosLosPermisos(
                    permisos
                )
                : tieneAlgunPermiso(
                    permisos
                );
    }

    if (!autorizado) {
        return (
            <Navigate
                to={obtenerRutaInicial()}
                replace
            />
        );
    }

    return <Outlet />;
}

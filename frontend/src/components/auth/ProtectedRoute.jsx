import {
    Navigate,
    Outlet,
    useLocation
} from "react-router-dom";

import {
    tienePermiso
} from "../../services/permisos";


export default function ProtectedRoute({
    permiso
}) {

    const location =
        useLocation();

    const token =
        localStorage.getItem("token");


    // =====================================================
    // SIN SESIÓN
    // =====================================================

    if (!token) {

        return (
            <Navigate
                to="/login"
                replace
                state={{
                    from: location
                }}
            />
        );

    }


    // =====================================================
    // RUTA SIN PERMISO ESPECÍFICO
    // =====================================================

    if (!permiso) {

        return <Outlet />;

    }


    // =====================================================
    // VALIDAR PERMISO
    // =====================================================

    if (!tienePermiso(permiso)) {

        return (
            <Navigate
                to="/inicio"
                replace
            />
        );

    }


    return <Outlet />;
}
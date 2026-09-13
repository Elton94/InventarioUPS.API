import { Navigate, Outlet } from "react-router-dom";

export default function RoleRoute({
    allowedRoles = []
}) {
    const token = localStorage.getItem("token");
    const rol = localStorage.getItem("rol");

    if (!token) {
        return (
            <Navigate
                to="/login"
                replace
            />
        );
    }

    if (!allowedRoles.includes(rol)) {
        return (
            <Navigate
                to="/inicio"
                replace
            />
        );
    }

    return <Outlet />;
}
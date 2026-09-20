import {
    Outlet,
    NavLink,
    useNavigate
} from "react-router-dom";

import "./MainLayout.css";

import {
    tienePermiso,
    tieneAlgunPermiso
} from "../services/permisos";

import {
    limpiarSesion
} from "../services/api";

export default function MainLayout() {
    const navigate = useNavigate();

    const rol =
        localStorage.getItem("rol");

    const nombreCompleto =
        localStorage.getItem(
            "nombreCompleto"
        ) || "Usuario";

    const cerrarSesion = () => {
        limpiarSesion();

        navigate(
            "/login",
            { replace: true }
        );
    };

    const opciones = [
        {
            ruta: "/inicio",
            texto: "Inicio",
            visible:
                tienePermiso("VER_INICIO")
        },
        {
            ruta: "/agencias",
            texto: "Agencias",
            visible:
                tienePermiso("VER_AGENCIAS")
        },
        {
            ruta: "/ups",
            texto: "UPS",
            visible:
                tienePermiso("VER_UPS")
        },
        {
            ruta: "/mantenimientos",
            texto: "Mantenimiento",
            visible:
                tienePermiso(
                    "VER_MANTENIMIENTO"
                )
        },
        {
            ruta: "/presupuesto",
            texto: "Presupuesto",
            visible:
                tienePermiso(
                    "VER_PRESUPUESTO"
                )
        },
        {
            ruta: "/usuarios",
            texto: "Usuarios y Roles",
            visible:
                tieneAlgunPermiso([
                    "GESTIONAR_USUARIOS",
                    "GESTIONAR_ROLES_PERMISOS"
                ])
        }
    ];

    return (
        <div className="app-container">
            <header className="topbar">
                <div className="logo">
                    <strong>
                        UPS Asset Manager
                    </strong>
                    <span>
                        Gestión Corporativa
                    </span>
                </div>

                <nav className="main-nav">
                    {opciones
                        .filter(
                            opcion =>
                                opcion.visible
                        )
                        .map(opcion => (
                            <NavLink
                                key={
                                    opcion.ruta
                                }
                                to={
                                    opcion.ruta
                                }
                            >
                                {
                                    opcion.texto
                                }
                            </NavLink>
                        ))}
                </nav>

                <div className="user-section">
                    <div className="user-info">
                        <span className="user-name">
                            {nombreCompleto}
                        </span>
                        <span className="user-role">
                            {rol || "Usuario"}
                        </span>
                    </div>

                    <button
                        type="button"
                        className="logout-button"
                        onClick={
                            cerrarSesion
                        }
                    >
                        Cerrar sesión
                    </button>
                </div>
            </header>

            <main className="content">
                <Outlet />
            </main>
        </div>
    );
}

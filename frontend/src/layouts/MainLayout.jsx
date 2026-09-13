import { Outlet, NavLink, useNavigate } from "react-router-dom";
import "./MainLayout.css";

export default function MainLayout() {
    const navigate = useNavigate();

    const token =
        localStorage.getItem("token");

    const rol =
        localStorage.getItem("rol");

    const nombreCompleto =
        localStorage.getItem("nombreCompleto") ||
        "Usuario";

    const esControlTotal =
        Boolean(token) &&
        rol === "Control Total";


    const cerrarSesion = () => {

        localStorage.removeItem("token");
        localStorage.removeItem("rol");
        localStorage.removeItem("usuarioId");
        localStorage.removeItem("nombreUsuario");
        localStorage.removeItem("nombreCompleto");

        navigate(
            "/login",
            {
                replace: true
            }
        );
    };


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

                    <NavLink to="/inicio">
                        Inicio
                    </NavLink>

                    <NavLink to="/agencias">
                        Agencias
                    </NavLink>

                    <NavLink to="/ups">
                        UPS
                    </NavLink>

                    <NavLink to="/baterias">
                        Baterías
                    </NavLink>

                    <NavLink to="/mantenimientos">
                        Mantenimiento
                    </NavLink>

                    <NavLink to="/presupuesto">
                        Presupuesto
                    </NavLink>

                    {esControlTotal && (

                        <NavLink to="/usuarios">
                            Usuarios
                        </NavLink>

                    )}

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
                        onClick={cerrarSesion}
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
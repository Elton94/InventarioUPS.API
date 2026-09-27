import {
    BrowserRouter,
    Routes,
    Route,
    Navigate
} from "react-router-dom";

import MainLayout
    from "./layouts/MainLayout";

import Login from "./pages/Login";
import Home from "./pages/Home";
import Agencias from "./pages/Agencias";
import UPS from "./pages/UPS";
import Mantenimientos
    from "./pages/Mantenimientos";
import Presupuesto
    from "./pages/Presupuesto";
import Configuracion from "./pages/Configuracion";
import Usuarios from "./pages/Usuarios";

import ProtectedRoute
    from "./components/auth/ProtectedRoute";

import {
    obtenerRutaInicial
} from "./services/permisos";

function RutaInicial() {
    return (
        <Navigate
            to={obtenerRutaInicial()}
            replace
        />
    );
}

function SinPermisos() {
    return (
        <div
            style={{
                padding: 32,
                textAlign: "center"
            }}
        >
            <h2>Sin permisos asignados</h2>
            <p>
                Su usuario está activo, pero no tiene
                permisos para utilizar módulos del sistema.
                Contacte al administrador.
            </p>
        </div>
    );
}

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route
                    path="/login"
                    element={<Login />}
                />

                <Route
                    element={<ProtectedRoute />}
                >
                    <Route
                        element={<MainLayout />}
                    >
                        <Route
                            path="/"
                            element={<RutaInicial />}
                        />

                        <Route
                            element={
                                <ProtectedRoute
                                    permiso="VER_INICIO"
                                />
                            }
                        >
                            <Route
                                path="/inicio"
                                element={<Home />}
                            />
                        </Route>

                        <Route
                            element={
                                <ProtectedRoute
                                    permiso="VER_AGENCIAS"
                                />
                            }
                        >
                            <Route
                                path="/agencias"
                                element={<Agencias />}
                            />
                        </Route>

                        <Route
                            element={
                                <ProtectedRoute
                                    permiso="VER_UPS"
                                />
                            }
                        >
                            <Route
                                path="/ups"
                                element={<UPS />}
                            />
                        </Route>

                        <Route
                            element={
                                <ProtectedRoute
                                    permiso="VER_MANTENIMIENTO"
                                />
                            }
                        >
                            <Route
                                path="/mantenimientos"
                                element={
                                    <Mantenimientos />
                                }
                            />
                        </Route>

                        <Route
                            element={
                                <ProtectedRoute
                                    permiso="VER_PRESUPUESTO"
                                />
                            }
                        >
                            <Route
                                path="/presupuesto"
                                element={
                                    <Presupuesto />
                                }
                            />
                        </Route>

                        <Route
                            element={
                                <ProtectedRoute
                                    permisos={[
                                        "GESTIONAR_USUARIOS",
                                        "GESTIONAR_ROLES_PERMISOS"
                                    ]}
                                />
                            }
                        >
                            <Route
                                path="/usuarios"
                                element={<Usuarios />}
                            />
                        </Route>

                        <Route element={<ProtectedRoute permiso="GESTIONAR_CATALOGOS" />}><Route path="/configuracion" element={<Configuracion />} /></Route>

                        <Route
                            path="/sin-permisos"
                            element={<SinPermisos />}
                        />
                    </Route>
                </Route>

                <Route
                    path="*"
                    element={<RutaInicial />}
                />
            </Routes>
        </BrowserRouter>
    );
}

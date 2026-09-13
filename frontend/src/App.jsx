import {
    BrowserRouter,
    Routes,
    Route,
    Navigate
} from "react-router-dom";

import MainLayout from "./layouts/MainLayout";

import Login from "./pages/Login";
import Home from "./pages/Home";
import Agencias from "./pages/Agencias";
import UPS from "./pages/UPS";
import Baterias from "./pages/Baterias";
import Mantenimientos from "./pages/Mantenimientos";
import Presupuesto from "./pages/Presupuesto";
import Usuarios from "./pages/Usuarios";

import ProtectedRoute from "./components/auth/ProtectedRoute";


function App() {

    return (

        <BrowserRouter>

            <Routes>

                {/* =====================================================
                    LOGIN
                ===================================================== */}

                <Route
                    path="/login"
                    element={<Login />}
                />


                {/* =====================================================
                    ZONA PROTEGIDA
                ===================================================== */}

                <Route
                    element={<ProtectedRoute />}
                >

                    <Route
                        element={<MainLayout />}
                    >

                        {/* -------------------------------------------------
                            INICIO
                        ------------------------------------------------- */}

                        <Route
                            path="/"
                            element={
                                <Navigate
                                    to="/inicio"
                                    replace
                                />
                            }
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


                        {/* -------------------------------------------------
                            AGENCIAS
                        ------------------------------------------------- */}

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


                        {/* -------------------------------------------------
                            UPS
                        ------------------------------------------------- */}

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

                            {/* Baterías pertenece al módulo UPS */}

                            <Route
                                path="/baterias"
                                element={<Baterias />}
                            />

                        </Route>


                        {/* -------------------------------------------------
                            MANTENIMIENTO
                        ------------------------------------------------- */}

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


                        {/* -------------------------------------------------
                            PRESUPUESTO
                        ------------------------------------------------- */}

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


                        {/* -------------------------------------------------
                            USUARIOS
                        ------------------------------------------------- */}

                        <Route
                            element={
                                <ProtectedRoute
                                    permiso="GESTIONAR_USUARIOS"
                                />
                            }
                        >

                            <Route
                                path="/usuarios"
                                element={<Usuarios />}
                            />

                        </Route>

                    </Route>

                </Route>


                {/* =====================================================
                    RUTA DESCONOCIDA
                ===================================================== */}

                <Route
                    path="*"
                    element={
                        <Navigate
                            to="/inicio"
                            replace
                        />
                    }
                />

            </Routes>

        </BrowserRouter>

    );
}


export default App;
import { Outlet, NavLink } from "react-router-dom";

import "./MainLayout.css";

export default function MainLayout() {
  return (
    <>
      <header className="topbar">

        <div className="logo">

          UPS Asset Manager

          <span>Gestión Corporativa</span>

        </div>

        <nav>

          <NavLink to="/inicio">Inicio</NavLink>

          <NavLink to="/agencias">Agencias</NavLink>

          <NavLink to="/ups">UPS</NavLink>

          <NavLink to="/baterias">Baterías</NavLink>

          <NavLink to="/mantenimientos">Mantenimiento</NavLink>

          <NavLink to="/presupuesto">Presupuesto</NavLink>

          <NavLink to="/reportes">Reportes</NavLink>

        </nav>

        <div className="user">

          Elton

        </div>

      </header>

      <main className="content">

        <Outlet />

      </main>

    </>
  );
}
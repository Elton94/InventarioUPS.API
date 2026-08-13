import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import MainLayout from "./layouts/MainLayout";

import Home from "./pages/Home";
import Agencias from "./pages/Agencias";
import UPS from "./pages/UPS";
import Baterias from "./pages/Baterias";
import Mantenimientos from "./pages/Mantenimientos";
import Presupuesto from "./pages/Presupuesto";
import Reportes from "./pages/Reportes";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<MainLayout />}>
          <Route path="/" element={<Navigate to="/inicio" />} />
          <Route path="/inicio" element={<Home />} />
          <Route path="/agencias" element={<Agencias />} />
          <Route path="/ups" element={<UPS />} />
          <Route path="/baterias" element={<Baterias />} />
          <Route path="/mantenimientos" element={<Mantenimientos />} />
          <Route path="/presupuesto" element={<Presupuesto />} />
          <Route path="/reportes" element={<Reportes />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
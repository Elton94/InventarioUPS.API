import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";

import { obtenerAgencias } from "../services/agenciaService";
import { obtenerUPS } from "../services/upsService";
import { obtenerMantenimientos } from "../services/mantenimientoService";
import { obtenerProyeccionPresupuesto } from "../services/presupuestoService";

import "./Home.css";


const formatoNumero = new Intl.NumberFormat("es-HN");

const formatoMoneda = new Intl.NumberFormat("es-HN", {
    style: "currency",
    currency: "HNL",
    minimumFractionDigits: 2
});


function obtenerEstadoUPS(ups) {
    return (
        ups?.estadoUPS?.nombre ||
        ups?.estadoUPSNombre ||
        ups?.estado ||
        "Sin estado"
    );
}


function obtenerCantidadBaterias(ups) {
    const cantidad = Number(
        ups?.cantidadBaterias ?? 0
    );

    return Number.isFinite(cantidad)
        ? cantidad
        : 0;
}


function obtenerCostoProyeccion(item) {
    return Number(
        item?.subtotal ??
        item?.costoTotal ??
        item?.total ??
        item?.costo ??
        0
    );
}


function obtenerBateriasProyectadas(item) {
    return Number(
        item?.cantidadBaterias ??
        item?.baterias ??
        item?.cantidad ??
        0
    );
}


function obtenerAgenciaProyeccion(item) {
    return (
        item?.agenciaNombre ||
        item?.nombreAgencia ||
        item?.agencia ||
        "—"
    );
}


function obtenerUPSProyeccion(item) {
    return (
        item?.codigoUPS ||
        item?.upsCodigo ||
        item?.codigo ||
        "—"
    );
}


function obtenerFechaProyeccion(item) {
    return (
        item?.fechaProyectada ||
        item?.fechaReemplazo ||
        item?.fecha ||
        null
    );
}


function formatearFecha(fecha) {

    if (!fecha) {
        return "—";
    }

    const fechaConvertida =
        new Date(fecha);

    if (
        Number.isNaN(
            fechaConvertida.getTime()
        )
    ) {
        return "—";
    }

    return fechaConvertida.toLocaleDateString(
        "es-HN"
    );
}


function Indicador({
    titulo,
    valor,
    descripcion,
    onClick
}) {

    return (
        <div
            className={
                onClick
                    ? "dashboard-indicator clickable"
                    : "dashboard-indicator"
            }
            onClick={onClick}
        >

            <div className="indicator-title">
                {titulo}
            </div>

            <div className="indicator-value">
                {valor}
            </div>

            <div className="indicator-description">
                {descripcion}
            </div>

        </div>
    );
}


export default function Home() {

    const navigate = useNavigate();

    const [agencias, setAgencias] =
        useState([]);

    const [ups, setUps] =
        useState([]);

    const [mantenimientos, setMantenimientos] =
        useState([]);

    const [proyeccion, setProyeccion] =
        useState([]);

    const [cargando, setCargando] =
        useState(true);

    const [error, setError] =
        useState("");


    const anioActual =
        new Date().getFullYear();


    useEffect(() => {

        const cargarDashboard =
            async () => {

                try {

                    setCargando(true);
                    setError("");

                    const resultados =
                        await Promise.allSettled([
                            obtenerAgencias(),
                            obtenerUPS(),
                            obtenerMantenimientos(),
                            obtenerProyeccionPresupuesto(
                                anioActual
                            )
                        ]);


                    const [
                        resultadoAgencias,
                        resultadoUPS,
                        resultadoMantenimientos,
                        resultadoProyeccion
                    ] = resultados;


                    if (
                        resultadoAgencias.status ===
                        "fulfilled"
                    ) {

                        setAgencias(
                            resultadoAgencias.value || []
                        );

                    }


                    if (
                        resultadoUPS.status ===
                        "fulfilled"
                    ) {

                        setUps(
                            resultadoUPS.value || []
                        );

                    }


                    if (
                        resultadoMantenimientos.status ===
                        "fulfilled"
                    ) {

                        setMantenimientos(
                            resultadoMantenimientos.value || []
                        );

                    }


                    if (
                        resultadoProyeccion.status ===
                        "fulfilled"
                    ) {

                        const datos =
                            resultadoProyeccion.value;


                        if (
                            Array.isArray(datos)
                        ) {

                            setProyeccion(
                                datos
                            );

                        } else if (
                            Array.isArray(
                                datos?.detalle
                            )
                        ) {

                            setProyeccion(
                                datos.detalle
                            );

                        } else if (
                            Array.isArray(
                                datos?.agencias
                            )
                        ) {

                            setProyeccion(
                                datos.agencias
                            );

                        } else {

                            setProyeccion([]);

                        }

                    }


                    const todosFallaron =
                        resultados.every(
                            resultado =>
                                resultado.status ===
                                "rejected"
                        );


                    if (todosFallaron) {

                        setError(
                            "No se pudo cargar la información del dashboard."
                        );

                    }

                } catch (error) {

                    console.error(
                        "Error al cargar dashboard:",
                        error
                    );

                    setError(
                        "No se pudo cargar la información del dashboard."
                    );

                } finally {

                    setCargando(false);

                }

            };


        cargarDashboard();

    }, [anioActual]);


    const agenciasActivas =
        useMemo(
            () =>
                agencias.filter(
                    agencia =>
                        agencia.activa !== false
                ),
            [agencias]
        );


    const totalBaterias =
        useMemo(
            () =>
                ups.reduce(
                    (
                        total,
                        equipo
                    ) =>
                        total +
                        obtenerCantidadBaterias(
                            equipo
                        ),
                    0
                ),
            [ups]
        );


    const upsFueraServicio =
        useMemo(
            () =>
                ups.filter(
                    equipo =>
                        obtenerEstadoUPS(
                            equipo
                        )
                            .toLowerCase()
                            .trim() ===
                        "fuera de servicio"
                ),
            [ups]
        );


    const costoMantenimiento =
        useMemo(
            () =>
                mantenimientos.reduce(
                    (
                        total,
                        mantenimiento
                    ) =>
                        total +
                        Number(
                            mantenimiento?.costo ||
                            0
                        ),
                    0
                ),
            [mantenimientos]
        );


    const totalBateriasProyectadas =
        useMemo(
            () =>
                proyeccion.reduce(
                    (
                        total,
                        item
                    ) =>
                        total +
                        obtenerBateriasProyectadas(
                            item
                        ),
                    0
                ),
            [proyeccion]
        );


    const presupuestoProyectado =
        useMemo(
            () =>
                proyeccion.reduce(
                    (
                        total,
                        item
                    ) =>
                        total +
                        obtenerCostoProyeccion(
                            item
                        ),
                    0
                ),
            [proyeccion]
        );


    const estadosUPS =
        useMemo(() => {

            const estados = {};

            ups.forEach(
                equipo => {

                    const estado =
                        obtenerEstadoUPS(
                            equipo
                        );

                    estados[estado] =
                        (
                            estados[estado] ||
                            0
                        ) + 1;

                }
            );


            return Object.entries(
                estados
            ).sort(
                (
                    [, cantidadA],
                    [, cantidadB]
                ) =>
                    cantidadB -
                    cantidadA
            );

        }, [ups]);


    const mantenimientosRecientes =
        useMemo(
            () =>
                [...mantenimientos]
                    .sort(
                        (
                            a,
                            b
                        ) =>
                            new Date(
                                b.fechaMantenimiento
                            ) -
                            new Date(
                                a.fechaMantenimiento
                            )
                    )
                    .slice(0, 5),
            [mantenimientos]
        );


    if (cargando) {

        return (
            <div className="dashboard-loading">

                <div className="dashboard-spinner" />

                <span>
                    Cargando información...
                </span>

            </div>
        );

    }


    return (

        <div className="dashboard">

            {/* ENCABEZADO */}

            <div className="dashboard-header">

                <div>

                    <h1>
                        Inicio
                    </h1>

                    <p>
                        Resumen general del inventario
                        nacional de UPS.
                    </p>

                </div>


                <div className="projection-year">

                    <span>
                        Proyección
                    </span>

                    <strong>
                        {anioActual}
                    </strong>

                </div>

            </div>


            {error && (

                <div className="dashboard-alert">
                    {error}
                </div>

            )}


            {/* INDICADORES */}

            <div className="dashboard-indicators">

                <Indicador
                    titulo="Agencias activas"
                    valor={formatoNumero.format(
                        agenciasActivas.length
                    )}
                    descripcion="Agencias registradas"
                    onClick={() =>
                        navigate("/agencias")
                    }
                />


                <Indicador
                    titulo="UPS registrados"
                    valor={formatoNumero.format(
                        ups.length
                    )}
                    descripcion="Equipos en inventario"
                    onClick={() =>
                        navigate("/ups")
                    }
                />


                <Indicador
                    titulo="Fuera de servicio"
                    valor={formatoNumero.format(
                        upsFueraServicio.length
                    )}
                    descripcion="UPS no operativos"
                    onClick={() =>
                        navigate("/ups")
                    }
                />


                <Indicador
                    titulo="Total de baterías"
                    valor={formatoNumero.format(
                        totalBaterias
                    )}
                    descripcion="Baterías instaladas"
                />


                <Indicador
                    titulo="Mantenimientos"
                    valor={formatoNumero.format(
                        mantenimientos.length
                    )}
                    descripcion="Registros de mantenimiento"
                    onClick={() =>
                        navigate(
                            "/mantenimientos"
                        )
                    }
                />


                <Indicador
                    titulo="Costo mantenimiento"
                    valor={formatoMoneda.format(
                        costoMantenimiento
                    )}
                    descripcion="Costo acumulado"
                />


                <Indicador
                    titulo="Baterías proyectadas"
                    valor={formatoNumero.format(
                        totalBateriasProyectadas
                    )}
                    descripcion={`Reemplazos ${anioActual}`}
                />


                <Indicador
                    titulo="Presupuesto proyectado"
                    valor={formatoMoneda.format(
                        presupuestoProyectado
                    )}
                    descripcion={`Presupuesto ${anioActual}`}
                    onClick={() =>
                        navigate(
                            "/presupuesto"
                        )
                    }
                />

            </div>


            {/* INFORMACIÓN OPERATIVA */}

            <div className="dashboard-columns">

                {/* UPS POR ESTADO */}

                <section className="dashboard-panel">

                    <div className="panel-header">

                        <div>

                            <h2>
                                UPS por estado
                            </h2>

                            <p>
                                Distribución actual del inventario
                            </p>

                        </div>

                    </div>


                    <div className="state-table">

                        <div className="state-header">

                            <span>
                                Estado
                            </span>

                            <span>
                                Cantidad
                            </span>

                            <span>
                                Porcentaje
                            </span>

                        </div>


                        {estadosUPS.length === 0 ? (

                            <div className="empty-row">
                                No hay UPS registrados.
                            </div>

                        ) : (

                            estadosUPS.map(
                                (
                                    [
                                        estado,
                                        cantidad
                                    ]
                                ) => {

                                    const porcentaje =
                                        ups.length
                                            ? Math.round(
                                                (
                                                    cantidad /
                                                    ups.length
                                                ) * 100
                                            )
                                            : 0;


                                    return (

                                        <div
                                            className="state-row"
                                            key={estado}
                                        >

                                            <span>
                                                {estado}
                                            </span>

                                            <strong>
                                                {cantidad}
                                            </strong>

                                            <div className="percentage">

                                                <span>
                                                    {porcentaje}%
                                                </span>

                                                <div className="progress">

                                                    <div
                                                        style={{
                                                            width:
                                                                `${porcentaje}%`
                                                        }}
                                                    />

                                                </div>

                                            </div>

                                        </div>

                                    );

                                }
                            )

                        )}

                    </div>

                </section>


                {/* MANTENIMIENTOS */}

                <section className="dashboard-panel">

                    <div className="panel-header">

                        <div>

                            <h2>
                                Mantenimientos recientes
                            </h2>

                            <p>
                                Últimos registros realizados
                            </p>

                        </div>


                        {mantenimientos.length > 0 && (

                            <button
                                className="panel-button"
                                onClick={() =>
                                    navigate(
                                        "/mantenimientos"
                                    )
                                }
                            >
                                Ver todos
                            </button>

                        )}

                    </div>


                    {mantenimientosRecientes.length === 0 ? (

                        <div className="empty-maintenance">

                            <strong>
                                No hay mantenimientos registrados.
                            </strong>

                            <span>
                                Los registros aparecerán
                                aquí cuando sean creados.
                            </span>

                        </div>

                    ) : (

                        <div className="maintenance-table">

                            <div className="maintenance-header">

                                <span>
                                    Fecha
                                </span>

                                <span>
                                    UPS
                                </span>

                                <span>
                                    Agencia
                                </span>

                                <span>
                                    Tipo
                                </span>

                                <span>
                                    Costo
                                </span>

                            </div>


                            {mantenimientosRecientes.map(
                                mantenimiento => (

                                    <div
                                        className="maintenance-row"
                                        key={
                                            mantenimiento.id
                                        }
                                    >

                                        <span>
                                            {formatearFecha(
                                                mantenimiento.fechaMantenimiento
                                            )}
                                        </span>

                                        <strong>
                                            {mantenimiento.codigoUPS ||
                                                "—"}
                                        </strong>

                                        <span>
                                            {mantenimiento.agenciaNombre ||
                                                "—"}
                                        </span>

                                        <span>
                                            {mantenimiento.tipoMantenimiento ||
                                                "—"}
                                        </span>

                                        <strong>
                                            {formatoMoneda.format(
                                                Number(
                                                    mantenimiento.costo ||
                                                    0
                                                )
                                            )}
                                        </strong>

                                    </div>

                                )
                            )}

                        </div>

                    )}

                </section>

            </div>


            {/* PROYECCIÓN */}

            <section className="dashboard-panel projection-panel">

                <div className="panel-header">

                    <div>

                        <h2>
                            Próximos reemplazos de baterías
                        </h2>

                        <p>
                            Necesidades proyectadas para{" "}
                            {anioActual}
                        </p>

                    </div>


                    {proyeccion.length > 0 && (

                        <button
                            className="panel-button"
                            onClick={() =>
                                navigate(
                                    "/presupuesto"
                                )
                            }
                        >
                            Ver presupuesto
                        </button>

                    )}

                </div>


                {proyeccion.length === 0 ? (

                    <div className="empty-projection">

                        <strong>
                            No hay reemplazos proyectados
                            para este año.
                        </strong>

                        <span>
                            Las proyecciones aparecerán
                            cuando existan UPS cuyo ciclo
                            de reemplazo corresponda a este año.
                        </span>

                    </div>

                ) : (

                    <div className="projection-table">

                        <div className="projection-header">

                            <span>
                                Agencia
                            </span>

                            <span>
                                UPS
                            </span>

                            <span>
                                Baterías
                            </span>

                            <span>
                                Fecha proyectada
                            </span>

                            <span>
                                Costo estimado
                            </span>

                        </div>


                        {proyeccion
                            .slice(0, 8)
                            .map(
                                (
                                    item,
                                    index
                                ) => (

                                    <div
                                        className="projection-row"
                                        key={
                                            item.id ??
                                            index
                                        }
                                    >

                                        <strong>
                                            {obtenerAgenciaProyeccion(
                                                item
                                            )}
                                        </strong>

                                        <span>
                                            {obtenerUPSProyeccion(
                                                item
                                            )}
                                        </span>

                                        <span>
                                            {obtenerBateriasProyectadas(
                                                item
                                            )}
                                        </span>

                                        <span>
                                            {formatearFecha(
                                                obtenerFechaProyeccion(
                                                    item
                                                )
                                            )}
                                        </span>

                                        <strong>
                                            {formatoMoneda.format(
                                                obtenerCostoProyeccion(
                                                    item
                                                )
                                            )}
                                        </strong>

                                    </div>

                                )
                            )}

                    </div>

                )}

            </section>

        </div>

    );

}
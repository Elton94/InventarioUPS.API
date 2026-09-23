import { useEffect, useMemo, useRef, useState } from "react";
import {
    Accordion, AccordionDetails, AccordionSummary, Alert, Box, Button,
    Card, CardContent, Chip, CircularProgress, Dialog, DialogActions,
    DialogContent, DialogTitle, MenuItem, Stack, Table, TableBody,
    TableCell, TableContainer, TableHead, TableRow, TextField, Typography
} from "@mui/material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import SettingsIcon from "@mui/icons-material/Settings";
import {
    obtenerConfiguracionesPresupuesto, crearConfiguracionPresupuesto,
    actualizarConfiguracionPresupuesto, obtenerPoliticaReemplazo,
    actualizarPoliticaReemplazo, obtenerProyeccionPresupuesto
} from "../services/presupuestoService";
import { tienePermiso } from "../services/permisos";

const moneda = valor => new Intl.NumberFormat("es-HN", { style: "currency", currency: "HNL" }).format(valor || 0);
const fecha = valor => valor ? new Date(valor).toLocaleDateString("es-HN") : "—";
const normalizar = valor => String(valor || "").toLocaleLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");

export default function Presupuesto() {
    const actual = new Date().getFullYear();
    const [anio, setAnio] = useState(actual);
    const [configs, setConfigs] = useState([]);
    const [politica, setPolitica] = useState(null);
    const [proyeccion, setProyeccion] = useState(null);
    const [cargando, setCargando] = useState(true);
    const [generando, setGenerando] = useState(false);
    const [guardando, setGuardando] = useState(false);
    const [error, setError] = useState("");
    const [mensaje, setMensaje] = useState("");
    const [busqueda, setBusqueda] = useState("");
    const [tipoAgencia, setTipoAgencia] = useState("Todas");
    const [dialogo, setDialogo] = useState(null);
    const [calendario, setCalendario] = useState(null);
    const [errorDialogo, setErrorDialogo] = useState("");
    const [costo, setCosto] = useState("");
    const [local, setLocal] = useState("");
    const [foranea, setForanea] = useState("");
    const solicitud = useRef(0);
    const puedeGestionar = tienePermiso("GESTIONAR_PRESUPUESTO");
    const configuracion = configs.find(c => Number(c.anio) === anio);
    const anios = [...new Set([...Array.from({ length: 6 }, (_, i) => actual + i), ...configs.map(c => Number(c.anio))])]
        .filter(a => a >= actual && a <= 2100).sort((a, b) => a - b);

    useEffect(() => {
        let vigente = true;
        Promise.all([obtenerConfiguracionesPresupuesto(), obtenerPoliticaReemplazo()])
            .then(([costos, reglas]) => { if (vigente) { setConfigs(costos || []); setPolitica(reglas); } })
            .catch(e => { if (vigente) setError(e.response?.data?.mensaje || "No se pudo cargar la configuración. Recargue la página para reintentar."); })
            .finally(() => { if (vigente) setCargando(false); });
        return () => { vigente = false; solicitud.current++; };
    }, []);

    const invalidar = () => { solicitud.current++; setProyeccion(null); setCalendario(null); setGenerando(false); };
    const generar = async () => {
        const id = ++solicitud.current;
        setError(""); setMensaje(""); setProyeccion(null);
        if (!configuracion?.activo) return setError(`Configure un costo activo para ${anio}.`);
        setGenerando(true);
        try {
            const datos = await obtenerProyeccionPresupuesto(anio);
            if (id === solicitud.current) setProyeccion(datos);
        } catch (e) {
            if (id === solicitud.current) setError(e.response?.data?.mensaje || "No se pudo generar la proyección. Intente de nuevo.");
        } finally {
            if (id === solicitud.current) setGenerando(false);
        }
    };

    const abrirCosto = () => {
        setCosto(configuracion?.costoUnitarioBateria ?? ""); setErrorDialogo("");
        setDialogo({ tipo: "costo", anio, id: configuracion?.id });
    };
    const abrirPolitica = () => {
        setLocal(politica.mesesReemplazoLocal); setForanea(politica.mesesReemplazoForanea);
        setErrorDialogo(""); setDialogo({ tipo: "politica" });
    };
    const guardar = async () => {
        setErrorDialogo("");
        const valor = Number(costo);
        if (dialogo.tipo === "costo") {
            if (!Number.isFinite(valor) || valor < 0.01 || valor >= 1e16 || !/^\d+(\.\d{1,2})?$/.test(String(costo).trim()))
                return setErrorDialogo("Ingrese un costo positivo con máximo dos decimales.");
        } else if (![Number(local), Number(foranea)].every(v => Number.isInteger(v) && v >= 1 && v <= 120)) {
            return setErrorDialogo("Use meses enteros entre 1 y 120.");
        }
        setGuardando(true);
        try {
            if (dialogo.tipo === "costo") {
                if (dialogo.id) await actualizarConfiguracionPresupuesto(dialogo.id, { costoUnitarioBateria: valor, activo: true });
                else await crearConfiguracionPresupuesto({ anio: dialogo.anio, costoUnitarioBateria: valor });
                setConfigs(await obtenerConfiguracionesPresupuesto());
            } else {
                await actualizarPoliticaReemplazo({ mesesReemplazoLocal: Number(local), mesesReemplazoForanea: Number(foranea) });
                setPolitica(await obtenerPoliticaReemplazo());
            }
            invalidar(); setError(""); setMensaje("Configuración guardada. Genere nuevamente la proyección."); setDialogo(null);
        } catch (e) {
            setErrorDialogo(e.response?.data?.mensaje || "No se pudo guardar. Verifique los datos e intente de nuevo.");
        } finally { setGuardando(false); }
    };

    const agencias = useMemo(() => (proyeccion?.agencias || []).map(a => ({
        ...a, ups: a.ups.filter(u => (tipoAgencia === "Todas" || u.tipoAgencia === tipoAgencia) &&
            normalizar([a.agencia, u.numeroInventario, u.numeroSerie].join(" ")).includes(normalizar(busqueda)))
    })).filter(a => a.ups.length), [proyeccion, busqueda, tipoAgencia]);
    const equipos = agencias.flatMap(a => a.ups);
    const filtrado = Boolean(busqueda.trim()) || tipoAgencia !== "Todas";
    const resumen = equipos.reduce((r, u) => ({ cambios: r.cambios + u.cantidadCambios,
        baterias: r.baterias + u.bateriasProyectadas, total: r.total + u.costoEstimado,
        pendiente: r.pendiente + u.costoPendiente }), { cambios: 0, baterias: 0, total: 0, pendiente: 0 });

    if (cargando) return <Box sx={{ py: 8, textAlign: "center" }}><CircularProgress aria-label="Cargando configuración" /></Box>;

    return <Box sx={{ maxWidth: 1500, mx: "auto" }}>
        <Stack direction={{ xs: "column", sm: "row" }} justifyContent="space-between" gap={2} sx={{ mb: 3 }}>
            <Box><Typography variant="h4" fontWeight={700}>Proyecciones de baterías</Typography>
                <Typography color="text.secondary">Planifique los reemplazos y las compras de cada año.</Typography></Box>
            {politica && <Stack direction="row" alignItems="center" flexWrap="wrap" gap={1}>
                <Chip label={`Local: ${politica.mesesReemplazoLocal} meses`} variant="outlined" />
                <Chip label={`Foránea: ${politica.mesesReemplazoForanea} meses`} variant="outlined" />
                {puedeGestionar && <Button startIcon={<SettingsIcon />} onClick={abrirPolitica}>Política</Button>}
            </Stack>}
        </Stack>
        {error && <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError("")}>{error}</Alert>}
        {mensaje && <Alert severity="success" sx={{ mb: 2 }} onClose={() => setMensaje("")}>{mensaje}</Alert>}
        <Card variant="outlined" sx={{ mb: 3, borderRadius: 3 }}><CardContent>
            <Stack direction={{ xs: "column", md: "row" }} alignItems={{ md: "center" }} gap={3}>
                <TextField select label="Año a proyectar" value={anio} sx={{ minWidth: 180 }} onChange={e => {
                    setAnio(Number(e.target.value)); invalidar(); setError(""); setMensaje("");
                }}>{anios.map(a => <MenuItem key={a} value={a}>{a}</MenuItem>)}</TextField>
                <Box sx={{ flex: 1 }}><Typography variant="body2" color="text.secondary">Costo estimado por batería · {anio}</Typography>
                    <Typography variant="h5" fontWeight={700}>{configuracion?.activo ? moneda(configuracion.costoUnitarioBateria) : "Sin configurar"}</Typography></Box>
                {puedeGestionar && <Button variant="outlined" onClick={abrirCosto}>{configuracion ? "Editar costo" : "Configurar costo"}</Button>}
                <Button variant="contained" size="large" disabled={generando || !configuracion?.activo || !politica} onClick={generar}>
                    {generando ? "Calculando…" : "Generar proyección"}</Button>
            </Stack>
        </CardContent></Card>
        {!configuracion?.activo && <Alert severity="info" sx={{ mb: 3 }}>Falta el costo de {anio}. {puedeGestionar ? "Use Configurar costo para continuar." : "Solicite la configuración al administrador."}</Alert>}
        {!proyeccion && !generando && configuracion?.activo && <Box sx={{ py: 5, textAlign: "center", border: "1px dashed #cbd5e1", borderRadius: 3 }}>
            <Typography variant="h6">La configuración está lista</Typography>
            <Typography color="text.secondary">Genere la proyección para ver fechas, cantidades y presupuesto por agencia.</Typography></Box>}
        {proyeccion && <>
            <Stack direction={{ xs: "column", sm: "row" }} gap={2} sx={{ mb: 2 }}>
                <TextField size="small" label="Buscar agencia, inventario o serie" value={busqueda} onChange={e => setBusqueda(e.target.value)} sx={{ flex: 1 }} />
                <TextField size="small" select label="Tipo de agencia" value={tipoAgencia} onChange={e => setTipoAgencia(e.target.value)} sx={{ minWidth: 180 }}>
                    {["Todas", "Local", "Foránea"].map(t => <MenuItem key={t} value={t}>{t}</MenuItem>)}</TextField>
                {filtrado && <Button onClick={() => { setBusqueda(""); setTipoAgencia("Todas"); }}>Limpiar filtros</Button>}
            </Stack>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>{filtrado ? "Resumen de los resultados filtrados" : `Resumen de ${anio}`}</Typography>
            <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr 1fr", md: "repeat(4, 1fr)" }, gap: 2, mb: 2 }}>
                {[["UPS con reemplazo", equipos.length], ["Cambios previstos", resumen.cambios], ["Baterías por comprar", resumen.baterias], ["Presupuesto estimado", moneda(resumen.total)]].map(([titulo, valor], i) =>
                    <Card key={titulo} variant="outlined" sx={{ borderRadius: 3, bgcolor: i === 3 ? "#eef4ff" : "white" }}><CardContent>
                        <Typography variant="body2" color="text.secondary">{titulo}</Typography><Typography variant="h5" fontWeight={700} sx={{ mt: 1 }}>{valor}</Typography>
                    </CardContent></Card>)}
            </Box>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                {anio === actual ? "Estimación pendiente desde hoy hasta diciembre; no incluye gastos ya realizados." : "Estimación de enero a diciembre con el inventario y la política actuales."}
            </Typography>
            {proyeccion.totalUPSPendientesPrevios > 0 && <Alert severity="warning" sx={{ mb: 2 }}>
                {anio === actual ? `Los vencidos se incluyen una sola vez. Importe pendiente${filtrado ? " filtrado" : ""}: ${moneda(resumen.pendiente)}.` : "Hay reemplazos vencidos. Los ciclos futuros suponen que se atienden los pendientes; confirme las fechas al realizar los cambios."}
                {" "}Las fechas futuras son estimadas y no crean órdenes de trabajo.
            </Alert>}
            {Boolean(proyeccion.totalUPSExcluidos) && <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                {proyeccion.totalUPSExcluidos} UPS fuera de servicio o de baja excluidos. Solo se incluyen agencias activas.
            </Typography>}
            {agencias.map(a => <Accordion key={a.agenciaId} defaultExpanded disableGutters sx={{ mb: 2, border: "1px solid #e2e8f0", boxShadow: "none", borderRadius: "12px !important", "&:before": { display: "none" } }}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Box sx={{ display: "flex", flexDirection: { xs: "column", sm: "row" }, alignItems: { sm: "center" }, gap: 2, justifyContent: "space-between", width: "100%", minWidth: 0, pr: 2 }}>
                        <Box sx={{ minWidth: 0, overflowWrap: "anywhere" }}><Typography fontWeight={700}>{a.agencia}</Typography><Typography variant="body2" color="text.secondary">{a.tipoAgencia} · {a.ups.length} UPS</Typography></Box>
                        <Typography fontWeight={700} sx={{ flexShrink: 0 }}>{moneda(a.ups.reduce((total, u) => total + u.costoEstimado, 0))}</Typography>
                    </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ pt: 0 }}>
                    <TableContainer sx={{ overflowX: "auto" }}>
                        <Table size="small" sx={{ minWidth: 1100, tableLayout: "fixed", "& td": { verticalAlign: "top", overflowWrap: "anywhere" } }}>
                            <colgroup>{[20, 8, 25, 8, 11, 11, 17].map((ancho, i) => <col key={i} style={{ width: `${ancho}%` }} />)}</colgroup>
                            <TableHead><TableRow>{["UPS / inventario", "Ciclo", "Fechas previstas", "Cambios", "Baterías / cambio", "Baterías totales", "Estimado"].map((c, i) => <TableCell key={c} align={i >= 3 ? "right" : "left"} sx={{ fontWeight: 700, bgcolor: "#f8fafc" }}>{c}</TableCell>)}</TableRow></TableHead>
                            <TableBody>{a.ups.map(u => <TableRow key={u.upsId} hover>
                                <TableCell><Typography fontWeight={600}>{u.numeroInventario}</Typography><Typography variant="caption" color="text.secondary">{u.marca} {u.modelo} · {u.numeroSerie}</Typography></TableCell>
                                <TableCell>{u.mesesCicloReemplazo} {u.mesesCicloReemplazo === 1 ? "mes" : "meses"}</TableCell>
                                <TableCell>
                                    <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.75, minWidth: 0, "& .MuiChip-root": { maxWidth: "100%" } }}>
                                        {u.estaVencido && <Chip size="small" color="warning" label={`Pendiente: ${fecha(u.fechaVencimientoPendiente)}`} />}
                                        {u.fechasReemplazo.slice(0, 2).map(f => <Chip key={f} size="small" variant="outlined" label={fecha(f)} />)}
                                    </Box>
                                    {u.fechasReemplazo.length > 2 && <Button size="small" onClick={() => setCalendario({ ...u, agencia: a.agencia })} aria-label={`Ver las ${u.fechasReemplazo.length} fechas de ${u.numeroInventario}`} sx={{ mt: 0.5 }}>Ver las {u.fechasReemplazo.length} fechas</Button>}
                                    {u.tieneMantenimientoProgramado && <Typography display="block" variant="caption" color="info.main" sx={{ mt: 0.5 }}>Trabajo programado: {fecha(u.fechaMantenimientoProgramado)}</Typography>}
                                </TableCell>
                                <TableCell align="right">{u.cantidadCambios}</TableCell>
                                <TableCell align="right">{u.cantidadBaterias}</TableCell>
                                <TableCell align="right">{u.bateriasProyectadas}</TableCell>
                                <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>{moneda(u.costoEstimado)}</TableCell>
                            </TableRow>)}</TableBody>
                        </Table>
                    </TableContainer>
                </AccordionDetails>
            </Accordion>)}
            {!agencias.length && <Alert severity="info">{filtrado ? "No hay equipos que coincidan con los filtros." : "No hay reemplazos previstos para este período con los datos actuales."}</Alert>}
            {proyeccion.upsInformacionInsuficienteDetalle?.length > 0 && <Alert severity="warning" sx={{ mt: 2 }}>
                <Typography fontWeight={600}>Equipos pendientes de corregir</Typography>
                {proyeccion.upsInformacionInsuficienteDetalle.map(u => <Typography key={u.upsId} variant="body2">{u.numeroInventario} · {u.agencia}: {u.observacion}</Typography>)}
            </Alert>}
        </>}
        <Dialog open={Boolean(calendario)} onClose={() => setCalendario(null)} fullWidth maxWidth="sm" aria-labelledby="calendario-titulo">
            <DialogTitle id="calendario-titulo">Fechas previstas · {calendario?.numeroInventario}</DialogTitle>
            <DialogContent dividers>
                <Typography fontWeight={600}>{calendario?.agencia}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Proyección {anio} · Ciclo de {calendario?.mesesCicloReemplazo} {calendario?.mesesCicloReemplazo === 1 ? "mes" : "meses"}</Typography>
                {calendario?.estaVencido && <Alert severity="warning" sx={{ mb: 2 }}>Reemplazo pendiente desde {fecha(calendario.fechaVencimientoPendiente)}.</Alert>}
                <Box component="ol" sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", sm: "repeat(3, minmax(0, 1fr))" }, gap: 1, listStyle: "none", p: 0, m: 0 }}>
                    {calendario?.fechasReemplazo.map((f, i) => <Box component="li" key={f} sx={{ p: 1.5, border: "1px solid", borderColor: "divider", borderRadius: 2 }}>
                        <Typography variant="caption" color="text.secondary">Fecha {i + 1}</Typography>
                        <Typography fontWeight={600}>{fecha(f)}</Typography>
                    </Box>)}
                </Box>
                {calendario?.tieneMantenimientoProgramado && <Alert severity="info" sx={{ mt: 2 }}>Trabajo programado: {fecha(calendario.fechaMantenimientoProgramado)}.</Alert>}
                <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>Estas fechas son estimadas y no crean órdenes de trabajo.</Typography>
            </DialogContent>
            <DialogActions><Button onClick={() => setCalendario(null)}>Cerrar</Button></DialogActions>
        </Dialog>
        <Dialog open={Boolean(dialogo)} onClose={guardando ? undefined : () => setDialogo(null)} fullWidth maxWidth="xs">
            <DialogTitle>{dialogo?.tipo === "costo" ? `Costo por batería · ${dialogo.anio}` : "Política de reemplazo"}</DialogTitle>
            <DialogContent><Stack gap={2} sx={{ pt: 1 }}>
                {errorDialogo && <Alert severity="error">{errorDialogo}</Alert>}
                {dialogo?.tipo === "costo" ? <TextField autoFocus label="Costo estimado (L)" type="number" value={costo} onChange={e => setCosto(e.target.value)} slotProps={{ htmlInput: { min: 0.01, step: 0.01 } }} /> : <>
                    <TextField label="Agencias locales · meses" type="number" value={local} onChange={e => setLocal(e.target.value)} slotProps={{ htmlInput: { min: 1, max: 120, step: 1 } }} />
                    <TextField label="Agencias foráneas · meses" type="number" value={foranea} onChange={e => setForanea(e.target.value)} slotProps={{ htmlInput: { min: 1, max: 120, step: 1 } }} />
                    <Typography variant="body2" color="text.secondary">La política aplica a las próximas proyecciones de todos los años.</Typography>
                </>}
            </Stack></DialogContent>
            <DialogActions><Button disabled={guardando} onClick={() => setDialogo(null)}>Cancelar</Button><Button variant="contained" disabled={guardando} onClick={guardar}>{guardando ? "Guardando…" : "Guardar"}</Button></DialogActions>
        </Dialog>
    </Box>;
}

import { useEffect, useState } from "react";

import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Button,
    MenuItem,
    Autocomplete,
    Box,
    Alert
} from "@mui/material";

import { obtenerAgencias } from "../../services/agenciaService";

import {
    crearUPS,
    actualizarUPS
} from "../../services/upsService";

import {
    obtenerEstadosUPS
} from "../../services/catalogosService";


const formularioInicial = {
    codigo: "",
    marca: "",
    modelo: "",
    numeroSerie: "",
    capacidadVA: "",
    agenciaId: "",
    fechaInstalacion: "",
    estadoUPSId: "",
    cantidadBaterias: "",
    fechaUltimoCambioBaterias: ""
};


export default function NuevoUPSModal({
    open,
    onClose,
    onGuardado,
    ups
}) {

    const [agencias, setAgencias] = useState([]);
    const [estadosUPS, setEstadosUPS] = useState([]);

    const [formulario, setFormulario] = useState(
        formularioInicial
    );

    const [guardando, setGuardando] = useState(false);
    const [cargando, setCargando] = useState(false);
    const [error, setError] = useState("");


    useEffect(() => {

        if (!open) {
            return;
        }

        setError("");

        cargarDatos();

    }, [open, ups]);


    const cargarDatos = async () => {

        try {

            setCargando(true);

            const [
                agenciasData,
                estadosData
            ] = await Promise.all([
                obtenerAgencias(),
                obtenerEstadosUPS()
            ]);


            const agenciasActivas =
                (agenciasData || []).filter(
                    agencia => agencia.activa
                );


            setAgencias(agenciasActivas);

            setEstadosUPS(
                estadosData || []
            );


            if (ups) {

                const estadoId =
                    ups.estadoUPSId ??
                    ups.estadoUPS?.id ??
                    "";


                setFormulario({

                    codigo:
                        ups.codigo || "",

                    marca:
                        ups.marca || "",

                    modelo:
                        ups.modelo || "",

                    numeroSerie:
                        ups.numeroSerie || "",

                    capacidadVA:
                        ups.capacidadVA ?? "",

                    agenciaId:
                        ups.agenciaId ??
                        ups.agencia?.id ??
                        "",

                    fechaInstalacion:
                        ups.fechaInstalacion
                            ? ups.fechaInstalacion.substring(
                                0,
                                10
                            )
                            : "",

                    estadoUPSId:
                        estadoId
                            ? String(estadoId)
                            : "",

                    cantidadBaterias:
                        ups.cantidadBaterias ?? "",

                    fechaUltimoCambioBaterias:
                        ups.fechaUltimoCambioBaterias
                            ? ups.fechaUltimoCambioBaterias.substring(
                                0,
                                10
                            )
                            : ""
                });

            } else {

                setFormulario({
                    ...formularioInicial
                });

            }

        } catch (error) {

            console.error(
                "Error al cargar datos del formulario UPS:",
                error
            );

            setError(
                "No se pudieron cargar las agencias y estados de UPS."
            );

        } finally {

            setCargando(false);

        }

    };


    const manejarCambio = event => {

        const {
            name,
            value
        } = event.target;


        setFormulario(prev => ({
            ...prev,
            [name]: value
        }));

    };


    const obtenerFechaActual = () => {

        const fecha =
            new Date();

        const anio =
            fecha.getFullYear();

        const mes =
            String(
                fecha.getMonth() + 1
            ).padStart(2, "0");

        const dia =
            String(
                fecha.getDate()
            ).padStart(2, "0");

        return `${anio}-${mes}-${dia}`;

    };


    const obtenerNombreCiudad = agencia => {

        if (!agencia) {
            return "";
        }


        if (
            typeof agencia.ciudad === "string"
        ) {

            return agencia.ciudad;

        }


        if (
            agencia.ciudad &&
            typeof agencia.ciudad === "object"
        ) {

            return (
                agencia.ciudad.nombre ||
                agencia.ciudad.nombreCiudad ||
                agencia.ciudad.descripcion ||
                ""
            );

        }


        if (
            agencia.ciudadNombre
        ) {

            return agencia.ciudadNombre;

        }


        return "";

    };


    const obtenerNombreDepartamento = agencia => {

        if (!agencia) {
            return "";
        }


        if (
            typeof agencia.departamento === "string"
        ) {

            return agencia.departamento;

        }


        if (
            agencia.departamento &&
            typeof agencia.departamento === "object"
        ) {

            return (
                agencia.departamento.nombre ||
                agencia.departamento.nombreDepartamento ||
                agencia.departamento.descripcion ||
                ""
            );

        }


        if (
            agencia.departamentoNombre
        ) {

            return agencia.departamentoNombre;

        }


        if (
            agencia.ciudad?.departamento
        ) {

            if (
                typeof agencia.ciudad.departamento === "string"
            ) {

                return agencia.ciudad.departamento;

            }

            return (
                agencia.ciudad.departamento.nombre ||
                agencia.ciudad.departamento.nombreDepartamento ||
                agencia.ciudad.departamento.descripcion ||
                ""
            );

        }


        return "";

    };


    const obtenerEtiquetaAgencia = agencia => {

        if (!agencia) {
            return "";
        }


        const codigo =
            agencia.codigo || "";


        const nombre =
            agencia.nombre || "";


        const ciudad =
            obtenerNombreCiudad(agencia);


        const departamento =
            obtenerNombreDepartamento(agencia);


        const ubicacion =
            ciudad && departamento
                ? `${ciudad}, ${departamento}`
                : ciudad || departamento;


        return [
            codigo,
            nombre,
            ubicacion
        ]
            .filter(Boolean)
            .join(" - ");

    };


    const guardar = async () => {

        setError("");


        if (!formulario.codigo.trim()) {

            setError(
                "El código es obligatorio."
            );

            return;

        }


        if (!formulario.numeroSerie.trim()) {

            setError(
                "El número de serie es obligatorio."
            );

            return;

        }


        if (!formulario.marca.trim()) {

            setError(
                "La marca es obligatoria."
            );

            return;

        }


        if (!formulario.modelo.trim()) {

            setError(
                "El modelo es obligatorio."
            );

            return;

        }


        if (
            !formulario.capacidadVA ||
            Number(formulario.capacidadVA) <= 0
        ) {

            setError(
                "La capacidad debe ser mayor que cero."
            );

            return;

        }


        if (!formulario.agenciaId) {

            setError(
                "Seleccione una agencia."
            );

            return;

        }


        if (!formulario.fechaInstalacion) {

            setError(
                "La fecha de instalación es obligatoria."
            );

            return;

        }


        const fechaActual =
            obtenerFechaActual();


        if (
            formulario.fechaInstalacion >
            fechaActual
        ) {

            setError(
                "La fecha de instalación no puede ser futura."
            );

            return;

        }


        if (!formulario.estadoUPSId) {

            setError(
                "Seleccione el estado del UPS."
            );

            return;

        }


        if (
            formulario.cantidadBaterias === "" ||
            Number(formulario.cantidadBaterias) <= 0
        ) {

            setError(
                "La cantidad de baterías debe ser mayor que cero."
            );

            return;

        }


        if (
            formulario.fechaUltimoCambioBaterias &&
            formulario.fechaUltimoCambioBaterias >
                fechaActual
        ) {

            setError(
                "La fecha del último cambio de baterías no puede ser futura."
            );

            return;

        }


        if (
            formulario.fechaUltimoCambioBaterias &&
            formulario.fechaUltimoCambioBaterias <
                formulario.fechaInstalacion
        ) {

            setError(
                "La fecha del último cambio de baterías no puede ser anterior a la fecha de instalación."
            );

            return;

        }


        try {

            setGuardando(true);


            const datosUPS = {

                id:
                    ups?.id ?? 0,

                codigo:
                    formulario.codigo.trim(),

                marca:
                    formulario.marca.trim(),

                modelo:
                    formulario.modelo.trim(),

                numeroSerie:
                    formulario.numeroSerie.trim(),

                capacidadVA:
                    Number(
                        formulario.capacidadVA
                    ),

                agenciaId:
                    Number(
                        formulario.agenciaId
                    ),

                estadoUPSId:
                    Number(
                        formulario.estadoUPSId
                    ),

                fechaInstalacion:
                    formulario.fechaInstalacion,

                cantidadBaterias:
                    Number(
                        formulario.cantidadBaterias
                    ),

                fechaUltimoCambioBaterias:
                    formulario.fechaUltimoCambioBaterias ||
                    null

            };


            if (ups) {

                await actualizarUPS(
                    ups.id,
                    datosUPS
                );

            } else {

                await crearUPS(
                    datosUPS
                );

            }


            await onGuardado();

            onClose();

        } catch (error) {

            console.error(
                "Error al guardar UPS:",
                error
            );


            const mensaje =
                error.response?.data?.mensaje ||
                error.response?.data?.message ||
                "No se pudo guardar el UPS.";


            setError(mensaje);

        } finally {

            setGuardando(false);

        }

    };


    const agenciaSeleccionada =
        agencias.find(
            agencia =>
                Number(agencia.id) ===
                Number(
                    formulario.agenciaId
                )
        ) || null;


    return (

        <Dialog
            open={open}
            onClose={
                guardando
                    ? undefined
                    : onClose
            }
            fullWidth
            maxWidth="sm"
        >

            <DialogTitle>

                {ups
                    ? "Editar UPS"
                    : "Nuevo UPS"}

            </DialogTitle>


            <DialogContent>

                {error && (

                    <Alert
                        severity="error"
                        sx={{
                            mt: 1,
                            mb: 1
                        }}
                    >

                        {error}

                    </Alert>

                )}


                <Box
                    sx={{
                        display: "flex",
                        flexDirection: "column",
                        gap: 1,
                        mt: 1
                    }}
                >

                    <TextField
                        fullWidth
                        required
                        label="Código"
                        name="codigo"
                        value={
                            formulario.codigo
                        }
                        onChange={
                            manejarCambio
                        }
                        disabled={
                            guardando
                        }
                    />


                    <TextField
                        fullWidth
                        required
                        label="Número de serie"
                        name="numeroSerie"
                        value={
                            formulario.numeroSerie
                        }
                        onChange={
                            manejarCambio
                        }
                        disabled={
                            guardando
                        }
                    />


                    <TextField
                        fullWidth
                        required
                        label="Marca"
                        name="marca"
                        value={
                            formulario.marca
                        }
                        onChange={
                            manejarCambio
                        }
                        disabled={
                            guardando
                        }
                    />


                    <TextField
                        fullWidth
                        required
                        label="Modelo"
                        name="modelo"
                        value={
                            formulario.modelo
                        }
                        onChange={
                            manejarCambio
                        }
                        disabled={
                            guardando
                        }
                    />


                    <TextField
                        fullWidth
                        required
                        label="Capacidad (VA)"
                        name="capacidadVA"
                        type="number"
                        value={
                            formulario.capacidadVA
                        }
                        onChange={
                            manejarCambio
                        }
                        disabled={
                            guardando
                        }
                        inputProps={{
                            min: 1
                        }}
                    />


                    <Autocomplete
                        fullWidth
                        options={agencias}
                        value={
                            agenciaSeleccionada
                        }
                        loading={cargando}
                        disabled={
                            guardando ||
                            cargando
                        }
                        getOptionLabel={
                            agencia =>
                                obtenerEtiquetaAgencia(
                                    agencia
                                )
                        }
                        isOptionEqualToValue={
                            (
                                option,
                                value
                            ) =>
                                Number(option.id) ===
                                Number(value.id)
                        }
                        onChange={
                            (
                                event,
                                nuevaAgencia
                            ) => {

                                setFormulario(
                                    prev => ({
                                        ...prev,
                                        agenciaId:
                                            nuevaAgencia
                                                ? nuevaAgencia.id
                                                : ""
                                    })
                                );

                            }
                        }
                        renderInput={
                            params => (

                                <TextField
                                    {...params}
                                    required
                                    label="Agencia"
                                    placeholder="Buscar agencia..."
                                />

                            )
                        }
                    />


                    <Box>

                        <Box
                            component="label"
                            sx={{
                                display: "block",
                                fontSize: "0.9rem",
                                color: "text.secondary",
                                mb: 0.5
                            }}
                        >
                            Fecha de instalación *
                        </Box>


                        <TextField
                            fullWidth
                            required
                            name="fechaInstalacion"
                            type="date"
                            value={
                                formulario.fechaInstalacion
                            }
                            onChange={
                                manejarCambio
                            }
                            disabled={
                                guardando
                            }
                            inputProps={{
                                max: obtenerFechaActual()
                            }}
                        />

                    </Box>


                    <TextField
                        fullWidth
                        required
                        label="Cantidad de baterías"
                        name="cantidadBaterias"
                        type="number"
                        value={
                            formulario.cantidadBaterias
                        }
                        onChange={
                            manejarCambio
                        }
                        disabled={
                            guardando
                        }
                        inputProps={{
                            min: 1
                        }}
                    />


                    <Box>

                        <Box
                            component="label"
                            sx={{
                                display: "block",
                                fontSize: "0.9rem",
                                color: "text.secondary",
                                mb: 0.5
                            }}
                        >
                            Último cambio de baterías
                        </Box>


                        <TextField
                            fullWidth
                            name="fechaUltimoCambioBaterias"
                            type="date"
                            value={
                                formulario.fechaUltimoCambioBaterias
                            }
                            onChange={
                                manejarCambio
                            }
                            disabled={
                                guardando
                            }
                            inputProps={{
                                min:
                                    formulario.fechaInstalacion ||
                                    undefined,
                                max: obtenerFechaActual()
                            }}
                        />

                    </Box>


                    <TextField
                        fullWidth
                        required
                        select
                        label="Estado"
                        name="estadoUPSId"
                        value={
                            formulario.estadoUPSId
                        }
                        onChange={
                            manejarCambio
                        }
                        disabled={
                            guardando ||
                            cargando
                        }
                    >

                        <MenuItem value="">
                            Seleccione...
                        </MenuItem>


                        {estadosUPS.map(
                            estado => (

                                <MenuItem
                                    key={
                                        estado.id
                                    }
                                    value={
                                        estado.id
                                    }
                                >

                                    {
                                        estado.nombre
                                    }

                                </MenuItem>

                            )
                        )}

                    </TextField>

                </Box>

            </DialogContent>


            <DialogActions>

                <Button
                    onClick={onClose}
                    disabled={
                        guardando
                    }
                >
                    Cancelar
                </Button>


                <Button
                    variant="contained"
                    onClick={guardar}
                    disabled={
                        guardando ||
                        cargando
                    }
                >

                    {guardando
                        ? "Guardando..."
                        : ups
                            ? "Actualizar"
                            : "Guardar"}

                </Button>

            </DialogActions>

        </Dialog>

    );

}
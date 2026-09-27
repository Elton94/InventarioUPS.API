export function resumirCompras(equipos, anio, mesActual) {
    const meses = Array.from({ length: 12 }, (_, mes) => ({ mes: mes + 1, cambios: 0, baterias: 0, costo: 0 }));
    const modelos = new Map();
    for (const u of equipos) {
        const fechas = u.fechasReemplazo || [];
        const eventos = fechas.map(f => Number(f.slice(5, 7)));
        if (u.estaVencido) eventos.push(mesActual);
        for (const mes of eventos) {
            const fila = meses[mes - 1];
            fila.cambios++;
            fila.baterias += u.cantidadBaterias;
            fila.costo += u.cantidadBaterias * u.costoUnitarioBateria;
        }
        const clave = u.modeloBateriaId ? `modelo:${u.modeloBateriaId}` : `tipo:${u.tipoBateria || 'Sin identificar'}`;
        const fila = modelos.get(clave) || { modelo: u.modeloBateria || 'Sin identificar', tipo: u.tipoBateria || 'Sin identificar', baterias: 0, costo: 0, general: u.usaCostoGeneral };
        fila.baterias += u.bateriasProyectadas; fila.costo += u.costoEstimado;
        modelos.set(clave, fila);
    }
    return { anio, meses, modelos: [...modelos.values()] };
}

export function csvProyeccion(equipos, anio) {
    const celda = valor => {
        let texto = String(valor ?? '');
        if (/^[\s]*[=+@-]/.test(texto)) texto = `'${texto}`;
        return `"${texto.replaceAll('"', '""')}"`;
    };
    const filas = [['Año', 'Agencia', 'Inventario', 'Tipo de batería', 'Modelo de batería', 'Cambios', 'Baterías por cambio', 'Baterías totales', 'Costo unitario', 'Origen del costo', 'Estimado', 'Fechas previstas', 'Vencimiento pendiente'],
        ...equipos.map(u => [anio, u.agencia, u.numeroInventario, u.tipoBateria, u.modeloBateria, u.cantidadCambios, u.cantidadBaterias, u.bateriasProyectadas, u.costoUnitarioBateria,
            u.usaCostoGeneral ? 'Referencia general' : 'Específico por modelo', u.costoEstimado, u.fechasReemplazo.map(f => f.slice(0,10)).join(' | '), u.fechaVencimientoPendiente?.slice(0,10) || ''])];
    return '\uFEFF' + filas.map(f => f.map(celda).join(';')).join('\r\n');
}

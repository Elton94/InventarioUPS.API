// =====================================================
// UTILIDADES DE PERMISOS
// =====================================================

const obtenerPayloadToken = () => {
    const token =
        localStorage.getItem("token");

    if (!token) {
        return null;
    }

    try {
        const partes =
            token.split(".");

        if (partes.length !== 3) {
            return null;
        }

        let base64 =
            partes[1]
                .replace(/-/g, "+")
                .replace(/_/g, "/");

        while (base64.length % 4 !== 0) {
            base64 += "=";
        }

        const json =
            decodeURIComponent(
                atob(base64)
                    .split("")
                    .map(
                        (caracter) =>
                            "%" +
                            (
                                "00" +
                                caracter
                                    .charCodeAt(0)
                                    .toString(16)
                            ).slice(-2)
                    )
                    .join("")
            );

        return JSON.parse(json);

    } catch (error) {
        console.error(
            "No fue posible leer el token JWT:",
            error
        );

        return null;
    }
};


// =====================================================
// OBTENER PERMISOS
// =====================================================

export const obtenerPermisosUsuario = () => {
    const payload =
        obtenerPayloadToken();

    if (!payload) {
        return [];
    }

    let permisos =
        payload.Permiso;

    /*
     * Dependiendo de cómo se serialicen los claims,
     * puede llegar como string o como arreglo.
     */

    if (!permisos) {
        return [];
    }

    if (!Array.isArray(permisos)) {
        permisos = [permisos];
    }

    return permisos
        .filter(Boolean)
        .map(
            (permiso) =>
                String(permiso).toUpperCase()
        );
};


// =====================================================
// VERIFICAR PERMISO
// =====================================================

export const tienePermiso = (
    codigoPermiso
) => {
    if (!codigoPermiso) {
        return false;
    }

    const rol =
        localStorage.getItem("rol");

    /*
     * Control Total siempre tiene acceso.
     */

    if (
        rol?.toLowerCase() ===
        "control total"
    ) {
        return true;
    }

    const permisos =
        obtenerPermisosUsuario();

    return permisos.includes(
        codigoPermiso.toUpperCase()
    );
};


// =====================================================
// VERIFICAR CUALQUIER PERMISO
// =====================================================

export const tieneAlgunPermiso = (
    codigosPermisos
) => {
    if (!Array.isArray(codigosPermisos)) {
        return false;
    }

    return codigosPermisos.some(
        (permiso) =>
            tienePermiso(permiso)
    );
};


// =====================================================
// VERIFICAR TODOS LOS PERMISOS
// =====================================================

export const tieneTodosLosPermisos = (
    codigosPermisos
) => {
    if (!Array.isArray(codigosPermisos)) {
        return false;
    }

    return codigosPermisos.every(
        (permiso) =>
            tienePermiso(permiso)
    );
};
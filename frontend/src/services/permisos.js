const obtenerPayloadToken = () => {
    const token =
        localStorage.getItem("token");

    if (!token) {
        return null;
    }

    try {
        const partes = token.split(".");

        if (partes.length !== 3) {
            return null;
        }

        let base64 = partes[1]
            .replace(/-/g, "+")
            .replace(/_/g, "/");

        while (base64.length % 4 !== 0) {
            base64 += "=";
        }

        const json = decodeURIComponent(
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

export const obtenerPermisosUsuario = () => {
    const payload = obtenerPayloadToken();

    if (!payload) {
        return [];
    }

    let permisos =
        payload.Permiso || [];

    if (!Array.isArray(permisos)) {
        permisos = [permisos];
    }

    return permisos
        .filter(Boolean)
        .map((permiso) =>
            String(permiso)
                .trim()
                .toUpperCase()
        );
};

export const tienePermiso = (
    codigoPermiso
) => {
    if (!codigoPermiso) {
        return false;
    }

    const rol =
        localStorage.getItem("rol");

    if (
        rol?.trim().toLowerCase() ===
        "control total"
    ) {
        return true;
    }

    return obtenerPermisosUsuario()
        .includes(
            codigoPermiso
                .trim()
                .toUpperCase()
        );
};

export const tieneAlgunPermiso = (
    codigosPermisos
) => {
    if (!Array.isArray(codigosPermisos)) {
        return false;
    }

    return codigosPermisos.some(
        tienePermiso
    );
};

export const tieneTodosLosPermisos = (
    codigosPermisos
) => {
    if (!Array.isArray(codigosPermisos)) {
        return false;
    }

    return codigosPermisos.every(
        tienePermiso
    );
};

export const obtenerRutaInicial = () => {
    const opciones = [
        ["VER_INICIO", "/inicio"],
        ["VER_AGENCIAS", "/agencias"],
        ["VER_UPS", "/ups"],
        [
            "VER_MANTENIMIENTO",
            "/mantenimientos"
        ],
        [
            "VER_PRESUPUESTO",
            "/presupuesto"
        ]
    ];

    for (const [permiso, ruta] of opciones) {
        if (tienePermiso(permiso)) {
            return ruta;
        }
    }

    if (
        tieneAlgunPermiso([
            "GESTIONAR_USUARIOS",
            "GESTIONAR_ROLES_PERMISOS"
        ])
    ) {
        return "/usuarios";
    }

    return "/sin-permisos";
};

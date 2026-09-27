import { useEffect, useRef } from 'react';
import { Button } from '@mui/material';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import { tieneAlgunPermiso, tieneTodosLosPermisos } from '../../services/permisos';

export default function AccesoCatalogo({ href, children, permisos = ['GESTIONAR_CATALOGOS'], alguno = [], disabled = false, onRegresar }) {
    const pendiente = useRef(false);
    const permitido = tieneTodosLosPermisos(permisos) && (!alguno.length || tieneAlgunPermiso(alguno));
    useEffect(() => {
        if (!permitido || !onRegresar) return;
        const actualizar = () => {
            if (!pendiente.current || disabled || document.visibilityState !== 'visible' || !document.hasFocus()) return;
            pendiente.current = false;
            onRegresar();
        };
        window.addEventListener('focus', actualizar);
        document.addEventListener('visibilitychange', actualizar);
        return () => {
            window.removeEventListener('focus', actualizar);
            document.removeEventListener('visibilitychange', actualizar);
        };
    }, [permitido, disabled, onRegresar]);
    if (!permitido) return null;
    return <Button component="a" href={href} target="_blank" rel="noopener noreferrer"
        size="small" endIcon={<OpenInNewIcon fontSize="small" />} disabled={disabled}
        title="Abre en otra pestaña. Al volver se actualiza la lista sin borrar el formulario."
        onClick={() => { pendiente.current = true; }} sx={{ alignSelf: 'flex-start', textTransform: 'none' }}>
        {children}
    </Button>;
}

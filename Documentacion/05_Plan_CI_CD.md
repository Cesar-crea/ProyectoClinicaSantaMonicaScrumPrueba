# Plan de Integración Continua / Entrega

## Pipeline
1. Checkout.
2. Restore NuGet.
3. Build Release.
4. Tests unitarios/integración.
5. Publicar resultados TRX.
6. (Opcional) análisis estático/SAST.
7. Publicar artefacto.
8. Deploy a staging.
9. Smoke test.
10. Aprobación manual para producción.

El workflow `.github/workflows/ci.yml` implementa los pasos 1-5. La conexión SQL se debe entregar como secret/setting en el ambiente, nunca en el repositorio.

## Branching
- `main/master`: producción.
- `develop`: integración.
- `feature/*`: historias.
- Pull Request obligatorio + CI verde.

## Definition of Done
Código revisado, compilación verde, pruebas verdes, criterios de aceptación verificados, documentación actualizada y sin defectos críticos/altos.

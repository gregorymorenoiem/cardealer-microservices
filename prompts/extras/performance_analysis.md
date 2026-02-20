Context: #codebase
Área de performance: [API / DB queries / Frontend bundle / memoria / etc.]

Diagnostica los principales cuellos de botella en esta área:
1. Queries sin índice o N+1 detectables estáticamente
2. Operaciones síncronas que bloquean el event loop o hilo principal
3. Componentes que re-renderizan innecesariamente (React)
4. Endpoints sin paginación o con payloads excesivos
5. Assets sin optimizar o sin lazy loading
6. Cacheo ausente donde sería trivial y de alto impacto

Para cada issue: archivo:línea | impacto estimado | fix recomendado | esfuerzo.
Prioriza los que tienen mayor impacto con menor esfuerzo.

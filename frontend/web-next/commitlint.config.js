module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      [
        'feat', // Nueva funcionalidad
        'fix', // Corrección de bug
        'docs', // Documentación
        'style', // Estilos (no afecta lógica)
        'refactor', // Refactorización
        'perf', // Mejora de performance
        'test', // Tests
        'build', // Build system
        'ci', // CI/CD
        'chore', // Mantenimiento
        'revert', // Revert de commit
      ],
    ],
    'scope-enum': [
      2,
      'always',
      [
        'ui', // Componentes UI
        'api', // Integraciones API
        'auth', // Autenticación
        'vehicles', // Vehículos
        'dealers', // Dealers
        'users', // Usuarios
        'search', // Búsqueda
        'billing', // Facturación
        'config', // Configuración
        'deps', // Dependencias
        'tests', // Tests
        'i18n', // Internacionalización
      ],
    ],
    'subject-case': [2, 'always', 'lower-case'],
    'header-max-length': [2, 'always', 100],
  },
};

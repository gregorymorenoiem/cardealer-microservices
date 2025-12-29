import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

/// PE-010: About & Legal (Sprint 11)
/// Información de la aplicación y documentos legales
class AboutLegalPage extends StatelessWidget {
  const AboutLegalPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Acerca de'),
      ),
      body: ListView(
        children: [
          // App logo and info
          Container(
            padding: const EdgeInsets.all(32),
            child: Column(
              children: [
                Container(
                  width: 100,
                  height: 100,
                  decoration: BoxDecoration(
                    color: Theme.of(context).colorScheme.primaryContainer,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Icon(
                    Icons.directions_car,
                    size: 60,
                    color: Theme.of(context).colorScheme.primary,
                  ),
                ),
                const SizedBox(height: 16),
                Text(
                  'CarDealer',
                  style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                const SizedBox(height: 8),
                Text(
                  'Version 2.1.0 (Build 210)',
                  style: TextStyle(
                    color: Colors.grey.shade600,
                    fontSize: 14,
                  ),
                ),
                const SizedBox(height: 16),
                OutlinedButton.icon(
                  onPressed: () {
                    _showWhatsNewDialog(context);
                  },
                  icon: const Icon(Icons.new_releases),
                  label: const Text('¿Qué hay de nuevo?'),
                ),
              ],
            ),
          ),
          const Divider(),

          // Version info
          const _SectionTitle(title: 'Información de la Aplicación'),
          const ListTile(
            leading: Icon(Icons.info),
            title: Text('Versión'),
            subtitle: Text('2.1.0 (210)'),
          ),
          const ListTile(
            leading: Icon(Icons.build),
            title: Text('Compilación'),
            subtitle: Text('Release • Diciembre 2024'),
          ),
          const ListTile(
            leading: Icon(Icons.update),
            title: Text('Última actualización'),
            subtitle: Text('15 de diciembre, 2024'),
          ),
          ListTile(
            leading: const Icon(Icons.check_circle),
            title: const Text('Buscar actualizaciones'),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('La aplicación está actualizada'),
                  backgroundColor: Colors.green,
                ),
              );
            },
          ),
          const Divider(),

          // Legal documents
          const _SectionTitle(title: 'Legal'),
          ListTile(
            leading: const Icon(Icons.description),
            title: const Text('Términos de Servicio'),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => const _LegalDocumentPage(
                    title: 'Términos de Servicio',
                    documentType: 'terms',
                  ),
                ),
              );
            },
          ),
          ListTile(
            leading: const Icon(Icons.privacy_tip),
            title: const Text('Política de Privacidad'),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => const _LegalDocumentPage(
                    title: 'Política de Privacidad',
                    documentType: 'privacy',
                  ),
                ),
              );
            },
          ),
          ListTile(
            leading: const Icon(Icons.cookie),
            title: const Text('Política de Cookies'),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => const _LegalDocumentPage(
                    title: 'Política de Cookies',
                    documentType: 'cookies',
                  ),
                ),
              );
            },
          ),
          ListTile(
            leading: const Icon(Icons.copyright),
            title: const Text('Aviso DMCA'),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => const _LegalDocumentPage(
                    title: 'Aviso DMCA',
                    documentType: 'dmca',
                  ),
                ),
              );
            },
          ),
          ListTile(
            leading: const Icon(Icons.article),
            title: const Text('Licencias de Software'),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () {
              showLicensePage(
                context: context,
                applicationName: 'CarDealer',
                applicationVersion: '2.1.0',
                applicationIcon: Container(
                  width: 60,
                  height: 60,
                  decoration: BoxDecoration(
                    color: Theme.of(context).colorScheme.primaryContainer,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Icon(
                    Icons.directions_car,
                    size: 36,
                    color: Theme.of(context).colorScheme.primary,
                  ),
                ),
              );
            },
          ),
          const Divider(),

          // Social and sharing
          const _SectionTitle(title: 'Comparte CarDealer'),
          ListTile(
            leading: const Icon(Icons.star),
            title: const Text('Calificar en la tienda'),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                    content: Text('Abriendo tienda de aplicaciones...')),
              );
            },
          ),
          ListTile(
            leading: const Icon(Icons.share),
            title: const Text('Compartir con amigos'),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Compartiendo...')),
              );
            },
          ),
          const Divider(),

          // Social media
          const _SectionTitle(title: 'Síguenos'),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                _SocialButton(
                  icon: Icons.facebook,
                  label: 'Facebook',
                  color: const Color(0xFF1877F2),
                  onTap: () => _launchURL('https://facebook.com/cardealer'),
                ),
                _SocialButton(
                  icon: Icons.camera_alt,
                  label: 'Instagram',
                  color: const Color(0xFFE4405F),
                  onTap: () => _launchURL('https://instagram.com/cardealer'),
                ),
                _SocialButton(
                  icon: Icons.alternate_email,
                  label: 'X (Twitter)',
                  color: Colors.black,
                  onTap: () => _launchURL('https://x.com/cardealer'),
                ),
                _SocialButton(
                  icon: Icons.play_arrow,
                  label: 'YouTube',
                  color: const Color(0xFFFF0000),
                  onTap: () => _launchURL('https://youtube.com/@cardealer'),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          const Divider(),

          // Developer info
          const _SectionTitle(title: 'Desarrollador'),
          const ListTile(
            leading: Icon(Icons.business),
            title: Text('CarDealer Inc.'),
            subtitle: Text('Miami, Florida'),
          ),
          ListTile(
            leading: const Icon(Icons.language),
            title: const Text('Sitio web'),
            subtitle: const Text('www.cardealer.com'),
            trailing: const Icon(Icons.open_in_new, size: 16),
            onTap: () => _launchURL('https://www.cardealer.com'),
          ),
          ListTile(
            leading: const Icon(Icons.email),
            title: const Text('Contacto'),
            subtitle: const Text('support@cardealer.com'),
            trailing: const Icon(Icons.open_in_new, size: 16),
            onTap: () => _launchURL('mailto:support@cardealer.com'),
          ),
          const SizedBox(height: 32),

          // Copyright
          Center(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Text(
                '© 2024 CarDealer Inc.\nTodos los derechos reservados',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.grey.shade600,
                  fontSize: 12,
                ),
              ),
            ),
          ),
          const SizedBox(height: 16),
        ],
      ),
    );
  }

  static void _showWhatsNewDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Row(
          children: [
            Icon(Icons.new_releases, color: Colors.blue),
            SizedBox(width: 8),
            Text('¿Qué hay de nuevo?'),
          ],
        ),
        content: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text(
                'Versión 2.1.0',
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
              const SizedBox(height: 16),
              const _ChangelogItem(
                icon: Icons.palette,
                text: 'Nueva personalización de apariencia',
              ),
              const _ChangelogItem(
                icon: Icons.recommend,
                text: 'Sistema de recomendaciones mejorado',
              ),
              const _ChangelogItem(
                icon: Icons.history,
                text: 'Historial de actividad completo',
              ),
              const _ChangelogItem(
                icon: Icons.help,
                text: 'Centro de ayuda renovado',
              ),
              const _ChangelogItem(
                icon: Icons.privacy_tip,
                text: 'Controles de privacidad mejorados',
              ),
              const SizedBox(height: 16),
              Text(
                'Versión anterior (2.0.0)',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 14,
                  color: Colors.grey.shade700,
                ),
              ),
              const SizedBox(height: 8),
              const _ChangelogItem(
                icon: Icons.analytics,
                text: 'Panel de análisis para dealers',
              ),
              const _ChangelogItem(
                icon: Icons.publish,
                text: 'Asistente de publicación mejorado',
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cerrar'),
          ),
        ],
      ),
    );
  }

  static Future<void> _launchURL(String url) async {
    final uri = Uri.parse(url);
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }
}

/// Section title widget
class _SectionTitle extends StatelessWidget {
  final String title;

  const _SectionTitle({required this.title});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
      child: Text(
        title.toUpperCase(),
        style: TextStyle(
          fontSize: 12,
          fontWeight: FontWeight.bold,
          color: Theme.of(context).colorScheme.primary,
          letterSpacing: 1.2,
        ),
      ),
    );
  }
}

/// Social button widget
class _SocialButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;

  const _SocialButton({
    required this.icon,
    required this.label,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.all(12),
        child: Column(
          children: [
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: color.withAlpha(25),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(icon, color: color, size: 28),
            ),
            const SizedBox(height: 8),
            Text(
              label,
              style: const TextStyle(fontSize: 11),
            ),
          ],
        ),
      ),
    );
  }
}

/// Changelog item widget
class _ChangelogItem extends StatelessWidget {
  final IconData icon;
  final String text;

  const _ChangelogItem({
    required this.icon,
    required this.text,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 16, color: Colors.grey.shade600),
          const SizedBox(width: 8),
          Expanded(child: Text(text)),
        ],
      ),
    );
  }
}

/// Legal document page
class _LegalDocumentPage extends StatelessWidget {
  final String title;
  final String documentType;

  const _LegalDocumentPage({
    required this.title,
    required this.documentType,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(title),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 8),
            Text(
              'Última actualización: 15 de diciembre, 2024',
              style: TextStyle(
                color: Colors.grey.shade600,
                fontSize: 12,
              ),
            ),
            const SizedBox(height: 24),
            Text(
              _getDocumentContent(documentType),
              style: const TextStyle(height: 1.6),
            ),
          ],
        ),
      ),
    );
  }

  String _getDocumentContent(String type) {
    switch (type) {
      case 'terms':
        return '''
TÉRMINOS DE SERVICIO

1. ACEPTACIÓN DE LOS TÉRMINOS
Al acceder y utilizar CarDealer, usted acepta estar sujeto a estos Términos de Servicio y todas las leyes y regulaciones aplicables.

2. USO DEL SERVICIO
Usted acepta utilizar CarDealer solo para fines legales y de acuerdo con estos términos.

3. CUENTA DE USUARIO
- Debe tener al menos 18 años para crear una cuenta
- Es responsable de mantener la confidencialidad de su contraseña
- Es responsable de toda actividad en su cuenta

4. CONTENIDO DEL USUARIO
- Usted conserva los derechos de su contenido
- Nos otorga una licencia para usar, modificar y mostrar su contenido
- No debe publicar contenido ilegal, ofensivo o engañoso

5. PUBLICACIONES DE VEHÍCULOS
- Debe tener derecho legal para vender los vehículos listados
- La información debe ser precisa y completa
- Nos reservamos el derecho de eliminar publicaciones que violen estos términos

6. TRANSACCIONES
- CarDealer facilita conexiones entre compradores y vendedores
- No somos parte de las transacciones entre usuarios
- No garantizamos la calidad, seguridad o legalidad de los vehículos

7. TARIFAS Y PAGOS
- Los precios de suscripciones están sujetos a cambio
- Los pagos no son reembolsables excepto según lo requerido por ley

8. PROPIEDAD INTELECTUAL
- Todo el contenido de CarDealer está protegido por derechos de autor
- No puede usar nuestro contenido sin permiso

9. LIMITACIÓN DE RESPONSABILIDAD
CarDealer no será responsable de daños indirectos, incidentales o consecuentes.

10. MODIFICACIONES
Nos reservamos el derecho de modificar estos términos en cualquier momento.

Para preguntas sobre estos términos, contacte a legal@cardealer.com
''';
      case 'privacy':
        return '''
POLÍTICA DE PRIVACIDAD

1. INFORMACIÓN QUE RECOPILAMOS
- Información de cuenta: nombre, email, teléfono
- Información de vehículos: detalles de publicaciones
- Información de uso: páginas visitadas, búsquedas
- Información del dispositivo: tipo, sistema operativo, identificadores

2. CÓMO USAMOS SU INFORMACIÓN
- Proporcionar y mejorar nuestros servicios
- Comunicarnos con usted
- Personalizar su experiencia
- Procesar transacciones
- Cumplir con obligaciones legales

3. COMPARTIR INFORMACIÓN
No vendemos su información personal. Podemos compartir con:
- Vendedores cuando contacta sobre un vehículo
- Proveedores de servicios que nos ayudan a operar
- Autoridades cuando lo requiere la ley

4. SEGURIDAD
Implementamos medidas de seguridad para proteger su información:
- Encriptación SSL/TLS
- Almacenamiento seguro de contraseñas
- Auditorías de seguridad regulares

5. SUS DERECHOS
Usted tiene derecho a:
- Acceder a su información
- Corregir información incorrecta
- Eliminar su cuenta y datos
- Exportar sus datos
- Optar por no recibir comunicaciones

6. COOKIES
Usamos cookies para:
- Mantener su sesión
- Recordar preferencias
- Analizar uso del sitio
- Personalizar contenido

7. RETENCIÓN DE DATOS
Mantenemos su información mientras su cuenta esté activa o según sea necesario para proporcionar servicios.

8. PRIVACIDAD DE MENORES
No recopilamos intencionalmente información de menores de 18 años.

9. CAMBIOS A ESTA POLÍTICA
Le notificaremos sobre cambios significativos a esta política.

10. CONTACTO
Para preguntas sobre privacidad: privacy@cardealer.com
''';
      case 'cookies':
        return '''
POLÍTICA DE COOKIES

1. ¿QUÉ SON LAS COOKIES?
Las cookies son pequeños archivos de texto que los sitios web almacenan en su dispositivo.

2. TIPOS DE COOKIES QUE USAMOS

Cookies Esenciales:
- Necesarias para el funcionamiento del sitio
- Manejo de sesión y autenticación
- No pueden ser desactivadas

Cookies de Rendimiento:
- Analizan cómo usa el sitio
- Nos ayudan a mejorar el servicio
- Puede optar por no usarlas

Cookies de Funcionalidad:
- Recuerdan sus preferencias
- Idioma, ubicación, configuración
- Mejoran su experiencia

Cookies de Publicidad:
- Personalizan anuncios
- Miden efectividad de campañas
- Puede optar por no usarlas

3. COOKIES DE TERCEROS
Usamos servicios de terceros que pueden establecer cookies:
- Google Analytics para análisis
- Facebook Pixel para publicidad
- Servicios de pago para transacciones

4. GESTIÓN DE COOKIES
Puede controlar cookies mediante:
- Configuración del navegador
- Nuestro centro de preferencias
- Herramientas de terceros

5. CONSECUENCIAS DE DESHABILITAR COOKIES
Algunas funciones pueden no funcionar correctamente sin cookies.

Para más información: cookies@cardealer.com
''';
      case 'dmca':
        return '''
AVISO DMCA

CarDealer respeta los derechos de propiedad intelectual de otros y espera que los usuarios hagan lo mismo.

1. NOTIFICACIÓN DE INFRACCIÓN
Si cree que su trabajo ha sido copiado de manera que constituye una infracción de derechos de autor, proporcione:

- Firma física o electrónica del propietario de los derechos
- Identificación del trabajo protegido
- Identificación del material infractor
- Información de contacto
- Declaración de buena fe
- Declaración de exactitud bajo pena de perjurio

2. PROCESO
Después de recibir una notificación válida:
- Eliminaremos el contenido infractor
- Notificaremos al usuario
- Tomaremos acción contra infractores reincidentes

3. CONTRANOTIFICACIÓN
Si cree que su contenido fue eliminado por error:
- Puede enviar una contranotificación
- Incluya las mismas certificaciones
- El contenido puede ser restaurado después de 10-14 días

4. CONTACTO
Agente DMCA de CarDealer:
Email: dmca@cardealer.com
Dirección: 123 Legal Street, Miami, FL 33101

No eliminamos contenido sin una notificación válida bajo la DMCA.
''';
      default:
        return 'Documento no disponible';
    }
  }
}

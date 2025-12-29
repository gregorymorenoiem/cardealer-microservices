import 'package:flutter/material.dart';

/// PE-009: Help & Support (Sprint 11)
/// Centro de ayuda completo con FAQ, soporte y chat
class HelpSupportPage extends StatefulWidget {
  const HelpSupportPage({super.key});

  @override
  State<HelpSupportPage> createState() => _HelpSupportPageState();
}

class _HelpSupportPageState extends State<HelpSupportPage>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';

  // FAQ data
  final List<Map<String, dynamic>> _faqs = [
    {
      'category': 'Account',
      'question': '¿Cómo creo una cuenta?',
      'answer':
          'Para crear una cuenta, haz clic en "Registrarse" en la pantalla de inicio. Completa el formulario con tu información personal y verifica tu correo electrónico.',
    },
    {
      'category': 'Account',
      'question': '¿Cómo cambio mi contraseña?',
      'answer':
          'Ve a Configuración > Cuenta > Cambiar contraseña. Ingresa tu contraseña actual y la nueva contraseña dos veces. La contraseña debe tener al menos 8 caracteres.',
    },
    {
      'category': 'Account',
      'question': '¿Cómo elimino mi cuenta?',
      'answer':
          'Ve a Configuración > Privacidad > Eliminar cuenta. Esta acción es irreversible y eliminará todos tus datos permanentemente.',
    },
    {
      'category': 'Buying',
      'question': '¿Cómo compro un vehículo?',
      'answer':
          'Busca el vehículo que te interesa, revisa los detalles y contacta al vendedor a través del botón "Contactar Vendedor". Podrás enviar mensajes y coordinar una visita o prueba de manejo.',
    },
    {
      'category': 'Buying',
      'question': '¿Puedo negociar el precio?',
      'answer':
          'Sí, puedes negociar directamente con el vendedor a través de nuestro sistema de mensajería. Muchos vendedores están abiertos a ofertas razonables.',
    },
    {
      'category': 'Buying',
      'question': '¿Cómo puedo verificar el historial del vehículo?',
      'answer':
          'Recomendamos solicitar el reporte de Carfax o AutoCheck al vendedor. Muchos vendedores lo incluyen en el anuncio. También puedes verificar el VIN en línea.',
    },
    {
      'category': 'Selling',
      'question': '¿Cómo publico un vehículo?',
      'answer':
          'Ve a tu perfil > Publicar Vehículo. Completa el formulario con los detalles del vehículo, sube fotos de calidad y establece un precio competitivo.',
    },
    {
      'category': 'Selling',
      'question': '¿Cuánto cuesta publicar?',
      'answer':
          'Tenemos planes desde gratis hasta premium. El plan gratuito incluye una publicación básica. Los planes pagos ofrecen más visibilidad, fotos y destacados.',
    },
    {
      'category': 'Selling',
      'question': '¿Cuánto tiempo dura mi publicación?',
      'answer':
          'Las publicaciones gratuitas duran 30 días. Las publicaciones premium duran 60 días y pueden renovarse automáticamente.',
    },
    {
      'category': 'Payments',
      'question': '¿Qué formas de pago aceptan?',
      'answer':
          'Para suscripciones y servicios premium aceptamos tarjetas de crédito/débito, PayPal y transferencias bancarias. Las transacciones de vehículos son directamente entre comprador y vendedor.',
    },
    {
      'category': 'Payments',
      'question': '¿Ofrecen financiamiento?',
      'answer':
          'No ofrecemos financiamiento directo, pero muchos de nuestros dealers asociados sí. Contacta al vendedor para consultar opciones de financiamiento.',
    },
    {
      'category': 'Technical',
      'question': '¿Por qué no puedo iniciar sesión?',
      'answer':
          'Verifica que tu correo y contraseña sean correctos. Si olvidaste tu contraseña, usa "Recuperar contraseña". Si el problema persiste, contacta a soporte.',
    },
    {
      'category': 'Technical',
      'question': '¿La app funciona sin internet?',
      'answer':
          'Necesitas conexión a internet para usar la mayoría de funciones. Algunas funciones como ver favoritos guardados funcionan sin conexión.',
    },
    {
      'category': 'Technical',
      'question': '¿Cómo actualizo la aplicación?',
      'answer':
          'Ve a la App Store (iOS) o Google Play Store (Android) y busca CarDealer. Si hay una actualización disponible, verás el botón "Actualizar".',
    },
  ];

  List<Map<String, dynamic>> get _filteredFaqs {
    if (_searchQuery.isEmpty) return _faqs;

    return _faqs.where((faq) {
      final question = faq['question'].toString().toLowerCase();
      final answer = faq['answer'].toString().toLowerCase();
      final query = _searchQuery.toLowerCase();
      return question.contains(query) || answer.contains(query);
    }).toList();
  }

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Ayuda y Soporte'),
        bottom: TabBar(
          controller: _tabController,
          tabs: const [
            Tab(icon: Icon(Icons.help), text: 'FAQ'),
            Tab(icon: Icon(Icons.email), text: 'Contacto'),
            Tab(icon: Icon(Icons.chat), text: 'Chat'),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          _buildFaqTab(),
          _buildContactTab(),
          _buildChatTab(),
        ],
      ),
    );
  }

  Widget _buildFaqTab() {
    return Column(
      children: [
        // Search bar
        Padding(
          padding: const EdgeInsets.all(16),
          child: TextField(
            controller: _searchController,
            decoration: InputDecoration(
              hintText: 'Buscar en preguntas frecuentes...',
              prefixIcon: const Icon(Icons.search),
              suffixIcon: _searchQuery.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.clear),
                      onPressed: () {
                        setState(() {
                          _searchController.clear();
                          _searchQuery = '';
                        });
                      },
                    )
                  : null,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
            onChanged: (value) {
              setState(() {
                _searchQuery = value;
              });
            },
          ),
        ),

        // FAQ list
        Expanded(
          child: _filteredFaqs.isEmpty
              ? Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(Icons.search_off,
                          size: 64, color: Colors.grey.shade300),
                      const SizedBox(height: 16),
                      const Text('No se encontraron resultados'),
                    ],
                  ),
                )
              : ListView.builder(
                  itemCount: _filteredFaqs.length,
                  itemBuilder: (context, index) {
                    final faq = _filteredFaqs[index];
                    return _FAQItem(faq: faq);
                  },
                ),
        ),
      ],
    );
  }

  Widget _buildContactTab() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Contact info card
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                children: [
                  const Icon(Icons.headset_mic, size: 48, color: Colors.blue),
                  const SizedBox(height: 16),
                  Text(
                    'Estamos aquí para ayudarte',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Te responderemos en 24-48 horas',
                    style: TextStyle(color: Colors.grey.shade600),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 24),

          // Contact form
          Text(
            'Formulario de Contacto',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 16),

          const _ContactForm(),
          const SizedBox(height: 32),

          // Alternative contact methods
          Text(
            'Otros Canales',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 16),

          Card(
            child: Column(
              children: [
                ListTile(
                  leading: const Icon(Icons.phone, color: Colors.green),
                  title: const Text('Llamar'),
                  subtitle: const Text('1-800-CARDEALER'),
                  trailing: const Icon(Icons.arrow_forward_ios, size: 16),
                  onTap: () {
                    // Open phone dialer
                  },
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.email, color: Colors.blue),
                  title: const Text('Email'),
                  subtitle: const Text('support@cardealer.com'),
                  trailing: const Icon(Icons.arrow_forward_ios, size: 16),
                  onTap: () {
                    // Open email client
                  },
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.chat_bubble, color: Colors.orange),
                  title: const Text('WhatsApp'),
                  subtitle: const Text('+1 (555) 123-4567'),
                  trailing: const Icon(Icons.arrow_forward_ios, size: 16),
                  onTap: () {
                    // Open WhatsApp
                  },
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildChatTab() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.chat_bubble_outline, size: 80, color: Colors.blue),
            const SizedBox(height: 24),
            Text(
              'Chat en Vivo',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 16),
            const Text(
              'Chatea con nuestro equipo de soporte en tiempo real',
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 16),
            ),
            const SizedBox(height: 32),

            // Operating hours
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.blue.shade50,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.blue.shade200),
              ),
              child: Column(
                children: [
                  Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.schedule,
                          color: Colors.blue.shade700, size: 20),
                      const SizedBox(width: 8),
                      Text(
                        'Horario de Atención',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          color: Colors.blue.shade700,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  const Text('Lunes - Viernes: 9:00 AM - 6:00 PM EST'),
                  const Text('Sábados: 10:00 AM - 4:00 PM EST'),
                  const Text('Domingos: Cerrado'),
                ],
              ),
            ),
            const SizedBox(height: 32),

            FilledButton.icon(
              onPressed: () {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Conectando con soporte...'),
                  ),
                );
              },
              icon: const Icon(Icons.chat),
              label: const Text('Iniciar Chat'),
              style: FilledButton.styleFrom(
                padding:
                    const EdgeInsets.symmetric(horizontal: 32, vertical: 16),
              ),
            ),
            const SizedBox(height: 16),
            TextButton(
              onPressed: () {
                _tabController.animateTo(1); // Go to contact tab
              },
              child: const Text('¿Prefieres enviarnos un email?'),
            ),
          ],
        ),
      ),
    );
  }
}

/// FAQ Item widget with expansion
class _FAQItem extends StatefulWidget {
  final Map<String, dynamic> faq;

  const _FAQItem({required this.faq});

  @override
  State<_FAQItem> createState() => _FAQItemState();
}

class _FAQItemState extends State<_FAQItem> {
  bool _isExpanded = false;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
      child: ExpansionTile(
        title: Text(widget.faq['question']),
        leading: Icon(
          _getCategoryIcon(widget.faq['category']),
          color: _getCategoryColor(widget.faq['category']),
        ),
        trailing: Icon(_isExpanded ? Icons.expand_less : Icons.expand_more),
        onExpansionChanged: (expanded) {
          setState(() {
            _isExpanded = expanded;
          });
        },
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  widget.faq['answer'],
                  style: TextStyle(
                    color: Colors.grey.shade700,
                    height: 1.5,
                  ),
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Text(
                      '¿Te ayudó esta respuesta?',
                      style: TextStyle(
                        fontSize: 13,
                        color: Colors.grey.shade600,
                      ),
                    ),
                    const Spacer(),
                    IconButton(
                      icon: const Icon(Icons.thumb_up_outlined, size: 20),
                      onPressed: () {},
                    ),
                    IconButton(
                      icon: const Icon(Icons.thumb_down_outlined, size: 20),
                      onPressed: () {},
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  IconData _getCategoryIcon(String category) {
    switch (category) {
      case 'Account':
        return Icons.account_circle;
      case 'Buying':
        return Icons.shopping_cart;
      case 'Selling':
        return Icons.sell;
      case 'Payments':
        return Icons.payment;
      case 'Technical':
        return Icons.settings;
      default:
        return Icons.help;
    }
  }

  Color _getCategoryColor(String category) {
    switch (category) {
      case 'Account':
        return Colors.blue;
      case 'Buying':
        return Colors.green;
      case 'Selling':
        return Colors.orange;
      case 'Payments':
        return Colors.purple;
      case 'Technical':
        return Colors.red;
      default:
        return Colors.grey;
    }
  }
}

/// Contact form widget
class _ContactForm extends StatefulWidget {
  const _ContactForm();

  @override
  State<_ContactForm> createState() => _ContactFormState();
}

class _ContactFormState extends State<_ContactForm> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _messageController = TextEditingController();
  String _subject = 'technical';
  bool _isSending = false;

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _messageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Form(
      key: _formKey,
      child: Column(
        children: [
          TextFormField(
            controller: _nameController,
            decoration: const InputDecoration(
              labelText: 'Nombre',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.person),
            ),
            validator: (value) {
              if (value == null || value.isEmpty) {
                return 'Por favor ingresa tu nombre';
              }
              return null;
            },
          ),
          const SizedBox(height: 16),
          TextFormField(
            controller: _emailController,
            decoration: const InputDecoration(
              labelText: 'Email',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.email),
            ),
            keyboardType: TextInputType.emailAddress,
            validator: (value) {
              if (value == null || value.isEmpty) {
                return 'Por favor ingresa tu email';
              }
              if (!value.contains('@')) {
                return 'Email inválido';
              }
              return null;
            },
          ),
          const SizedBox(height: 16),
          DropdownButtonFormField<String>(
            initialValue: _subject,
            decoration: const InputDecoration(
              labelText: 'Asunto',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.subject),
            ),
            items: const [
              DropdownMenuItem(
                  value: 'technical', child: Text('Soporte Técnico')),
              DropdownMenuItem(
                  value: 'sales', child: Text('Consulta de Ventas')),
              DropdownMenuItem(
                  value: 'account', child: Text('Problema de Cuenta')),
              DropdownMenuItem(value: 'other', child: Text('Otro')),
            ],
            onChanged: (value) {
              setState(() {
                _subject = value!;
              });
            },
          ),
          const SizedBox(height: 16),
          TextFormField(
            controller: _messageController,
            decoration: const InputDecoration(
              labelText: 'Mensaje',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.message),
              alignLabelWithHint: true,
            ),
            maxLines: 5,
            validator: (value) {
              if (value == null || value.isEmpty) {
                return 'Por favor describe tu problema';
              }
              if (value.length < 20) {
                return 'Por favor proporciona más detalles (mínimo 20 caracteres)';
              }
              return null;
            },
          ),
          const SizedBox(height: 16),
          OutlinedButton.icon(
            onPressed: () {},
            icon: const Icon(Icons.attach_file),
            label: const Text('Adjuntar archivo (opcional)'),
          ),
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            child: FilledButton.icon(
              onPressed: _isSending ? null : _sendMessage,
              icon: const Icon(Icons.send),
              label: _isSending
                  ? const Text('Enviando...')
                  : const Text('Enviar Mensaje'),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _sendMessage() async {
    if (_formKey.currentState!.validate()) {
      setState(() {
        _isSending = true;
      });

      // Simulate API call
      await Future.delayed(const Duration(seconds: 2));

      if (mounted) {
        setState(() {
          _isSending = false;
        });

        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Mensaje enviado correctamente'),
            backgroundColor: Colors.green,
          ),
        );

        // Clear form
        _formKey.currentState!.reset();
        _nameController.clear();
        _emailController.clear();
        _messageController.clear();
      }
    }
  }
}

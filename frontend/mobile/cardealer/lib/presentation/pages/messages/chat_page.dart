import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';

/// CM-002: Chat UI Premium
/// CM-003: Media Sharing
/// CM-004: Quick Replies
/// CM-005: Vehicle Card in Chat
/// CM-006: Call Integration
class ChatPage extends StatefulWidget {
  const ChatPage({super.key});

  @override
  State<ChatPage> createState() => _ChatPageState();
}

class _ChatPageState extends State<ChatPage> {
  final TextEditingController _messageController = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  final ImagePicker _imagePicker = ImagePicker();

  bool _isTyping = false;

  final List<Map<String, dynamic>> _messages = [
    {
      'id': '1',
      'text': 'Hola, vi tu anuncio del Toyota Camry 2024',
      'sender': 'user',
      'timestamp': DateTime.now().subtract(const Duration(hours: 2)),
      'status': 'read',
    },
    {
      'id': '2',
      'text':
          '¡Hola! Sí, el vehículo está disponible. ¿Te gustaría agendar una visita?',
      'sender': 'dealer',
      'timestamp':
          DateTime.now().subtract(const Duration(hours: 2, minutes: -2)),
      'status': 'read',
    },
    {
      'id': '3',
      'text': 'Sí, me interesa. ¿Cuál es el mejor precio que puedes ofrecer?',
      'sender': 'user',
      'timestamp':
          DateTime.now().subtract(const Duration(hours: 1, minutes: 30)),
      'status': 'read',
    },
    {
      'id': '4',
      'text':
          'El precio publicado es \$28,000. Por ser compra de contado, puedo ofrecerte \$27,500',
      'sender': 'dealer',
      'timestamp':
          DateTime.now().subtract(const Duration(hours: 1, minutes: 25)),
      'status': 'read',
    },
    {
      'id': '5',
      'text': 'Suena bien. ¿Cuándo puedo verlo?',
      'sender': 'user',
      'timestamp': DateTime.now().subtract(const Duration(minutes: 10)),
      'status': 'delivered',
    },
    {
      'id': '6',
      'text': '¿Cuándo puedes venir a verlo?',
      'sender': 'dealer',
      'timestamp': DateTime.now().subtract(const Duration(minutes: 5)),
      'status': 'delivered',
    },
  ];

  final List<String> _quickReplies = [
    '¿Está disponible?',
    '¿Cuál es el precio final?',
    'Quiero agendar una visita',
    '¿Aceptan financiamiento?',
    'Gracias por la información',
  ];

  final Map<String, dynamic> _vehicleInfo = {
    'title': 'Toyota Camry 2024',
    'price': '\$28,000',
    'year': '2024',
    'mileage': '5,000 km',
    'location': 'Santo Domingo',
    'image': 'https://via.placeholder.com/300x200',
  };

  @override
  void dispose() {
    _messageController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final dealerInfo =
        ModalRoute.of(context)?.settings.arguments as Map<String, dynamic>?;

    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            Stack(
              children: [
                CircleAvatar(
                  radius: 20,
                  backgroundImage: NetworkImage(
                    dealerInfo?['avatar'] ??
                        'https://ui-avatars.com/api/?name=Dealer',
                  ),
                ),
                if (dealerInfo?['isOnline'] == true)
                  Positioned(
                    right: 0,
                    bottom: 0,
                    child: Container(
                      width: 12,
                      height: 12,
                      decoration: BoxDecoration(
                        color: theme.colorScheme.tertiary,
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: theme.colorScheme.surface,
                          width: 2,
                        ),
                      ),
                    ),
                  ),
              ],
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    dealerInfo?['dealerName'] ?? 'Premium Motors',
                    style: theme.textTheme.titleMedium,
                  ),
                  Text(
                    dealerInfo?['isOnline'] == true
                        ? 'En línea'
                        : 'Desconectado',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.phone),
            onPressed: () => _showCallOptions(context),
          ),
          IconButton(
            icon: const Icon(Icons.more_vert),
            onPressed: () => _showChatOptions(context),
          ),
        ],
      ),
      body: Column(
        children: [
          // Vehicle card
          _buildVehicleCard(),

          // Messages list
          Expanded(
            child: ListView.builder(
              controller: _scrollController,
              padding: const EdgeInsets.all(16),
              itemCount: _messages.length,
              itemBuilder: (context, index) {
                final message = _messages[index];
                return _MessageBubble(message: message);
              },
            ),
          ),

          // Typing indicator
          if (_isTyping)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              child: Row(
                children: [
                  CircleAvatar(
                    radius: 12,
                    backgroundImage: NetworkImage(
                      dealerInfo?['avatar'] ??
                          'https://ui-avatars.com/api/?name=Dealer',
                    ),
                  ),
                  const SizedBox(width: 8),
                  _TypingIndicator(),
                ],
              ),
            ),

          // Quick replies
          if (_quickReplies.isNotEmpty)
            SizedBox(
              height: 50,
              child: ListView.builder(
                scrollDirection: Axis.horizontal,
                padding:
                    const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                itemCount: _quickReplies.length,
                itemBuilder: (context, index) {
                  return Padding(
                    padding: const EdgeInsets.only(right: 8),
                    child: ActionChip(
                      label: Text(_quickReplies[index]),
                      onPressed: () {
                        _sendQuickReply(_quickReplies[index]);
                      },
                    ),
                  );
                },
              ),
            ),

          // Message input
          _buildMessageInput(),
        ],
      ),
    );
  }

  Widget _buildVehicleCard() {
    final theme = Theme.of(context);

    return Card(
      margin: const EdgeInsets.all(16),
      child: InkWell(
        onTap: () {
          // TODO: Navigate to vehicle details
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Ver detalles del vehículo')),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Row(
            children: [
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: Image.network(
                  _vehicleInfo['image'],
                  width: 80,
                  height: 60,
                  fit: BoxFit.cover,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      _vehicleInfo['title'],
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      '${_vehicleInfo['year']} • ${_vehicleInfo['mileage']}',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      _vehicleInfo['price'],
                      style: theme.textTheme.titleMedium?.copyWith(
                        color: theme.colorScheme.primary,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
              ),
              Icon(
                Icons.chevron_right,
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildMessageInput() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            offset: const Offset(0, -2),
            blurRadius: 8,
          ),
        ],
      ),
      child: Row(
        children: [
          IconButton(
            icon: const Icon(Icons.attach_file),
            onPressed: () => _showAttachmentOptions(context),
          ),
          Expanded(
            child: TextField(
              controller: _messageController,
              decoration: InputDecoration(
                hintText: 'Escribe un mensaje...',
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(24),
                  borderSide: BorderSide.none,
                ),
                filled: true,
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 10,
                ),
              ),
              onChanged: (value) {
                // Simulate typing indicator
                if (value.isNotEmpty && !_isTyping) {
                  setState(() => _isTyping = true);
                  Future.delayed(const Duration(seconds: 2), () {
                    if (mounted) setState(() => _isTyping = false);
                  });
                }
              },
            ),
          ),
          IconButton(
            icon: const Icon(Icons.send),
            onPressed: _sendMessage,
          ),
        ],
      ),
    );
  }

  void _sendMessage() {
    if (_messageController.text.trim().isEmpty) return;

    setState(() {
      _messages.add({
        'id': DateTime.now().millisecondsSinceEpoch.toString(),
        'text': _messageController.text,
        'sender': 'user',
        'timestamp': DateTime.now(),
        'status': 'pending',
      });
      _messageController.clear();
    });

    // Scroll to bottom
    Future.delayed(const Duration(milliseconds: 100), () {
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 300),
        curve: Curves.easeOut,
      );
    });

    // Simulate message delivered
    Future.delayed(const Duration(seconds: 1), () {
      if (mounted) {
        setState(() {
          _messages.last['status'] = 'delivered';
        });
      }
    });
  }

  void _sendQuickReply(String text) {
    _messageController.text = text;
    _sendMessage();
  }

  void _showAttachmentOptions(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.photo_camera),
              title: const Text('Cámara'),
              onTap: () async {
                final navigator = Navigator.of(context);
                final messenger = ScaffoldMessenger.of(context);
                navigator.pop();
                final image = await _imagePicker.pickImage(
                  source: ImageSource.camera,
                );
                if (image != null && mounted) {
                  // TODO: Handle image upload
                  messenger.showSnackBar(
                    const SnackBar(content: Text('Foto capturada')),
                  );
                }
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_library),
              title: const Text('Galería'),
              onTap: () async {
                final navigator = Navigator.of(context);
                final messenger = ScaffoldMessenger.of(context);
                navigator.pop();
                final image = await _imagePicker.pickImage(
                  source: ImageSource.gallery,
                );
                if (image != null && mounted) {
                  // TODO: Handle image upload
                  messenger.showSnackBar(
                    const SnackBar(content: Text('Imagen seleccionada')),
                  );
                }
              },
            ),
            ListTile(
              leading: const Icon(Icons.calendar_today),
              title: const Text('Agendar visita'),
              onTap: () {
                Navigator.pop(context);
                Navigator.pushNamed(context, '/messages/schedule-visit');
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showCallOptions(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.phone),
              title: const Text('Llamar directamente'),
              subtitle: const Text('+1 809-555-0100'),
              onTap: () {
                Navigator.pop(context);
                // TODO: Launch phone dialer
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Iniciando llamada...')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.video_call),
              title: const Text('Videollamada'),
              subtitle: const Text('Disponible en la app'),
              onTap: () {
                Navigator.pop(context);
                // TODO: Start video call
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Iniciando videollamada...')),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showChatOptions(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.volume_off),
              title: const Text('Silenciar conversación'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Conversación silenciada')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.search),
              title: const Text('Buscar en conversación'),
              onTap: () {
                Navigator.pop(context);
                Navigator.pushNamed(context, '/messages/conversation-search');
              },
            ),
            ListTile(
              leading: const Icon(Icons.archive),
              title: const Text('Archivar conversación'),
              onTap: () {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Conversación archivada')),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.delete, color: Colors.red),
              title: const Text('Eliminar conversación',
                  style: TextStyle(color: Colors.red)),
              onTap: () {
                Navigator.pop(context);
                // Show confirmation dialog
                showDialog(
                  context: context,
                  builder: (context) => AlertDialog(
                    title: const Text('Eliminar conversación'),
                    content: const Text(
                        '¿Estás seguro de que deseas eliminar esta conversación?'),
                    actions: [
                      TextButton(
                        onPressed: () => Navigator.pop(context),
                        child: const Text('Cancelar'),
                      ),
                      FilledButton(
                        onPressed: () {
                          Navigator.pop(context);
                          Navigator.pop(context);
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(
                                content: Text('Conversación eliminada')),
                          );
                        },
                        style: FilledButton.styleFrom(
                          backgroundColor: Colors.red,
                        ),
                        child: const Text('Eliminar'),
                      ),
                    ],
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}

class _MessageBubble extends StatelessWidget {
  final Map<String, dynamic> message;

  const _MessageBubble({required this.message});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isUser = message['sender'] == 'user';
    final status = message['status'] as String?;

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        mainAxisAlignment:
            isUser ? MainAxisAlignment.end : MainAxisAlignment.start,
        children: [
          if (!isUser) ...[
            const CircleAvatar(
              radius: 16,
              backgroundImage: NetworkImage(
                'https://ui-avatars.com/api/?name=Dealer',
              ),
            ),
            const SizedBox(width: 8),
          ],
          Flexible(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
              decoration: BoxDecoration(
                color: isUser
                    ? theme.colorScheme.primary
                    : theme.colorScheme.surfaceContainerHighest,
                borderRadius: BorderRadius.circular(16).copyWith(
                  topLeft: isUser ? const Radius.circular(16) : Radius.zero,
                  topRight: isUser ? Radius.zero : const Radius.circular(16),
                ),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    message['text'],
                    style: theme.textTheme.bodyLarge?.copyWith(
                      color: isUser
                          ? theme.colorScheme.onPrimary
                          : theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        _formatTime(message['timestamp'] as DateTime),
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: isUser
                              ? theme.colorScheme.onPrimary
                                  .withValues(alpha: 0.7)
                              : theme.colorScheme.onSurfaceVariant
                                  .withValues(alpha: 0.7),
                        ),
                      ),
                      if (isUser && status != null) ...[
                        const SizedBox(width: 4),
                        Icon(
                          status == 'read'
                              ? Icons.done_all
                              : status == 'delivered'
                                  ? Icons.done
                                  : Icons.schedule,
                          size: 16,
                          color: status == 'read'
                              ? theme.colorScheme.tertiary
                              : theme.colorScheme.onPrimary
                                  .withValues(alpha: 0.7),
                        ),
                      ],
                    ],
                  ),
                ],
              ),
            ),
          ),
          if (isUser) ...[
            const SizedBox(width: 8),
            const CircleAvatar(
              radius: 16,
              backgroundImage: NetworkImage(
                'https://ui-avatars.com/api/?name=User',
              ),
            ),
          ],
        ],
      ),
    );
  }

  String _formatTime(DateTime timestamp) {
    return '${timestamp.hour.toString().padLeft(2, '0')}:${timestamp.minute.toString().padLeft(2, '0')}';
  }
}

class _TypingIndicator extends StatefulWidget {
  @override
  State<_TypingIndicator> createState() => _TypingIndicatorState();
}

class _TypingIndicatorState extends State<_TypingIndicator>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 1500),
      vsync: this,
    )..repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      decoration: BoxDecoration(
        color: theme.colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: List.generate(3, (index) {
          return AnimatedBuilder(
            animation: _controller,
            builder: (context, child) {
              final delay = index * 0.2;
              final value = (_controller.value - delay).clamp(0.0, 1.0);
              final opacity = (Curves.easeInOut.transform(value) * 2 - 1).abs();

              return Padding(
                padding: const EdgeInsets.symmetric(horizontal: 2),
                child: Opacity(
                  opacity: opacity,
                  child: Container(
                    width: 8,
                    height: 8,
                    decoration: BoxDecoration(
                      color: theme.colorScheme.onSurfaceVariant,
                      shape: BoxShape.circle,
                    ),
                  ),
                ),
              );
            },
          );
        }),
      ),
    );
  }
}

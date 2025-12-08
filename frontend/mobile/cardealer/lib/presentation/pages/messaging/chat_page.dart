import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../../domain/entities/conversation.dart';
import '../../bloc/messaging/messaging_bloc.dart';
import '../../bloc/messaging/messaging_event.dart';
import '../../bloc/messaging/messaging_state.dart';
import 'widgets/message_bubble.dart';
import 'widgets/message_input.dart';

/// Chat page showing messages for a specific conversation
class ChatPage extends StatefulWidget {
  final Conversation conversation;

  const ChatPage({
    super.key,
    required this.conversation,
  });

  @override
  State<ChatPage> createState() => _ChatPageState();
}

class _ChatPageState extends State<ChatPage> {
  final ScrollController _scrollController = ScrollController();
  final TextEditingController _textController = TextEditingController();

  @override
  void initState() {
    super.initState();
    // Auto-scroll to bottom when new messages arrive
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController.dispose();
    _textController.dispose();
    super.dispose();
  }

  void _onScroll() {
    // TODO: Implement load more messages when scrolling to top
    if (_scrollController.position.pixels ==
        _scrollController.position.maxScrollExtent) {
      // Reached top, load more messages
    }
  }

  void _scrollToBottom() {
    if (_scrollController.hasClients) {
      _scrollController.animateTo(
        0, // ListView.builder with reverse: true
        duration: const Duration(milliseconds: 300),
        curve: Curves.easeOut,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<MessagingBloc>()
        ..add(LoadMessages(conversationId: widget.conversation.id))
        ..add(MarkAsRead(widget.conversation.id))
        ..add(ListenToMessagesEvent(widget.conversation.id)),
      child: Scaffold(
        appBar: AppBar(
          title: Row(
            children: [
              CircleAvatar(
                radius: 18,
                backgroundImage: widget.conversation.userAvatar != null
                    ? NetworkImage(widget.conversation.userAvatar!)
                    : null,
                child: widget.conversation.userAvatar == null
                    ? Text(
                        widget.conversation.userName
                            .substring(0, 1)
                            .toUpperCase(),
                        style: const TextStyle(fontSize: 16),
                      )
                    : null,
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      widget.conversation.userName,
                      style: const TextStyle(fontSize: 16),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    if (widget.conversation.isOnline)
                      const Text(
                        'En línea',
                        style: TextStyle(fontSize: 12, color: Colors.green),
                      )
                    else if (widget.conversation.lastSeenAt != null)
                      Text(
                        'Visto hace ${_getTimeAgo(widget.conversation.lastSeenAt!)}',
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.grey[400],
                        ),
                      ),
                  ],
                ),
              ),
            ],
          ),
          actions: [
            IconButton(
              icon: const Icon(Icons.call),
              onPressed: () {
                // TODO: Implement call functionality
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Llamar - Próximamente')),
                );
              },
            ),
            IconButton(
              icon: const Icon(Icons.more_vert),
              onPressed: () {
                _showMoreOptions(context);
              },
            ),
          ],
        ),
        body: Column(
          children: [
            // Vehicle card if conversation is about a vehicle
            if (widget.conversation.vehicleTitle != null)
              Container(
                padding: const EdgeInsets.all(12),
                color: Colors.grey[100],
                child: Row(
                  children: [
                    if (widget.conversation.vehicleImage != null)
                      ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: Image.network(
                          widget.conversation.vehicleImage!,
                          width: 60,
                          height: 60,
                          fit: BoxFit.cover,
                          errorBuilder: (context, error, stackTrace) {
                            return Container(
                              width: 60,
                              height: 60,
                              color: Colors.grey[300],
                              child: const Icon(Icons.directions_car),
                            );
                          },
                        ),
                      ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            widget.conversation.vehicleTitle!,
                            style: const TextStyle(
                              fontWeight: FontWeight.bold,
                            ),
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const SizedBox(height: 4),
                          TextButton(
                            onPressed: () {
                              Navigator.pushNamed(
                                context,
                                '/vehicle-detail',
                                arguments: widget.conversation.vehicleId,
                              );
                            },
                            style: TextButton.styleFrom(
                              padding: EdgeInsets.zero,
                              minimumSize: const Size(0, 30),
                            ),
                            child: const Text('Ver vehículo'),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            
            // Messages list
            Expanded(
              child: BlocConsumer<MessagingBloc, MessagingState>(
                listener: (context, state) {
                  if (state is MessageSent || state is MessageReceived) {
                    // Scroll to bottom when new message is sent/received
                    Future.delayed(const Duration(milliseconds: 100), () {
                      _scrollToBottom();
                    });
                  }

                  if (state is MessageSendError) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(state.message),
                        backgroundColor: Colors.red,
                      ),
                    );
                  }
                },
                builder: (context, state) {
                  if (state is MessagesLoading) {
                    return const Center(child: CircularProgressIndicator());
                  }

                  if (state is MessagesLoaded) {
                    if (state.messages.isEmpty) {
                      return Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(
                              Icons.chat_bubble_outline,
                              size: 80,
                              color: Colors.grey[300],
                            ),
                            const SizedBox(height: 16),
                            Text(
                              'Inicia la conversación',
                              style: TextStyle(
                                fontSize: 18,
                                color: Colors.grey[600],
                              ),
                            ),
                          ],
                        ),
                      );
                    }

                    return ListView.builder(
                      controller: _scrollController,
                      reverse: true, // Start from bottom
                      padding: const EdgeInsets.all(16),
                      itemCount: state.messages.length,
                      itemBuilder: (context, index) {
                        final message = state.messages[
                            state.messages.length - 1 - index
                        ]; // Reverse order
                        final isFromMe = message.isFromMe('current-user-id');
                        
                        // Show date separator if day changes
                        final showDateSeparator = index == 0 ||
                            !_isSameDay(
                              message.createdAt,
                              state.messages[state.messages.length - index]
                                  .createdAt,
                            );

                        return Column(
                          children: [
                            if (showDateSeparator)
                              Padding(
                                padding:
                                    const EdgeInsets.symmetric(vertical: 16),
                                child: Text(
                                  _getDateSeparatorText(message.createdAt),
                                  style: TextStyle(
                                    color: Colors.grey[600],
                                    fontSize: 12,
                                  ),
                                ),
                              ),
                            MessageBubble(
                              message: message,
                              isFromMe: isFromMe,
                            ),
                          ],
                        );
                      },
                    );
                  }

                  return const SizedBox.shrink();
                },
              ),
            ),

            // Message input
            MessageInput(
              controller: _textController,
              onSend: (message) {
                if (message.trim().isNotEmpty) {
                  context.read<MessagingBloc>().add(
                        SendMessageEvent(
                          conversationId: widget.conversation.id,
                          content: message,
                        ),
                      );
                  _textController.clear();
                }
              },
              onAttachment: () {
                // TODO: Implement attachment functionality
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Adjuntar archivos - Próximamente'),
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showMoreOptions(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.block),
              title: const Text('Bloquear usuario'),
              onTap: () {
                Navigator.pop(context);
                // TODO: Implement block user
              },
            ),
            ListTile(
              leading: const Icon(Icons.report),
              title: const Text('Reportar'),
              onTap: () {
                Navigator.pop(context);
                // TODO: Implement report
              },
            ),
            ListTile(
              leading: const Icon(Icons.delete, color: Colors.red),
              title: const Text(
                'Eliminar conversación',
                style: TextStyle(color: Colors.red),
              ),
              onTap: () {
                Navigator.pop(context);
                context
                    .read<MessagingBloc>()
                    .add(DeleteConversationEvent(widget.conversation.id));
                Navigator.pop(context); // Go back to conversations list
              },
            ),
          ],
        ),
      ),
    );
  }

  String _getTimeAgo(DateTime dateTime) {
    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inMinutes < 1) {
      return 'un momento';
    } else if (difference.inHours < 1) {
      return '${difference.inMinutes}m';
    } else if (difference.inDays < 1) {
      return '${difference.inHours}h';
    } else {
      return '${difference.inDays}d';
    }
  }

  String _getDateSeparatorText(DateTime date) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final messageDate = DateTime(date.year, date.month, date.day);

    if (messageDate == today) {
      return 'Hoy';
    } else if (messageDate == today.subtract(const Duration(days: 1))) {
      return 'Ayer';
    } else {
      return '${date.day}/${date.month}/${date.year}';
    }
  }

  bool _isSameDay(DateTime date1, DateTime date2) {
    return date1.year == date2.year &&
        date1.month == date2.month &&
        date1.day == date2.day;
  }
}

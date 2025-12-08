import 'package:flutter/material.dart';

/// Widget for message input with send button and attachment options
class MessageInput extends StatelessWidget {
  final TextEditingController controller;
  final Function(String) onSend;
  final VoidCallback onAttachment;

  const MessageInput({
    super.key,
    required this.controller,
    required this.onSend,
    required this.onAttachment,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.1),
            blurRadius: 4,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: SafeArea(
        top: false,
        child: Row(
          children: [
            // Attachment button
            IconButton(
              icon: const Icon(Icons.attach_file),
              color: Colors.grey[600],
              onPressed: onAttachment,
            ),

            // Text input
            Expanded(
              child: TextField(
                controller: controller,
                decoration: InputDecoration(
                  hintText: 'Escribe un mensaje...',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(24),
                    borderSide: BorderSide.none,
                  ),
                  filled: true,
                  fillColor: Colors.grey[100],
                  contentPadding: const EdgeInsets.symmetric(
                    horizontal: 20,
                    vertical: 10,
                  ),
                ),
                maxLines: null,
                textCapitalization: TextCapitalization.sentences,
                onSubmitted: (_) {
                  final message = controller.text;
                  if (message.trim().isNotEmpty) {
                    onSend(message);
                  }
                },
              ),
            ),

            const SizedBox(width: 8),

            // Send button
            CircleAvatar(
              backgroundColor: Theme.of(context).primaryColor,
              child: IconButton(
                icon: const Icon(Icons.send, color: Colors.white),
                onPressed: () {
                  final message = controller.text;
                  if (message.trim().isNotEmpty) {
                    onSend(message);
                  }
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}

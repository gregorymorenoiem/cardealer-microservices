import 'package:equatable/equatable.dart';
import 'message.dart';

/// Conversation entity representing a chat thread between two users
class Conversation extends Equatable {
  final String id;
  final String userId; // Other participant
  final String userName;
  final String? userAvatar;
  final String? vehicleId; // Optional: conversation about specific vehicle
  final String? vehicleTitle;
  final String? vehicleImage;
  final Message? lastMessage;
  final int unreadCount;
  final bool isOnline;
  final DateTime? lastSeenAt;
  final DateTime createdAt;
  final DateTime updatedAt;

  const Conversation({
    required this.id,
    required this.userId,
    required this.userName,
    this.userAvatar,
    this.vehicleId,
    this.vehicleTitle,
    this.vehicleImage,
    this.lastMessage,
    required this.unreadCount,
    required this.isOnline,
    this.lastSeenAt,
    required this.createdAt,
    required this.updatedAt,
  });

  /// Check if conversation has unread messages
  bool get hasUnread => unreadCount > 0;

  /// Get last message preview text
  String get lastMessagePreview {
    if (lastMessage == null) return 'Sin mensajes';
    
    switch (lastMessage!.type) {
      case MessageType.text:
        return lastMessage!.content;
      case MessageType.image:
        return '📷 Foto';
      case MessageType.video:
        return '🎥 Video';
      case MessageType.document:
        return '📄 Documento';
      case MessageType.location:
        return '📍 Ubicación';
      case MessageType.vehicleCard:
        return '🚗 Vehículo compartido';
    }
  }

  /// Get formatted time for last message
  String get formattedTime {
    if (lastMessage == null) return '';
    
    final now = DateTime.now();
    final messageDate = lastMessage!.createdAt;
    final difference = now.difference(messageDate);

    if (difference.inMinutes < 1) {
      return 'Ahora';
    } else if (difference.inHours < 1) {
      return '${difference.inMinutes}m';
    } else if (difference.inDays < 1) {
      return '${difference.inHours}h';
    } else if (difference.inDays < 7) {
      return '${difference.inDays}d';
    } else {
      return '${messageDate.day}/${messageDate.month}';
    }
  }

  /// Copy with method
  Conversation copyWith({
    String? id,
    String? userId,
    String? userName,
    String? userAvatar,
    String? vehicleId,
    String? vehicleTitle,
    String? vehicleImage,
    Message? lastMessage,
    int? unreadCount,
    bool? isOnline,
    DateTime? lastSeenAt,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return Conversation(
      id: id ?? this.id,
      userId: userId ?? this.userId,
      userName: userName ?? this.userName,
      userAvatar: userAvatar ?? this.userAvatar,
      vehicleId: vehicleId ?? this.vehicleId,
      vehicleTitle: vehicleTitle ?? this.vehicleTitle,
      vehicleImage: vehicleImage ?? this.vehicleImage,
      lastMessage: lastMessage ?? this.lastMessage,
      unreadCount: unreadCount ?? this.unreadCount,
      isOnline: isOnline ?? this.isOnline,
      lastSeenAt: lastSeenAt ?? this.lastSeenAt,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }

  @override
  List<Object?> get props => [
        id,
        userId,
        userName,
        userAvatar,
        vehicleId,
        vehicleTitle,
        vehicleImage,
        lastMessage,
        unreadCount,
        isOnline,
        lastSeenAt,
        createdAt,
        updatedAt,
      ];
}

import 'package:equatable/equatable.dart';

/// Status of a lead in the CRM pipeline
enum LeadStatus {
  nuevo,
  contactado,
  calificado,
  negociacion,
  ganado,
  perdido,
}

/// Priority level of a lead
enum LeadPriority {
  baja,
  media,
  alta,
  urgente,
}

/// Source where the lead came from
enum LeadSource {
  web,
  mobile,
  llamada,
  email,
  referido,
  eventoLocal,
  redesSociales,
}

/// Represents a sales lead for a dealer
class Lead extends Equatable {
  final String id;
  final String dealerId;
  final String listingId;
  
  // Customer info
  final String userId;
  final String userName;
  final String userEmail;
  final String? userPhone;
  final String? userAvatar;
  
  // Listing info
  final String vehicleTitle;
  final String? vehicleImage;
  final double vehiclePrice;
  
  // Lead details
  final LeadStatus status;
  final LeadPriority priority;
  final LeadSource source;
  final String? message;
  final String? notes;
  
  // Financial info
  final double? budget;
  final bool? needsFinancing;
  final bool? hasTradeIn;
  final String? tradeInDetails;
  
  // Engagement
  final int messageCount;
  final int callCount;
  final DateTime? lastContactedAt;
  final DateTime? nextFollowUpAt;
  final String? assignedTo;
  
  // Conversion tracking
  final double? offeredPrice;
  final String? lostReason;
  final DateTime? convertedAt;
  final double? soldPrice;
  
  // Timestamps
  final DateTime createdAt;
  final DateTime updatedAt;

  const Lead({
    required this.id,
    required this.dealerId,
    required this.listingId,
    required this.userId,
    required this.userName,
    required this.userEmail,
    this.userPhone,
    this.userAvatar,
    required this.vehicleTitle,
    this.vehicleImage,
    required this.vehiclePrice,
    required this.status,
    required this.priority,
    required this.source,
    this.message,
    this.notes,
    this.budget,
    this.needsFinancing,
    this.hasTradeIn,
    this.tradeInDetails,
    required this.messageCount,
    required this.callCount,
    this.lastContactedAt,
    this.nextFollowUpAt,
    this.assignedTo,
    this.offeredPrice,
    this.lostReason,
    this.convertedAt,
    this.soldPrice,
    required this.createdAt,
    required this.updatedAt,
  });

  @override
  List<Object?> get props => [
        id,
        dealerId,
        listingId,
        userId,
        userName,
        userEmail,
        userPhone,
        userAvatar,
        vehicleTitle,
        vehicleImage,
        vehiclePrice,
        status,
        priority,
        source,
        message,
        notes,
        budget,
        needsFinancing,
        hasTradeIn,
        tradeInDetails,
        messageCount,
        callCount,
        lastContactedAt,
        nextFollowUpAt,
        assignedTo,
        offeredPrice,
        lostReason,
        convertedAt,
        soldPrice,
        createdAt,
        updatedAt,
      ];

  // Helper getters
  bool get isNuevo => status == LeadStatus.nuevo;
  bool get isContactado => status == LeadStatus.contactado;
  bool get isCalificado => status == LeadStatus.calificado;
  bool get isNegociacion => status == LeadStatus.negociacion;
  bool get isGanado => status == LeadStatus.ganado;
  bool get isPerdido => status == LeadStatus.perdido;
  
  bool get isActive => !isGanado && !isPerdido;
  bool get isClosed => isGanado || isPerdido;
  
  bool get isHighPriority => priority == LeadPriority.alta || priority == LeadPriority.urgente;
  
  bool get needsFollowUp {
    if (nextFollowUpAt == null) return false;
    return nextFollowUpAt!.isBefore(DateTime.now()) || 
           nextFollowUpAt!.isAtSameMomentAs(DateTime.now());
  }
  
  bool get isOverdue {
    if (nextFollowUpAt == null) return false;
    return nextFollowUpAt!.isBefore(DateTime.now());
  }
  
  int get daysSinceLastContact {
    final lastContact = lastContactedAt ?? createdAt;
    return DateTime.now().difference(lastContact).inDays;
  }
  
  int get daysOld {
    return DateTime.now().difference(createdAt).inDays;
  }
  
  bool get isStale => daysSinceLastContact > 7 && isActive;
  
  bool get hasEngagement => messageCount > 0 || callCount > 0;
  
  String get statusText {
    switch (status) {
      case LeadStatus.nuevo:
        return 'Nuevo';
      case LeadStatus.contactado:
        return 'Contactado';
      case LeadStatus.calificado:
        return 'Calificado';
      case LeadStatus.negociacion:
        return 'Negociación';
      case LeadStatus.ganado:
        return 'Ganado';
      case LeadStatus.perdido:
        return 'Perdido';
    }
  }
  
  String get priorityText {
    switch (priority) {
      case LeadPriority.baja:
        return 'Baja';
      case LeadPriority.media:
        return 'Media';
      case LeadPriority.alta:
        return 'Alta';
      case LeadPriority.urgente:
        return 'Urgente';
    }
  }
  
  String get sourceText {
    switch (source) {
      case LeadSource.web:
        return 'Web';
      case LeadSource.mobile:
        return 'Móvil';
      case LeadSource.llamada:
        return 'Llamada';
      case LeadSource.email:
        return 'Email';
      case LeadSource.referido:
        return 'Referido';
      case LeadSource.eventoLocal:
        return 'Evento Local';
      case LeadSource.redesSociales:
        return 'Redes Sociales';
    }
  }

  Lead copyWith({
    String? id,
    String? dealerId,
    String? listingId,
    String? userId,
    String? userName,
    String? userEmail,
    String? userPhone,
    String? userAvatar,
    String? vehicleTitle,
    String? vehicleImage,
    double? vehiclePrice,
    LeadStatus? status,
    LeadPriority? priority,
    LeadSource? source,
    String? message,
    String? notes,
    double? budget,
    bool? needsFinancing,
    bool? hasTradeIn,
    String? tradeInDetails,
    int? messageCount,
    int? callCount,
    DateTime? lastContactedAt,
    DateTime? nextFollowUpAt,
    String? assignedTo,
    double? offeredPrice,
    String? lostReason,
    DateTime? convertedAt,
    double? soldPrice,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return Lead(
      id: id ?? this.id,
      dealerId: dealerId ?? this.dealerId,
      listingId: listingId ?? this.listingId,
      userId: userId ?? this.userId,
      userName: userName ?? this.userName,
      userEmail: userEmail ?? this.userEmail,
      userPhone: userPhone ?? this.userPhone,
      userAvatar: userAvatar ?? this.userAvatar,
      vehicleTitle: vehicleTitle ?? this.vehicleTitle,
      vehicleImage: vehicleImage ?? this.vehicleImage,
      vehiclePrice: vehiclePrice ?? this.vehiclePrice,
      status: status ?? this.status,
      priority: priority ?? this.priority,
      source: source ?? this.source,
      message: message ?? this.message,
      notes: notes ?? this.notes,
      budget: budget ?? this.budget,
      needsFinancing: needsFinancing ?? this.needsFinancing,
      hasTradeIn: hasTradeIn ?? this.hasTradeIn,
      tradeInDetails: tradeInDetails ?? this.tradeInDetails,
      messageCount: messageCount ?? this.messageCount,
      callCount: callCount ?? this.callCount,
      lastContactedAt: lastContactedAt ?? this.lastContactedAt,
      nextFollowUpAt: nextFollowUpAt ?? this.nextFollowUpAt,
      assignedTo: assignedTo ?? this.assignedTo,
      offeredPrice: offeredPrice ?? this.offeredPrice,
      lostReason: lostReason ?? this.lostReason,
      convertedAt: convertedAt ?? this.convertedAt,
      soldPrice: soldPrice ?? this.soldPrice,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }
}

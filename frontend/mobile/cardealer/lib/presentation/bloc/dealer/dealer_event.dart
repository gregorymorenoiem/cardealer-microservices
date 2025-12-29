import 'package:equatable/equatable.dart';
import '../../../domain/entities/dealer_listing.dart';
import '../../../domain/entities/lead.dart';

abstract class DealerEvent extends Equatable {
  @override
  List<Object?> get props => [];
}

// Dashboard events
class LoadDashboard extends DealerEvent {
  final String dealerId;

  LoadDashboard(this.dealerId);

  @override
  List<Object> get props => [dealerId];
}

class RefreshDashboard extends DealerEvent {
  final String dealerId;

  RefreshDashboard(this.dealerId);

  @override
  List<Object> get props => [dealerId];
}

// Listings events
class LoadListings extends DealerEvent {
  final String dealerId;
  final ListingStatus? status;
  final String? searchQuery;

  LoadListings({
    required this.dealerId,
    this.status,
    this.searchQuery,
  });

  @override
  List<Object?> get props => [dealerId, status, searchQuery];
}

class FilterListings extends DealerEvent {
  final String dealerId;
  final ListingStatus? status;

  FilterListings({
    required this.dealerId,
    this.status,
  });

  @override
  List<Object?> get props => [dealerId, status];
}

class SearchListings extends DealerEvent {
  final String dealerId;
  final String query;

  SearchListings({
    required this.dealerId,
    required this.query,
  });

  @override
  List<Object> get props => [dealerId, query];
}

class LoadListingDetail extends DealerEvent {
  final String listingId;

  LoadListingDetail(this.listingId);

  @override
  List<Object> get props => [listingId];
}

// Listing operations events
class CreateListingEvent extends DealerEvent {
  final DealerListing listing;

  CreateListingEvent(this.listing);

  @override
  List<Object> get props => [listing];
}

class UpdateListingEvent extends DealerEvent {
  final DealerListing listing;

  UpdateListingEvent(this.listing);

  @override
  List<Object> get props => [listing];
}

class DeleteListingEvent extends DealerEvent {
  final String listingId;

  DeleteListingEvent(this.listingId);

  @override
  List<Object> get props => [listingId];
}

class PublishListingEvent extends DealerEvent {
  final String listingId;

  PublishListingEvent(this.listingId);

  @override
  List<Object> get props => [listingId];
}

class MarkListingAsSoldEvent extends DealerEvent {
  final String listingId;
  final double soldPrice;

  MarkListingAsSoldEvent({
    required this.listingId,
    required this.soldPrice,
  });

  @override
  List<Object> get props => [listingId, soldPrice];
}

// Leads events
class LoadLeads extends DealerEvent {
  final String dealerId;
  final LeadStatus? status;
  final LeadPriority? priority;
  final String? searchQuery;

  LoadLeads({
    required this.dealerId,
    this.status,
    this.priority,
    this.searchQuery,
  });

  @override
  List<Object?> get props => [dealerId, status, priority, searchQuery];
}

class FilterLeads extends DealerEvent {
  final String dealerId;
  final LeadStatus? status;
  final LeadPriority? priority;

  FilterLeads({
    required this.dealerId,
    this.status,
    this.priority,
  });

  @override
  List<Object?> get props => [dealerId, status, priority];
}

class SearchLeads extends DealerEvent {
  final String dealerId;
  final String query;

  SearchLeads({
    required this.dealerId,
    required this.query,
  });

  @override
  List<Object> get props => [dealerId, query];
}

class LoadLeadDetail extends DealerEvent {
  final String leadId;

  LoadLeadDetail(this.leadId);

  @override
  List<Object> get props => [leadId];
}

// Lead operations events
class UpdateLeadStatusEvent extends DealerEvent {
  final String leadId;
  final LeadStatus status;

  UpdateLeadStatusEvent({
    required this.leadId,
    required this.status,
  });

  @override
  List<Object> get props => [leadId, status];
}

class UpdateLeadNotesEvent extends DealerEvent {
  final String leadId;
  final String notes;

  UpdateLeadNotesEvent({
    required this.leadId,
    required this.notes,
  });

  @override
  List<Object> get props => [leadId, notes];
}

class ScheduleFollowUpEvent extends DealerEvent {
  final String leadId;
  final DateTime followUpDate;

  ScheduleFollowUpEvent({
    required this.leadId,
    required this.followUpDate,
  });

  @override
  List<Object> get props => [leadId, followUpDate];
}

class ConvertLeadEvent extends DealerEvent {
  final String leadId;
  final double soldPrice;

  ConvertLeadEvent({
    required this.leadId,
    required this.soldPrice,
  });

  @override
  List<Object> get props => [leadId, soldPrice];
}

class MarkLeadAsLostEvent extends DealerEvent {
  final String leadId;
  final String reason;

  MarkLeadAsLostEvent({
    required this.leadId,
    required this.reason,
  });

  @override
  List<Object> get props => [leadId, reason];
}

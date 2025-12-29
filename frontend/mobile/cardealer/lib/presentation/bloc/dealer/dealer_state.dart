import 'package:equatable/equatable.dart';
import '../../../domain/entities/dealer_listing.dart';
import '../../../domain/entities/dealer_stats.dart';
import '../../../domain/entities/lead.dart';

abstract class DealerState extends Equatable {
  @override
  List<Object?> get props => [];
}

// Initial state
class DealerInitial extends DealerState {}

// Dashboard states
class DashboardLoading extends DealerState {}

class DashboardLoaded extends DealerState {
  final DealerStats stats;

  DashboardLoaded(this.stats);

  @override
  List<Object> get props => [stats];
}

class DashboardError extends DealerState {
  final String message;

  DashboardError(this.message);

  @override
  List<Object> get props => [message];
}

// Listings states
class ListingsLoading extends DealerState {}

class ListingsLoaded extends DealerState {
  final List<DealerListing> listings;
  final ListingStatus? currentFilter;

  ListingsLoaded(this.listings, {this.currentFilter});

  @override
  List<Object?> get props => [listings, currentFilter];
}

class ListingsEmpty extends DealerState {}

class ListingsError extends DealerState {
  final String message;

  ListingsError(this.message);

  @override
  List<Object> get props => [message];
}

// Listing detail states
class ListingDetailLoading extends DealerState {}

class ListingDetailLoaded extends DealerState {
  final DealerListing listing;

  ListingDetailLoaded(this.listing);

  @override
  List<Object> get props => [listing];
}

class ListingDetailError extends DealerState {
  final String message;

  ListingDetailError(this.message);

  @override
  List<Object> get props => [message];
}

// Listing operations states
class ListingCreating extends DealerState {}

class ListingCreated extends DealerState {
  final DealerListing listing;

  ListingCreated(this.listing);

  @override
  List<Object> get props => [listing];
}

class ListingUpdating extends DealerState {}

class ListingUpdated extends DealerState {
  final DealerListing listing;

  ListingUpdated(this.listing);

  @override
  List<Object> get props => [listing];
}

class ListingDeleting extends DealerState {}

class ListingDeleted extends DealerState {
  final String listingId;

  ListingDeleted(this.listingId);

  @override
  List<Object> get props => [listingId];
}

class ListingOperationError extends DealerState {
  final String message;

  ListingOperationError(this.message);

  @override
  List<Object> get props => [message];
}

// Leads states
class LeadsLoading extends DealerState {}

class LeadsLoaded extends DealerState {
  final List<Lead> leads;
  final LeadStatus? currentFilter;
  final LeadPriority? priorityFilter;

  LeadsLoaded(
    this.leads, {
    this.currentFilter,
    this.priorityFilter,
  });

  @override
  List<Object?> get props => [leads, currentFilter, priorityFilter];
}

class LeadsEmpty extends DealerState {}

class LeadsError extends DealerState {
  final String message;

  LeadsError(this.message);

  @override
  List<Object> get props => [message];
}

// Lead detail states
class LeadDetailLoading extends DealerState {}

class LeadDetailLoaded extends DealerState {
  final Lead lead;

  LeadDetailLoaded(this.lead);

  @override
  List<Object> get props => [lead];
}

class LeadDetailError extends DealerState {
  final String message;

  LeadDetailError(this.message);

  @override
  List<Object> get props => [message];
}

// Lead operations states
class LeadUpdating extends DealerState {}

class LeadUpdated extends DealerState {
  final Lead lead;

  LeadUpdated(this.lead);

  @override
  List<Object> get props => [lead];
}

class LeadOperationError extends DealerState {
  final String message;

  LeadOperationError(this.message);

  @override
  List<Object> get props => [message];
}

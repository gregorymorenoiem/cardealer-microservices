import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/repositories/dealer_repository.dart';
import '../../../domain/usecases/dealer/get_dealer_stats.dart';
import '../../../domain/usecases/dealer/get_leads.dart';
import '../../../domain/usecases/dealer/get_listings.dart';
import '../../../domain/usecases/dealer/manage_listing.dart';
import '../../../domain/usecases/dealer/update_lead.dart';
import 'dealer_event.dart';
import 'dealer_state.dart';

class DealerBloc extends Bloc<DealerEvent, DealerState> {
  final GetDealerStats getDealerStats;
  final GetListings getListings;
  final CreateListing createListing;
  final UpdateListing updateListing;
  final DeleteListing deleteListing;
  final PublishListing publishListing;
  final GetLeads getLeads;
  final UpdateLeadStatus updateLeadStatus;
  final UpdateLeadNotes updateLeadNotes;
  final ScheduleFollowUp scheduleFollowUp;
  final DealerRepository dealerRepository;

  DealerBloc({
    required this.getDealerStats,
    required this.getListings,
    required this.createListing,
    required this.updateListing,
    required this.deleteListing,
    required this.publishListing,
    required this.getLeads,
    required this.updateLeadStatus,
    required this.updateLeadNotes,
    required this.scheduleFollowUp,
    required this.dealerRepository,
  }) : super(DealerInitial()) {
    on<LoadDashboard>(_onLoadDashboard);
    on<RefreshDashboard>(_onRefreshDashboard);
    on<LoadListings>(_onLoadListings);
    on<FilterListings>(_onFilterListings);
    on<SearchListings>(_onSearchListings);
    on<LoadListingDetail>(_onLoadListingDetail);
    on<CreateListingEvent>(_onCreateListing);
    on<UpdateListingEvent>(_onUpdateListing);
    on<DeleteListingEvent>(_onDeleteListing);
    on<PublishListingEvent>(_onPublishListing);
    on<MarkListingAsSoldEvent>(_onMarkListingAsSold);
    on<LoadLeads>(_onLoadLeads);
    on<FilterLeads>(_onFilterLeads);
    on<SearchLeads>(_onSearchLeads);
    on<LoadLeadDetail>(_onLoadLeadDetail);
    on<UpdateLeadStatusEvent>(_onUpdateLeadStatus);
    on<UpdateLeadNotesEvent>(_onUpdateLeadNotes);
    on<ScheduleFollowUpEvent>(_onScheduleFollowUp);
    on<ConvertLeadEvent>(_onConvertLead);
    on<MarkLeadAsLostEvent>(_onMarkLeadAsLost);
  }

  // Dashboard handlers
  Future<void> _onLoadDashboard(
    LoadDashboard event,
    Emitter<DealerState> emit,
  ) async {
    emit(DashboardLoading());

    final result = await getDealerStats(event.dealerId);

    result.fold(
      (failure) => emit(DashboardError(failure.message)),
      (stats) => emit(DashboardLoaded(stats)),
    );
  }

  Future<void> _onRefreshDashboard(
    RefreshDashboard event,
    Emitter<DealerState> emit,
  ) async {
    final result = await getDealerStats(event.dealerId);

    result.fold(
      (failure) => emit(DashboardError(failure.message)),
      (stats) => emit(DashboardLoaded(stats)),
    );
  }

  // Listings handlers
  Future<void> _onLoadListings(
    LoadListings event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingsLoading());

    final result = await getListings(GetListingsParams(
      dealerId: event.dealerId,
      status: event.status,
      searchQuery: event.searchQuery,
    ));

    result.fold(
      (failure) => emit(ListingsError(failure.message)),
      (listings) {
        if (listings.isEmpty) {
          emit(ListingsEmpty());
        } else {
          emit(ListingsLoaded(listings, currentFilter: event.status));
        }
      },
    );
  }

  Future<void> _onFilterListings(
    FilterListings event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingsLoading());

    final result = await getListings(GetListingsParams(
      dealerId: event.dealerId,
      status: event.status,
    ));

    result.fold(
      (failure) => emit(ListingsError(failure.message)),
      (listings) {
        if (listings.isEmpty) {
          emit(ListingsEmpty());
        } else {
          emit(ListingsLoaded(listings, currentFilter: event.status));
        }
      },
    );
  }

  Future<void> _onSearchListings(
    SearchListings event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingsLoading());

    final result = await getListings(GetListingsParams(
      dealerId: event.dealerId,
      searchQuery: event.query,
    ));

    result.fold(
      (failure) => emit(ListingsError(failure.message)),
      (listings) {
        if (listings.isEmpty) {
          emit(ListingsEmpty());
        } else {
          emit(ListingsLoaded(listings));
        }
      },
    );
  }

  Future<void> _onLoadListingDetail(
    LoadListingDetail event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingDetailLoading());

    final result = await dealerRepository.getListingById(event.listingId);

    result.fold(
      (failure) => emit(ListingDetailError(failure.message)),
      (listing) => emit(ListingDetailLoaded(listing)),
    );
  }

  // Listing operations handlers
  Future<void> _onCreateListing(
    CreateListingEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingCreating());

    final result = await createListing(event.listing);

    result.fold(
      (failure) => emit(ListingOperationError(failure.message)),
      (listing) => emit(ListingCreated(listing)),
    );
  }

  Future<void> _onUpdateListing(
    UpdateListingEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingUpdating());

    final result = await updateListing(event.listing);

    result.fold(
      (failure) => emit(ListingOperationError(failure.message)),
      (listing) => emit(ListingUpdated(listing)),
    );
  }

  Future<void> _onDeleteListing(
    DeleteListingEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingDeleting());

    final result = await deleteListing(event.listingId);

    result.fold(
      (failure) => emit(ListingOperationError(failure.message)),
      (_) => emit(ListingDeleted(event.listingId)),
    );
  }

  Future<void> _onPublishListing(
    PublishListingEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingUpdating());

    final result = await publishListing(event.listingId);

    result.fold(
      (failure) => emit(ListingOperationError(failure.message)),
      (listing) => emit(ListingUpdated(listing)),
    );
  }

  Future<void> _onMarkListingAsSold(
    MarkListingAsSoldEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(ListingUpdating());

    final result = await dealerRepository.markListingAsSold(
      listingId: event.listingId,
      soldPrice: event.soldPrice,
    );

    result.fold(
      (failure) => emit(ListingOperationError(failure.message)),
      (listing) => emit(ListingUpdated(listing)),
    );
  }

  // Leads handlers
  Future<void> _onLoadLeads(
    LoadLeads event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadsLoading());

    final result = await getLeads(GetLeadsParams(
      dealerId: event.dealerId,
      status: event.status,
      priority: event.priority,
      searchQuery: event.searchQuery,
    ));

    result.fold(
      (failure) => emit(LeadsError(failure.message)),
      (leads) {
        if (leads.isEmpty) {
          emit(LeadsEmpty());
        } else {
          emit(LeadsLoaded(
            leads,
            currentFilter: event.status,
            priorityFilter: event.priority,
          ));
        }
      },
    );
  }

  Future<void> _onFilterLeads(
    FilterLeads event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadsLoading());

    final result = await getLeads(GetLeadsParams(
      dealerId: event.dealerId,
      status: event.status,
      priority: event.priority,
    ));

    result.fold(
      (failure) => emit(LeadsError(failure.message)),
      (leads) {
        if (leads.isEmpty) {
          emit(LeadsEmpty());
        } else {
          emit(LeadsLoaded(
            leads,
            currentFilter: event.status,
            priorityFilter: event.priority,
          ));
        }
      },
    );
  }

  Future<void> _onSearchLeads(
    SearchLeads event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadsLoading());

    final result = await getLeads(GetLeadsParams(
      dealerId: event.dealerId,
      searchQuery: event.query,
    ));

    result.fold(
      (failure) => emit(LeadsError(failure.message)),
      (leads) {
        if (leads.isEmpty) {
          emit(LeadsEmpty());
        } else {
          emit(LeadsLoaded(leads));
        }
      },
    );
  }

  Future<void> _onLoadLeadDetail(
    LoadLeadDetail event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadDetailLoading());

    final result = await dealerRepository.getLeadById(event.leadId);

    result.fold(
      (failure) => emit(LeadDetailError(failure.message)),
      (lead) => emit(LeadDetailLoaded(lead)),
    );
  }

  // Lead operations handlers
  Future<void> _onUpdateLeadStatus(
    UpdateLeadStatusEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadUpdating());

    final result = await updateLeadStatus(UpdateLeadStatusParams(
      leadId: event.leadId,
      status: event.status,
    ));

    result.fold(
      (failure) => emit(LeadOperationError(failure.message)),
      (lead) => emit(LeadUpdated(lead)),
    );
  }

  Future<void> _onUpdateLeadNotes(
    UpdateLeadNotesEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadUpdating());

    final result = await updateLeadNotes(UpdateLeadNotesParams(
      leadId: event.leadId,
      notes: event.notes,
    ));

    result.fold(
      (failure) => emit(LeadOperationError(failure.message)),
      (lead) => emit(LeadUpdated(lead)),
    );
  }

  Future<void> _onScheduleFollowUp(
    ScheduleFollowUpEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadUpdating());

    final result = await scheduleFollowUp(ScheduleFollowUpParams(
      leadId: event.leadId,
      followUpDate: event.followUpDate,
    ));

    result.fold(
      (failure) => emit(LeadOperationError(failure.message)),
      (lead) => emit(LeadUpdated(lead)),
    );
  }

  Future<void> _onConvertLead(
    ConvertLeadEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadUpdating());

    final result = await dealerRepository.convertLead(
      leadId: event.leadId,
      soldPrice: event.soldPrice,
    );

    result.fold(
      (failure) => emit(LeadOperationError(failure.message)),
      (lead) => emit(LeadUpdated(lead)),
    );
  }

  Future<void> _onMarkLeadAsLost(
    MarkLeadAsLostEvent event,
    Emitter<DealerState> emit,
  ) async {
    emit(LeadUpdating());

    final result = await dealerRepository.markLeadAsLost(
      leadId: event.leadId,
      reason: event.reason,
    );

    result.fold(
      (failure) => emit(LeadOperationError(failure.message)),
      (lead) => emit(LeadUpdated(lead)),
    );
  }
}

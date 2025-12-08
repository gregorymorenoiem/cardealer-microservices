import 'package:get_it/get_it.dart';
import 'package:injectable/injectable.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'injection.config.dart';
import '../../data/datasources/mock/mock_vehicle_datasource.dart';
import '../../data/repositories/vehicle_repository_impl.dart';
import '../../data/repositories/mock_messaging_repository.dart';
import '../../data/repositories/mock_dealer_repository.dart';
import '../../domain/repositories/vehicle_repository.dart';
import '../../domain/repositories/messaging_repository.dart';
import '../../domain/repositories/dealer_repository.dart';
import '../../domain/usecases/vehicles/search_vehicles.dart';
import '../../domain/usecases/vehicles/filter_vehicles.dart';
import '../../domain/usecases/vehicles/get_filter_suggestions.dart';
import '../../domain/usecases/vehicles/get_vehicle_detail.dart';
import '../../domain/usecases/vehicles/contact_seller.dart';
import '../../domain/usecases/vehicles/get_similar_vehicles.dart';
import '../../domain/usecases/favorites/toggle_favorite.dart';
import '../../domain/usecases/profile/get_user_profile.dart';
import '../../domain/usecases/profile/update_profile.dart';
import '../../domain/usecases/favorites/get_favorites.dart';
import '../../domain/usecases/favorites/remove_favorite.dart';
import '../../domain/usecases/search/get_search_history.dart';
import '../../domain/usecases/messaging/get_conversations.dart';
import '../../domain/usecases/messaging/get_messages.dart';
import '../../domain/usecases/messaging/send_message.dart';
import '../../domain/usecases/messaging/get_or_create_conversation.dart';
import '../../domain/usecases/messaging/mark_conversation_as_read.dart';
import '../../domain/usecases/dealer/get_dealer_stats.dart';
import '../../domain/usecases/dealer/get_listings.dart';
import '../../domain/usecases/dealer/get_leads.dart';
import '../../domain/usecases/dealer/manage_listing.dart';
import '../../domain/usecases/dealer/update_lead.dart';
import '../../domain/repositories/auth_repository.dart';
import '../../presentation/bloc/vehicles/vehicles_bloc.dart';
import '../../presentation/bloc/filter/filter_bloc.dart';
import '../../presentation/bloc/search/search_bloc.dart';
import '../../presentation/bloc/vehicle_detail/vehicle_detail_bloc.dart';
import '../../presentation/bloc/profile/profile_bloc.dart';
import '../../presentation/bloc/favorites/favorites_bloc.dart';
import '../../presentation/bloc/messaging/messaging_bloc.dart';
import '../../presentation/bloc/dealer/dealer_bloc.dart';
import '../network/network_info.dart';

final getIt = GetIt.instance;

@InjectableInit(
  initializerName: 'init',
  preferRelativeImports: true,
  asExtension: true,
)
Future<void> configureDependencies() async {
  await getIt.init();

  // Register SharedPreferences
  final sharedPreferences = await SharedPreferences.getInstance();
  getIt.registerSingleton<SharedPreferences>(sharedPreferences);

  // Register network info
  getIt.registerLazySingleton<NetworkInfo>(() => NetworkInfoImpl());

  // Register data sources
  getIt.registerLazySingleton<MockVehicleDataSource>(
      () => MockVehicleDataSource());

  // Register repositories
  getIt.registerLazySingleton<VehicleRepository>(
    () => VehicleRepositoryImpl(
      mockDataSource: getIt<MockVehicleDataSource>(),
      networkInfo: getIt<NetworkInfo>(),
    ),
  );

  // Register messaging repository (mock for now)
  getIt.registerLazySingleton<MessagingRepository>(
    () => MockMessagingRepository(),
  );

  // Register dealer repository (mock for now)
  getIt.registerLazySingleton<DealerRepository>(
    () => MockDealerRepository(),
  );

  // Register use cases
  getIt.registerLazySingleton<SearchVehicles>(
    () => SearchVehicles(getIt<VehicleRepository>()),
  );
  getIt.registerLazySingleton<FilterVehicles>(
    () => FilterVehicles(getIt<VehicleRepository>()),
  );
  getIt.registerLazySingleton<GetFilterSuggestions>(
    () => GetFilterSuggestions(getIt<VehicleRepository>()),
  );
  getIt.registerLazySingleton<GetVehicleDetail>(
    () => GetVehicleDetail(getIt<VehicleRepository>()),
  );
  getIt.registerLazySingleton<ContactSeller>(
    () => ContactSeller(getIt<VehicleRepository>()),
  );
  getIt.registerLazySingleton<GetSimilarVehicles>(
    () => GetSimilarVehicles(getIt<VehicleRepository>()),
  );
  getIt.registerLazySingleton<ToggleFavorite>(
    () => ToggleFavorite(),
  );
  getIt.registerLazySingleton<GetUserProfile>(
    () => GetUserProfile(getIt<AuthRepository>()),
  );
  getIt.registerLazySingleton<UpdateProfile>(
    () => UpdateProfile(getIt<AuthRepository>()),
  );
  getIt.registerLazySingleton<GetFavorites>(
    () => GetFavorites(getIt<VehicleRepository>(), getIt<SharedPreferences>()),
  );
  getIt.registerLazySingleton<RemoveFavorite>(
    () => RemoveFavorite(getIt<SharedPreferences>()),
  );
  getIt.registerLazySingleton<GetSearchHistory>(
    () => GetSearchHistory(getIt<SharedPreferences>()),
  );

  // Messaging use cases
  getIt.registerLazySingleton<GetConversations>(
    () => GetConversations(getIt<MessagingRepository>()),
  );
  getIt.registerLazySingleton<GetMessages>(
    () => GetMessages(getIt<MessagingRepository>()),
  );
  getIt.registerLazySingleton<SendMessage>(
    () => SendMessage(getIt<MessagingRepository>()),
  );
  getIt.registerLazySingleton<GetOrCreateConversation>(
    () => GetOrCreateConversation(getIt<MessagingRepository>()),
  );
  getIt.registerLazySingleton<MarkConversationAsRead>(
    () => MarkConversationAsRead(getIt<MessagingRepository>()),
  );

  // Dealer use cases
  getIt.registerLazySingleton<GetDealerStats>(
    () => GetDealerStats(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<GetListings>(
    () => GetListings(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<GetLeads>(
    () => GetLeads(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<CreateListing>(
    () => CreateListing(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<UpdateListing>(
    () => UpdateListing(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<DeleteListing>(
    () => DeleteListing(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<PublishListing>(
    () => PublishListing(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<UpdateLeadStatus>(
    () => UpdateLeadStatus(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<UpdateLeadNotes>(
    () => UpdateLeadNotes(getIt<DealerRepository>()),
  );
  getIt.registerLazySingleton<ScheduleFollowUp>(
    () => ScheduleFollowUp(getIt<DealerRepository>()),
  );

  // Register BLoCs
  getIt.registerFactory<VehiclesBloc>(
    () => VehiclesBloc(repository: getIt<VehicleRepository>()),
  );
  getIt.registerFactory<FilterBloc>(
    () => FilterBloc(
      filterVehicles: getIt<FilterVehicles>(),
      getFilterSuggestions: getIt<GetFilterSuggestions>(),
    ),
  );
  getIt.registerFactory<SearchBloc>(
    () => SearchBloc(
      searchVehicles: getIt<SearchVehicles>(),
      sharedPreferences: getIt<SharedPreferences>(),
    ),
  );
  getIt.registerFactory<VehicleDetailBloc>(
    () => VehicleDetailBloc(
      getVehicleDetail: getIt<GetVehicleDetail>(),
      contactSeller: getIt<ContactSeller>(),
      getSimilarVehicles: getIt<GetSimilarVehicles>(),
      toggleFavorite: getIt<ToggleFavorite>(),
      sharedPreferences: getIt<SharedPreferences>(),
    ),
  );
  getIt.registerFactory<ProfileBloc>(
    () => ProfileBloc(
      getUserProfile: getIt<GetUserProfile>(),
      updateProfile: getIt<UpdateProfile>(),
    ),
  );
  getIt.registerFactory<FavoritesBloc>(
    () => FavoritesBloc(
      getFavorites: getIt<GetFavorites>(),
      removeFavorite: getIt<RemoveFavorite>(),
      toggleFavorite: getIt<ToggleFavorite>(),
    ),
  );

  // Messaging BLoC
  getIt.registerFactory<MessagingBloc>(
    () => MessagingBloc(
      getConversations: getIt<GetConversations>(),
      getMessages: getIt<GetMessages>(),
      sendMessage: getIt<SendMessage>(),
      markConversationAsRead: getIt<MarkConversationAsRead>(),
      messagingRepository: getIt<MessagingRepository>(),
    ),
  );

  // Dealer BLoC
  getIt.registerFactory<DealerBloc>(
    () => DealerBloc(
      getDealerStats: getIt<GetDealerStats>(),
      getListings: getIt<GetListings>(),
      createListing: getIt<CreateListing>(),
      updateListing: getIt<UpdateListing>(),
      deleteListing: getIt<DeleteListing>(),
      publishListing: getIt<PublishListing>(),
      getLeads: getIt<GetLeads>(),
      updateLeadStatus: getIt<UpdateLeadStatus>(),
      updateLeadNotes: getIt<UpdateLeadNotes>(),
      scheduleFollowUp: getIt<ScheduleFollowUp>(),
      dealerRepository: getIt<DealerRepository>(),
    ),
  );
}

import 'package:equatable/equatable.dart';

/// Eventos del SearchBloc
abstract class SearchEvent extends Equatable {
  const SearchEvent();

  @override
  List<Object?> get props => [];
}

/// Evento para buscar vehículos
class SearchVehiclesEvent extends SearchEvent {
  final String query;
  final int? limit;

  const SearchVehiclesEvent({
    required this.query,
    this.limit,
  });

  @override
  List<Object?> get props => [query, limit];
}

/// Evento para limpiar la búsqueda
class ClearSearch extends SearchEvent {
  const ClearSearch();
}

/// Evento para agregar a búsquedas recientes
class AddToRecentSearches extends SearchEvent {
  final String query;

  const AddToRecentSearches(this.query);

  @override
  List<Object> get props => [query];
}

/// Evento para cargar búsquedas recientes
class LoadRecentSearches extends SearchEvent {
  const LoadRecentSearches();
}

/// Evento para eliminar una búsqueda reciente
class RemoveRecentSearch extends SearchEvent {
  final String query;

  const RemoveRecentSearch(this.query);

  @override
  List<Object> get props => [query];
}

/// Evento para limpiar todas las búsquedas recientes
class ClearRecentSearches extends SearchEvent {
  const ClearRecentSearches();
}

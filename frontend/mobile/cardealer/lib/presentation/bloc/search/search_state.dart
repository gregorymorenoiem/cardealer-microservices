import 'package:equatable/equatable.dart';
import '../../../domain/entities/vehicle.dart';

/// Estados del SearchBloc
abstract class SearchState extends Equatable {
  const SearchState();

  @override
  List<Object?> get props => [];
}

/// Estado inicial
class SearchInitial extends SearchState {
  const SearchInitial();
}

/// Estado de carga
class SearchLoading extends SearchState {
  const SearchLoading();
}

/// Estado con resultados de búsqueda
class SearchLoaded extends SearchState {
  final List<Vehicle> results;
  final String query;
  final int totalResults;

  const SearchLoaded({
    required this.results,
    required this.query,
    required this.totalResults,
  });

  @override
  List<Object> get props => [results, query, totalResults];
}

/// Estado vacío (sin resultados)
class SearchEmpty extends SearchState {
  final String query;

  const SearchEmpty(this.query);

  @override
  List<Object> get props => [query];
}

/// Estado con búsquedas recientes
class SearchRecentLoaded extends SearchState {
  final List<String> recentSearches;

  const SearchRecentLoaded(this.recentSearches);

  @override
  List<Object> get props => [recentSearches];
}

/// Estado de error
class SearchError extends SearchState {
  final String message;

  const SearchError(this.message);

  @override
  List<Object> get props => [message];
}

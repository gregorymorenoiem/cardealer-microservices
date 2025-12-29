import 'package:flutter/material.dart';
import '../../../../domain/entities/filter_criteria.dart';
import '../../../../core/theme/colors.dart';
import '../../../../core/theme/spacing.dart';

/// Chips que muestran los filtros activos
class ActiveFiltersChips extends StatelessWidget {
  final FilterCriteria criteria;
  final VoidCallback onClearAll;

  const ActiveFiltersChips({
    super.key,
    required this.criteria,
    required this.onClearAll,
  });

  @override
  Widget build(BuildContext context) {
    final chips = <Widget>[];

    // Precio
    if (criteria.minPrice != null || criteria.maxPrice != null) {
      final text = _buildPriceLabel();
      chips.add(_buildChip(text));
    }

    // Año
    if (criteria.minYear != null || criteria.maxYear != null) {
      final text = _buildYearLabel();
      chips.add(_buildChip(text));
    }

    // Marcas
    if (criteria.makes != null && criteria.makes!.isNotEmpty) {
      chips.add(_buildChip('${criteria.makes!.length} marca(s)'));
    }

    // Tipos de carrocería
    if (criteria.bodyTypes != null && criteria.bodyTypes!.isNotEmpty) {
      chips.add(_buildChip('${criteria.bodyTypes!.length} tipo(s)'));
    }

    // Más filtros
    if (criteria.activeFilterCount > 4) {
      chips.add(_buildChip('+${criteria.activeFilterCount - 4} más'));
    }

    return SizedBox(
      height: 32,
      child: ListView(
        scrollDirection: Axis.horizontal,
        children: [
          ...chips,
          const SizedBox(width: AppSpacing.sm),
          _buildClearButton(),
        ],
      ),
    );
  }

  Widget _buildChip(String label) {
    return Container(
      margin: const EdgeInsets.only(right: AppSpacing.xs),
      child: Chip(
        label: Text(
          label,
          style: const TextStyle(fontSize: 12),
        ),
        backgroundColor: AppColors.primaryLight.withValues(alpha: 0.1),
        deleteIconColor: AppColors.primary,
      ),
    );
  }

  Widget _buildClearButton() {
    return TextButton.icon(
      onPressed: onClearAll,
      icon: const Icon(Icons.clear_all, size: 16),
      label: const Text('Limpiar', style: TextStyle(fontSize: 12)),
      style: TextButton.styleFrom(
        padding: const EdgeInsets.symmetric(horizontal: AppSpacing.sm),
      ),
    );
  }

  String _buildPriceLabel() {
    if (criteria.minPrice != null && criteria.maxPrice != null) {
      return '\$${_formatNumber(criteria.minPrice!)} - \$${_formatNumber(criteria.maxPrice!)}';
    } else if (criteria.minPrice != null) {
      return 'Desde \$${_formatNumber(criteria.minPrice!)}';
    } else {
      return 'Hasta \$${_formatNumber(criteria.maxPrice!)}';
    }
  }

  String _buildYearLabel() {
    if (criteria.minYear != null && criteria.maxYear != null) {
      return '${criteria.minYear} - ${criteria.maxYear}';
    } else if (criteria.minYear != null) {
      return 'Desde ${criteria.minYear}';
    } else {
      return 'Hasta ${criteria.maxYear}';
    }
  }

  String _formatNumber(double number) {
    if (number >= 1000000) {
      return '${(number / 1000000).toStringAsFixed(1)}M';
    } else if (number >= 1000) {
      return '${(number / 1000).toStringAsFixed(0)}K';
    }
    return number.toStringAsFixed(0);
  }
}

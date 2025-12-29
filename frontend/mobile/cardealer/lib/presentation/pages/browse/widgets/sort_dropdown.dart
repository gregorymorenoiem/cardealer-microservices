import 'package:flutter/material.dart';
import '../../../../domain/entities/filter_criteria.dart';

/// Dropdown para seleccionar opción de ordenamiento
class SortDropdown extends StatelessWidget {
  final SortOption value;
  final ValueChanged<SortOption?> onChanged;

  const SortDropdown({
    super.key,
    required this.value,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    return DropdownButton<SortOption>(
      value: value,
      onChanged: onChanged,
      underline: const SizedBox(),
      icon: const Icon(Icons.sort),
      items: SortOption.values.map((option) {
        return DropdownMenuItem(
          value: option,
          child: Text(
            option.label,
            style: const TextStyle(fontSize: 14),
          ),
        );
      }).toList(),
    );
  }
}

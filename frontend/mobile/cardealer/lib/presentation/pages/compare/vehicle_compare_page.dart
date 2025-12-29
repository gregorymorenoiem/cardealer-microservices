import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/typography.dart';
import '../../../core/theme/spacing.dart';
import '../../widgets/buttons/gradient_button.dart';

/// Vehicle Comparison Feature
/// Sprint 8 - SF-002: Compare Feature
class VehicleComparePage extends StatefulWidget {
  final List<String>? preselectedVehicles;

  const VehicleComparePage({
    super.key,
    this.preselectedVehicles,
  });

  @override
  State<VehicleComparePage> createState() => _VehicleComparePageState();
}

class _VehicleComparePageState extends State<VehicleComparePage> {
  List<VehicleCompareModel> _selectedVehicles = [];
  String _comparisonView = 'table'; // table or cards

  @override
  void initState() {
    super.initState();
    // Load preselected vehicles
    _selectedVehicles = [
      VehicleCompareModel(
        id: '1',
        name: 'BMW X5 M Sport',
        year: 2023,
        price: 45000,
        mileage: 15000,
        transmission: 'Automática',
        fuelType: 'Gasolina',
        horsepower: 335,
        seats: 5,
        features: ['Techo panorámico', 'Apple CarPlay', 'Cámara 360°'],
        imageUrl: '',
      ),
      VehicleCompareModel(
        id: '2',
        name: 'Mercedes GLE 450',
        year: 2023,
        price: 52000,
        mileage: 12000,
        transmission: 'Automática',
        fuelType: 'Híbrido',
        horsepower: 362,
        seats: 5,
        features: ['MBUX', 'Suspensión neumática', 'Burmester'],
        imageUrl: '',
      ),
    ];
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: _buildAppBar(),
      body: _selectedVehicles.isEmpty
          ? _buildEmptyState()
          : _comparisonView == 'table'
              ? _buildTableView()
              : _buildCardsView(),
      bottomNavigationBar:
          _selectedVehicles.length >= 2 ? _buildBottomActions() : null,
    );
  }

  AppBar _buildAppBar() {
    return AppBar(
      backgroundColor: AppColors.background,
      elevation: 0,
      title: Text(
        'Comparar Vehículos',
        style: AppTypography.h3.copyWith(
          color: AppColors.textPrimary,
        ),
      ),
      actions: [
        if (_selectedVehicles.length >= 2)
          IconButton(
            icon: Icon(
              _comparisonView == 'table'
                  ? Icons.view_carousel
                  : Icons.table_chart,
              color: AppColors.textPrimary,
            ),
            onPressed: () {
              setState(() {
                _comparisonView =
                    _comparisonView == 'table' ? 'cards' : 'table';
              });
            },
          ),
        if (_selectedVehicles.isNotEmpty)
          IconButton(
            icon: const Icon(Icons.add, color: AppColors.primary),
            onPressed: _addVehicle,
          ),
        PopupMenuButton<String>(
          onSelected: _handleMenuAction,
          itemBuilder: (context) => [
            const PopupMenuItem(value: 'share', child: Text('Compartir')),
            const PopupMenuItem(value: 'export', child: Text('Exportar PDF')),
            const PopupMenuItem(value: 'clear', child: Text('Limpiar')),
          ],
        ),
      ],
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(AppSpacing.xl),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [
                    AppColors.primary.withValues(alpha: 0.1),
                    AppColors.accent.withValues(alpha: 0.1),
                  ],
                ),
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.compare_arrows,
                size: 80,
                color: AppColors.primary,
              ),
            ),
            const SizedBox(height: AppSpacing.xl),
            Text(
              'Compara Vehículos',
              style: AppTypography.h2.copyWith(
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: AppSpacing.md),
            Text(
              'Selecciona hasta 4 vehículos para comparar\nsus características lado a lado',
              style: AppTypography.bodyLarge.copyWith(
                color: AppColors.textSecondary,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: AppSpacing.xl),
            GradientButton(
              text: 'Seleccionar Vehículos',
              icon: const Icon(Icons.add),
              onPressed: _addVehicle,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTableView() {
    return SingleChildScrollView(
      child: Column(
        children: [
          // Vehicle Images Row
          _buildVehicleImagesRow(),
          const SizedBox(height: AppSpacing.md),

          // Comparison Table
          _buildComparisonSection(
              'Precio', (v) => '\$${v.price.toStringAsFixed(0)}'),
          _buildComparisonSection('Año', (v) => v.year.toString()),
          _buildComparisonSection(
              'Kilometraje', (v) => '${v.mileage.toStringAsFixed(0)} km'),
          _buildComparisonSection('Transmisión', (v) => v.transmission),
          _buildComparisonSection('Combustible', (v) => v.fuelType),
          _buildComparisonSection('Potencia', (v) => '${v.horsepower} HP'),
          _buildComparisonSection('Asientos', (v) => v.seats.toString()),

          // Features Comparison
          _buildFeaturesComparison(),

          const SizedBox(height: AppSpacing.xxl),
        ],
      ),
    );
  }

  Widget _buildVehicleImagesRow() {
    return SizedBox(
      height: 200,
      child: Row(
        children: _selectedVehicles.map((vehicle) {
          return Expanded(
            child: Stack(
              children: [
                Container(
                  margin: const EdgeInsets.all(AppSpacing.xs),
                  decoration: BoxDecoration(
                    color: Colors.grey[300],
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(Icons.directions_car, size: 60),
                      const SizedBox(height: AppSpacing.sm),
                      Padding(
                        padding: const EdgeInsets.symmetric(
                            horizontal: AppSpacing.xs),
                        child: Text(
                          vehicle.name,
                          style: AppTypography.bodyMedium.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                          textAlign: TextAlign.center,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                ),
                Positioned(
                  top: AppSpacing.sm,
                  right: AppSpacing.sm,
                  child: GestureDetector(
                    onTap: () => _removeVehicle(vehicle.id),
                    child: Container(
                      padding: const EdgeInsets.all(4),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        shape: BoxShape.circle,
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withValues(alpha: 0.2),
                            blurRadius: 4,
                          ),
                        ],
                      ),
                      child: const Icon(Icons.close, size: 16),
                    ),
                  ),
                ),
              ],
            ),
          );
        }).toList(),
      ),
    );
  }

  Widget _buildComparisonSection(
      String label, String Function(VehicleCompareModel) getValue) {
    // Find best value for highlighting
    final values = _selectedVehicles.map((v) => getValue(v)).toList();

    return Container(
      margin: const EdgeInsets.symmetric(
        horizontal: AppSpacing.lg,
        vertical: AppSpacing.xs,
      ),
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          // Label
          Expanded(
            flex: 2,
            child: Container(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Text(
                label,
                style: AppTypography.bodyMedium.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          ),
          // Values
          ..._selectedVehicles.map((vehicle) {
            final value = getValue(vehicle);
            final isBest = _isBestValue(label, value, values);

            return Expanded(
              flex: 3,
              child: Container(
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: BoxDecoration(
                  color:
                      isBest ? AppColors.primary.withValues(alpha: 0.1) : null,
                  border: const Border(
                    left: BorderSide(
                      color: AppColors.background,
                      width: 1,
                    ),
                  ),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    if (isBest)
                      const Padding(
                        padding: EdgeInsets.only(right: 4),
                        child: Icon(
                          Icons.star,
                          size: 14,
                          color: AppColors.primary,
                        ),
                      ),
                    Flexible(
                      child: Text(
                        value,
                        style: AppTypography.bodyMedium.copyWith(
                          color: isBest
                              ? AppColors.primary
                              : AppColors.textPrimary,
                          fontWeight: isBest ? FontWeight.bold : null,
                        ),
                        textAlign: TextAlign.center,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                  ],
                ),
              ),
            );
          }),
        ],
      ),
    );
  }

  bool _isBestValue(String label, String value, List<String> allValues) {
    // Logic to determine best value
    if (label == 'Precio') {
      final prices = allValues
          .map((v) => double.tryParse(v.replaceAll(RegExp(r'[^\d.]'), '')) ?? 0)
          .toList();
      final minPrice = prices.reduce((a, b) => a < b ? a : b);
      final currentPrice =
          double.tryParse(value.replaceAll(RegExp(r'[^\d.]'), '')) ?? 0;
      return currentPrice == minPrice;
    }
    if (label == 'Potencia') {
      final powers = allValues
          .map((v) => int.tryParse(v.replaceAll(RegExp(r'[^\d]'), '')) ?? 0)
          .toList();
      final maxPower = powers.reduce((a, b) => a > b ? a : b);
      final currentPower =
          int.tryParse(value.replaceAll(RegExp(r'[^\d]'), '')) ?? 0;
      return currentPower == maxPower;
    }
    return false;
  }

  Widget _buildFeaturesComparison() {
    // Get all unique features
    final allFeatures = <String>{};
    for (var vehicle in _selectedVehicles) {
      allFeatures.addAll(vehicle.features);
    }

    return Container(
      margin: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Text(
              'Características',
              style: AppTypography.h4.copyWith(
                color: AppColors.textPrimary,
              ),
            ),
          ),
          ...allFeatures.map((feature) {
            return Row(
              children: [
                Expanded(
                  flex: 2,
                  child: Padding(
                    padding: const EdgeInsets.all(AppSpacing.sm),
                    child: Text(
                      feature,
                      style: AppTypography.bodySmall,
                    ),
                  ),
                ),
                ..._selectedVehicles.map((vehicle) {
                  final hasFeature = vehicle.features.contains(feature);
                  return Expanded(
                    flex: 3,
                    child: Container(
                      padding: const EdgeInsets.all(AppSpacing.sm),
                      child: Icon(
                        hasFeature ? Icons.check_circle : Icons.cancel,
                        color: hasFeature
                            ? Colors.green
                            : Colors.red.withValues(alpha: 0.3),
                        size: 20,
                      ),
                    ),
                  );
                }),
              ],
            );
          }),
        ],
      ),
    );
  }

  Widget _buildCardsView() {
    return PageView.builder(
      itemCount: _selectedVehicles.length,
      itemBuilder: (context, index) {
        final vehicle = _selectedVehicles[index];
        return _buildVehicleCard(vehicle);
      },
    );
  }

  Widget _buildVehicleCard(VehicleCompareModel vehicle) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Image
          Container(
            height: 200,
            decoration: BoxDecoration(
              color: Colors.grey[300],
              borderRadius: BorderRadius.circular(16),
            ),
            child: const Center(
              child: Icon(Icons.directions_car, size: 80),
            ),
          ),
          const SizedBox(height: AppSpacing.lg),

          // Name and Price
          Text(vehicle.name, style: AppTypography.h2),
          const SizedBox(height: AppSpacing.xs),
          Text(
            '\$${vehicle.price.toStringAsFixed(0)}',
            style: AppTypography.h3.copyWith(color: AppColors.primary),
          ),
          const SizedBox(height: AppSpacing.lg),

          // Specs Grid
          GridView.count(
            crossAxisCount: 2,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            childAspectRatio: 2.5,
            crossAxisSpacing: AppSpacing.md,
            mainAxisSpacing: AppSpacing.md,
            children: [
              _buildSpecCard('Año', vehicle.year.toString()),
              _buildSpecCard('Kilometraje', '${vehicle.mileage} km'),
              _buildSpecCard('Transmisión', vehicle.transmission),
              _buildSpecCard('Combustible', vehicle.fuelType),
              _buildSpecCard('Potencia', '${vehicle.horsepower} HP'),
              _buildSpecCard('Asientos', vehicle.seats.toString()),
            ],
          ),
          const SizedBox(height: AppSpacing.lg),

          // Features
          const Text('Características', style: AppTypography.h4),
          const SizedBox(height: AppSpacing.md),
          Wrap(
            spacing: AppSpacing.sm,
            runSpacing: AppSpacing.sm,
            children: vehicle.features.map((feature) {
              return Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: AppSpacing.md,
                  vertical: AppSpacing.sm,
                ),
                decoration: BoxDecoration(
                  color: AppColors.primary.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  feature,
                  style: AppTypography.bodySmall.copyWith(
                    color: AppColors.primary,
                  ),
                ),
              );
            }).toList(),
          ),
        ],
      ),
    );
  }

  Widget _buildSpecCard(String label, String value) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text(
            label,
            style: AppTypography.caption.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            value,
            style: AppTypography.bodyMedium.copyWith(
              fontWeight: FontWeight.w600,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
        ],
      ),
    );
  }

  Widget _buildBottomActions() {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 8,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: Row(
        children: [
          Expanded(
            child: OutlinedButton.icon(
              onPressed: _shareComparison,
              icon: const Icon(Icons.share),
              label: const Text('Compartir'),
              style: OutlinedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: AppSpacing.md),
              ),
            ),
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: GradientButton(
              text: 'Exportar PDF',
              icon: const Icon(Icons.picture_as_pdf),
              onPressed: _exportPDF,
            ),
          ),
        ],
      ),
    );
  }

  void _addVehicle() {
    if (_selectedVehicles.length >= 4) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Máximo 4 vehículos para comparar'),
        ),
      );
      return;
    }
    // Navigate to vehicle selection
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Seleccionar vehículo desde favoritos o búsqueda'),
      ),
    );
  }

  void _removeVehicle(String id) {
    setState(() {
      _selectedVehicles.removeWhere((v) => v.id == id);
    });
  }

  void _shareComparison() {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Compartiendo comparación...')),
    );
  }

  void _exportPDF() {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Exportando a PDF...')),
    );
  }

  void _handleMenuAction(String action) {
    switch (action) {
      case 'share':
        _shareComparison();
        break;
      case 'export':
        _exportPDF();
        break;
      case 'clear':
        setState(() => _selectedVehicles.clear());
        break;
    }
  }
}

class VehicleCompareModel {
  final String id;
  final String name;
  final int year;
  final double price;
  final double mileage;
  final String transmission;
  final String fuelType;
  final int horsepower;
  final int seats;
  final List<String> features;
  final String imageUrl;

  VehicleCompareModel({
    required this.id,
    required this.name,
    required this.year,
    required this.price,
    required this.mileage,
    required this.transmission,
    required this.fuelType,
    required this.horsepower,
    required this.seats,
    required this.features,
    required this.imageUrl,
  });
}

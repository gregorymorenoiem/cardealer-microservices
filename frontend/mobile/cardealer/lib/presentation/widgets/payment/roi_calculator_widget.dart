import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';

/// MF-004: ROI Calculator
///
/// Features:
/// - "Cuánto puedes ganar"
/// - Input de vehículos a vender
/// - Cálculo de ROI
/// - Animación de resultado
class ROICalculatorWidget extends StatefulWidget {
  final double planPrice;

  const ROICalculatorWidget({
    super.key,
    required this.planPrice,
  });

  @override
  State<ROICalculatorWidget> createState() => _ROICalculatorWidgetState();
}

class _ROICalculatorWidgetState extends State<ROICalculatorWidget>
    with SingleTickerProviderStateMixin {
  int _vehiclesPerMonth = 5;
  double _averageProfit = 2000.0;
  bool _showResult = false;

  late AnimationController _animationController;
  late Animation<double> _scaleAnimation;
  late Animation<double> _fadeAnimation;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 600),
      vsync: this,
    );

    _scaleAnimation = Tween<double>(begin: 0.8, end: 1.0).animate(
      CurvedAnimation(
        parent: _animationController,
        curve: Curves.easeOutBack,
      ),
    );

    _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _animationController,
        curve: Curves.easeIn,
      ),
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  void _calculate() {
    setState(() {
      _showResult = true;
    });
    _animationController.forward(from: 0);
  }

  double get _monthlyGrossProfit => _vehiclesPerMonth * _averageProfit;
  double get _annualGrossProfit => _monthlyGrossProfit * 12;
  double get _annualPlanCost => widget.planPrice * 12;
  double get _netProfit => _annualGrossProfit - _annualPlanCost;
  double get _roi => ((_netProfit / _annualPlanCost) * 100);

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            AppColors.primary.withValues(alpha: 0.05),
            AppColors.accent.withValues(alpha: 0.05),
          ],
        ),
        borderRadius: BorderRadius.circular(24),
        border: Border.all(
          color: AppColors.primary.withValues(alpha: 0.2),
          width: 2,
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(AppSpacing.sm),
                  decoration: BoxDecoration(
                    color: AppColors.primary.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(
                    Icons.calculate,
                    color: AppColors.primary,
                    size: 28,
                  ),
                ),
                const SizedBox(width: AppSpacing.md),
                const Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Calculadora de ROI',
                        style: TextStyle(
                          fontFamily: 'Poppins',
                          fontSize: 22,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text(
                        '¿Cuánto podrías ganar?',
                        style: TextStyle(
                          fontFamily: 'Inter',
                          fontSize: 14,
                          color: AppColors.textSecondary,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),

            const SizedBox(height: AppSpacing.xl),

            // Vehicles per month slider
            _buildSlider(
              label: 'Vehículos vendidos por mes',
              value: _vehiclesPerMonth.toDouble(),
              min: 1,
              max: 50,
              divisions: 49,
              displayValue: '$_vehiclesPerMonth vehículos',
              onChanged: (value) {
                setState(() {
                  _vehiclesPerMonth = value.toInt();
                  _showResult = false;
                });
              },
            ),

            const SizedBox(height: AppSpacing.lg),

            // Average profit slider
            _buildSlider(
              label: 'Ganancia promedio por vehículo',
              value: _averageProfit,
              min: 500,
              max: 10000,
              divisions: 19,
              displayValue: '\$${_averageProfit.toStringAsFixed(0)}',
              onChanged: (value) {
                setState(() {
                  _averageProfit = (value / 500).round() * 500;
                  _showResult = false;
                });
              },
            ),

            const SizedBox(height: AppSpacing.xl),

            // Calculate button
            SizedBox(
              width: double.infinity,
              height: 56,
              child: ElevatedButton(
                onPressed: _calculate,
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  elevation: 4,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                ),
                child: const Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.insights),
                    SizedBox(width: AppSpacing.sm),
                    Text(
                      'Calcular Retorno',
                      style: TextStyle(
                        fontFamily: 'Poppins',
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
            ),

            // Results
            if (_showResult) ...[
              const SizedBox(height: AppSpacing.xl),
              FadeTransition(
                opacity: _fadeAnimation,
                child: ScaleTransition(
                  scale: _scaleAnimation,
                  child: Container(
                    padding: const EdgeInsets.all(AppSpacing.lg),
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        colors: [
                          AppColors.success.withValues(alpha: 0.1),
                          AppColors.success.withValues(alpha: 0.05),
                        ],
                      ),
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(
                        color: AppColors.success.withValues(alpha: 0.3),
                        width: 2,
                      ),
                    ),
                    child: Column(
                      children: [
                        // ROI percentage
                        Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          crossAxisAlignment: CrossAxisAlignment.end,
                          children: [
                            Text(
                              _roi.toStringAsFixed(0),
                              style: const TextStyle(
                                fontFamily: 'Poppins',
                                fontSize: 56,
                                fontWeight: FontWeight.bold,
                                color: AppColors.success,
                                height: 1,
                              ),
                            ),
                            const Padding(
                              padding: EdgeInsets.only(bottom: 8),
                              child: Text(
                                '%',
                                style: TextStyle(
                                  fontFamily: 'Poppins',
                                  fontSize: 32,
                                  fontWeight: FontWeight.bold,
                                  color: AppColors.success,
                                ),
                              ),
                            ),
                          ],
                        ),

                        const Text(
                          'Retorno de Inversión Anual',
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontSize: 14,
                            color: AppColors.textSecondary,
                          ),
                        ),

                        const SizedBox(height: AppSpacing.lg),

                        // Breakdown
                        _buildResultRow(
                          'Ganancia Bruta Anual',
                          '\$${_annualGrossProfit.toStringAsFixed(0)}',
                          AppColors.textPrimary,
                        ),
                        _buildResultRow(
                          'Costo del Plan Anual',
                          '-\$${_annualPlanCost.toStringAsFixed(0)}',
                          AppColors.error,
                        ),
                        const Divider(height: AppSpacing.lg),
                        _buildResultRow(
                          'Ganancia Neta Anual',
                          '\$${_netProfit.toStringAsFixed(0)}',
                          AppColors.success,
                          isBold: true,
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildSlider({
    required String label,
    required double value,
    required double min,
    required double max,
    required int divisions,
    required String displayValue,
    required ValueChanged<double> onChanged,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              label,
              style: const TextStyle(
                fontFamily: 'Inter',
                fontSize: 14,
                fontWeight: FontWeight.w500,
              ),
            ),
            Container(
              padding: const EdgeInsets.symmetric(
                horizontal: AppSpacing.sm,
                vertical: AppSpacing.xxs,
              ),
              decoration: BoxDecoration(
                color: AppColors.primary.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text(
                displayValue,
                style: const TextStyle(
                  fontFamily: 'Poppins',
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: AppColors.primary,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: AppSpacing.xs),
        SliderTheme(
          data: SliderThemeData(
            activeTrackColor: AppColors.primary,
            inactiveTrackColor: AppColors.border,
            thumbColor: AppColors.primary,
            overlayColor: AppColors.primary.withValues(alpha: 0.2),
            valueIndicatorColor: AppColors.primary,
          ),
          child: Slider(
            value: value,
            min: min,
            max: max,
            divisions: divisions,
            onChanged: onChanged,
          ),
        ),
      ],
    );
  }

  Widget _buildResultRow(String label, String value, Color color,
      {bool isBold = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: AppSpacing.xs),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              fontFamily: 'Inter',
              fontSize: isBold ? 15 : 14,
              fontWeight: isBold ? FontWeight.w600 : FontWeight.normal,
            ),
          ),
          Text(
            value,
            style: TextStyle(
              fontFamily: 'Poppins',
              fontSize: isBold ? 18 : 16,
              fontWeight: isBold ? FontWeight.bold : FontWeight.w600,
              color: color,
            ),
          ),
        ],
      ),
    );
  }
}

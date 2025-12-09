import 'package:flutter/material.dart';
import 'dart:math' as math;

/// Financing Calculator Interactive
///
/// Features:
/// - Down payment slider (0-50%)
/// - Loan term slider (12-84 months)
/// - Interest rate input
/// - Monthly payment calculation
/// - Total cost breakdown
class FinancingCalculator extends StatefulWidget {
  final double vehiclePrice;
  final double defaultInterestRate;

  const FinancingCalculator({
    super.key,
    required this.vehiclePrice,
    this.defaultInterestRate = 6.5,
  });

  @override
  State<FinancingCalculator> createState() => _FinancingCalculatorState();
}

class _FinancingCalculatorState extends State<FinancingCalculator> {
  double _downPaymentPercent = 20.0;
  int _loanTermMonths = 60;
  late double _interestRate;

  @override
  void initState() {
    super.initState();
    _interestRate = widget.defaultInterestRate;
  }

  double get _downPaymentAmount =>
      widget.vehiclePrice * (_downPaymentPercent / 100);
  double get _loanAmount => widget.vehiclePrice - _downPaymentAmount;

  double get _monthlyPayment {
    if (_loanAmount <= 0) return 0;

    final monthlyRate = (_interestRate / 100) / 12;
    if (monthlyRate == 0) return _loanAmount / _loanTermMonths;

    return _loanAmount *
        (monthlyRate * math.pow(1 + monthlyRate, _loanTermMonths)) /
        (math.pow(1 + monthlyRate, _loanTermMonths) - 1);
  }

  double get _totalCost =>
      (_monthlyPayment * _loanTermMonths) + _downPaymentAmount;
  double get _totalInterest => _totalCost - widget.vehiclePrice;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            const Color(0xFF001F54).withValues(alpha: 0.03),
            const Color(0xFF001F54).withValues(alpha: 0.08),
          ],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: const Color(0xFF001F54).withValues(alpha: 0.1),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Title
          const Row(
            children: [
              Icon(
                Icons.calculate_outlined,
                color: Color(0xFF001F54),
                size: 24,
              ),
              SizedBox(width: 12),
              Text(
                'Financing Calculator',
                style: TextStyle(
                  color: Color(0xFF001F54),
                  fontSize: 20,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
          ),

          const SizedBox(height: 24),

          // Monthly Payment Display
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: const Color(0xFF001F54),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Column(
              children: [
                const Text(
                  'Estimated Monthly Payment',
                  style: TextStyle(
                    color: Colors.white70,
                    fontSize: 14,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  '\$${_monthlyPayment.toStringAsFixed(0)}',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 36,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'per month for $_loanTermMonths months',
                  style: const TextStyle(
                    color: Colors.white70,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: 24),

          // Down Payment Slider
          _buildSliderSection(
            label: 'Down Payment',
            value: '\$${_downPaymentAmount.toStringAsFixed(0)}',
            subtitle: '${_downPaymentPercent.toInt()}% of vehicle price',
            sliderValue: _downPaymentPercent,
            min: 0,
            max: 50,
            divisions: 50,
            onChanged: (value) {
              setState(() {
                _downPaymentPercent = value;
              });
            },
          ),

          const SizedBox(height: 20),

          // Loan Term Slider
          _buildSliderSection(
            label: 'Loan Term',
            value: '$_loanTermMonths months',
            subtitle: '${(_loanTermMonths / 12).toStringAsFixed(1)} years',
            sliderValue: _loanTermMonths.toDouble(),
            min: 12,
            max: 84,
            divisions: 72,
            onChanged: (value) {
              setState(() {
                _loanTermMonths = value.toInt();
              });
            },
          ),

          const SizedBox(height: 20),

          // Interest Rate
          _buildSliderSection(
            label: 'Interest Rate (APR)',
            value: '${_interestRate.toStringAsFixed(1)}%',
            subtitle: 'Annual Percentage Rate',
            sliderValue: _interestRate,
            min: 0,
            max: 15,
            divisions: 150,
            onChanged: (value) {
              setState(() {
                _interestRate = value;
              });
            },
          ),

          const SizedBox(height: 24),

          // Breakdown
          _buildBreakdownRow(
              'Loan Amount', '\$${_loanAmount.toStringAsFixed(0)}'),
          const SizedBox(height: 8),
          _buildBreakdownRow(
              'Total Interest', '\$${_totalInterest.toStringAsFixed(0)}'),
          const SizedBox(height: 8),
          _buildBreakdownRow('Total Cost', '\$${_totalCost.toStringAsFixed(0)}',
              isTotal: true),

          const SizedBox(height: 20),

          // Disclaimer
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.amber.shade50,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(
                color: Colors.amber.shade200,
              ),
            ),
            child: Row(
              children: [
                Icon(
                  Icons.info_outline,
                  size: 16,
                  color: Colors.amber.shade900,
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'This is an estimate. Actual rates may vary based on credit.',
                    style: TextStyle(
                      color: Colors.amber.shade900,
                      fontSize: 11,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSliderSection({
    required String label,
    required String value,
    required String subtitle,
    required double sliderValue,
    required double min,
    required double max,
    required int divisions,
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
                color: Color(0xFF1E293B),
                fontSize: 14,
                fontWeight: FontWeight.w600,
              ),
            ),
            Text(
              value,
              style: const TextStyle(
                color: Color(0xFF001F54),
                fontSize: 16,
                fontWeight: FontWeight.w700,
              ),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Text(
          subtitle,
          style: TextStyle(
            color: Colors.grey.shade600,
            fontSize: 12,
          ),
        ),
        SliderTheme(
          data: SliderTheme.of(context).copyWith(
            activeTrackColor: const Color(0xFF001F54),
            inactiveTrackColor: const Color(0xFF001F54).withValues(alpha: 0.2),
            thumbColor: const Color(0xFF001F54),
            overlayColor: const Color(0xFF001F54).withValues(alpha: 0.1),
            trackHeight: 4,
          ),
          child: Slider(
            value: sliderValue,
            min: min,
            max: max,
            divisions: divisions,
            onChanged: onChanged,
          ),
        ),
      ],
    );
  }

  Widget _buildBreakdownRow(String label, String value,
      {bool isTotal = false}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: TextStyle(
            color: isTotal ? const Color(0xFF001F54) : Colors.grey.shade700,
            fontSize: isTotal ? 16 : 14,
            fontWeight: isTotal ? FontWeight.w700 : FontWeight.w500,
          ),
        ),
        Text(
          value,
          style: TextStyle(
            color: isTotal ? const Color(0xFF001F54) : Colors.grey.shade900,
            fontSize: isTotal ? 18 : 14,
            fontWeight: FontWeight.w700,
          ),
        ),
      ],
    );
  }
}

import 'package:flutter/material.dart';

/// DP-007: Performance Insights
/// Análisis de rendimiento y sugerencias de mejora
class PerformanceInsightsPage extends StatelessWidget {
  const PerformanceInsightsPage({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Insights de Rendimiento'),
        actions: [
          IconButton(
            icon: const Icon(Icons.info_outline),
            onPressed: () {
              showDialog(
                context: context,
                builder: (context) => AlertDialog(
                  title: const Text('Acerca de Insights'),
                  content: const Text(
                    'Los insights de rendimiento te ayudan a optimizar tus publicaciones '
                    'basándose en datos reales de visualizaciones, interacciones y conversiones.',
                  ),
                  actions: [
                    TextButton(
                      onPressed: () => Navigator.pop(context),
                      child: const Text('Entendido'),
                    ),
                  ],
                ),
              );
            },
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Overall performance score
          _PerformanceScoreCard(),
          const SizedBox(height: 16),

          // Top performing vehicles
          Text(
            'Vehículos con Mejor Rendimiento',
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 12),
          const _VehiclePerformanceCard(
            title: 'Toyota Camry 2024',
            score: 92,
            views: 1245,
            leads: 23,
            conversionRate: 1.8,
            trend: 'up',
            suggestions: [
              'Excelente rendimiento',
              'Considera promover más',
            ],
          ),
          const SizedBox(height: 8),
          const _VehiclePerformanceCard(
            title: 'Honda Civic 2023',
            score: 78,
            views: 892,
            leads: 15,
            conversionRate: 1.7,
            trend: 'stable',
            suggestions: [
              'Buen rendimiento general',
              'Actualiza las fotos para mejorar',
            ],
          ),
          const SizedBox(height: 16),

          // Underperforming vehicles
          Text(
            'Vehículos que Necesitan Atención',
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 12),
          const _VehiclePerformanceCard(
            title: 'Ford Escape 2024',
            score: 45,
            views: 145,
            leads: 3,
            conversionRate: 2.1,
            trend: 'down',
            suggestions: [
              'Bajo número de visualizaciones',
              'Ajusta el precio (-8%)',
              'Mejora la descripción',
              'Agrega más fotos de calidad',
            ],
          ),
          const SizedBox(height: 8),
          const _VehiclePerformanceCard(
            title: 'Mazda CX-5 2023',
            score: 38,
            views: 543,
            leads: 8,
            conversionRate: 1.5,
            trend: 'down',
            suggestions: [
              'Tasa de conversión baja',
              'Destaca características únicas',
              'Considera incluir video',
            ],
          ),
          const SizedBox(height: 16),

          // Recommendations
          Text(
            'Recomendaciones Generales',
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 12),
          _RecommendationCard(
            icon: Icons.photo_camera,
            title: 'Mejora tus fotografías',
            description:
                'Los anuncios con 8+ fotos tienen 45% más visualizaciones',
            priority: 'high',
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Ver guía de fotografía')),
              );
            },
          ),
          const SizedBox(height: 8),
          _RecommendationCard(
            icon: Icons.description,
            title: 'Optimiza descripciones',
            description:
                'Descripciones detalladas aumentan conversiones en 32%',
            priority: 'medium',
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Ver tips de descripción')),
              );
            },
          ),
          const SizedBox(height: 8),
          _RecommendationCard(
            icon: Icons.access_time,
            title: 'Responde rápidamente',
            description: 'Tiempo de respuesta promedio: 2.5h. Meta: <1h',
            priority: 'high',
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Configurar alertas')),
              );
            },
          ),
          const SizedBox(height: 8),
          _RecommendationCard(
            icon: Icons.trending_up,
            title: 'Promociona estratégicamente',
            description: 'Los viernes tienen 28% más actividad',
            priority: 'low',
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Ver análisis de timing')),
              );
            },
          ),
          const SizedBox(height: 16),

          // Market insights
          Text(
            'Insights del Mercado',
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 12),
          const _MarketInsightCard(
            title: 'SUVs en alta demanda',
            description: 'Los SUVs tienen 45% más interés este mes',
            icon: Icons.trending_up,
            color: Colors.green,
          ),
          const SizedBox(height: 8),
          const _MarketInsightCard(
            title: 'Precios competitivos',
            description: 'Tus precios están 7% por encima del mercado',
            icon: Icons.warning,
            color: Colors.orange,
          ),
          const SizedBox(height: 8),
          const _MarketInsightCard(
            title: 'Época de fin de año',
            description: 'Históricamente, diciembre tiene 22% más ventas',
            icon: Icons.celebration,
            color: Colors.blue,
          ),
        ],
      ),
    );
  }
}

class _PerformanceScoreCard extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            Text(
              'Puntuación General',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            Stack(
              alignment: Alignment.center,
              children: [
                SizedBox(
                  width: 120,
                  height: 120,
                  child: CircularProgressIndicator(
                    value: 0.73,
                    strokeWidth: 12,
                    backgroundColor: theme.colorScheme.surfaceContainerHighest,
                    valueColor: AlwaysStoppedAnimation<Color>(
                      _getScoreColor(73),
                    ),
                  ),
                ),
                Column(
                  children: [
                    Text(
                      '73',
                      style: theme.textTheme.displayMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: _getScoreColor(73),
                      ),
                    ),
                    Text(
                      '/100',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 16),
            const Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                _ScoreMetric(
                  label: 'Visibilidad',
                  value: 82,
                  icon: Icons.visibility,
                ),
                _ScoreMetric(
                  label: 'Engagement',
                  value: 68,
                  icon: Icons.favorite,
                ),
                _ScoreMetric(
                  label: 'Conversión',
                  value: 71,
                  icon: Icons.check_circle,
                ),
              ],
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                const Icon(
                  Icons.trending_up,
                  size: 16,
                  color: Colors.green,
                ),
                const SizedBox(width: 4),
                Text(
                  '+5 puntos vs. semana anterior',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: Colors.green,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Color _getScoreColor(int score) {
    if (score >= 80) return Colors.green;
    if (score >= 60) return Colors.orange;
    return Colors.red;
  }
}

class _ScoreMetric extends StatelessWidget {
  final String label;
  final int value;
  final IconData icon;

  const _ScoreMetric({
    required this.label,
    required this.value,
    required this.icon,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final color = _getScoreColor(value);

    return Column(
      children: [
        Icon(icon, color: color, size: 20),
        const SizedBox(height: 4),
        Text(
          '$value',
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
            color: color,
          ),
        ),
        Text(
          label,
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
          ),
        ),
      ],
    );
  }

  Color _getScoreColor(int score) {
    if (score >= 80) return Colors.green;
    if (score >= 60) return Colors.orange;
    return Colors.red;
  }
}

class _VehiclePerformanceCard extends StatelessWidget {
  final String title;
  final int score;
  final int views;
  final int leads;
  final double conversionRate;
  final String trend;
  final List<String> suggestions;

  const _VehiclePerformanceCard({
    required this.title,
    required this.score,
    required this.views,
    required this.leads,
    required this.conversionRate,
    required this.trend,
    required this.suggestions,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final scoreColor = _getScoreColor(score);
    final trendIcon = _getTrendIcon(trend);
    final trendColor = _getTrendColor(trend);

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    title,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                Row(
                  children: [
                    Icon(trendIcon, size: 16, color: trendColor),
                    const SizedBox(width: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: scoreColor.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: scoreColor),
                      ),
                      child: Text(
                        '$score/100',
                        style: theme.textTheme.labelLarge?.copyWith(
                          color: scoreColor,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 12),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                _MetricItem(
                  icon: Icons.visibility,
                  label: 'Vistas',
                  value: views.toString(),
                ),
                _MetricItem(
                  icon: Icons.people,
                  label: 'Leads',
                  value: leads.toString(),
                ),
                _MetricItem(
                  icon: Icons.percent,
                  label: 'Conv.',
                  value: '${conversionRate.toStringAsFixed(1)}%',
                ),
              ],
            ),
            if (suggestions.isNotEmpty) ...[
              const SizedBox(height: 12),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: theme.colorScheme.surfaceContainerHighest,
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Icon(
                          Icons.lightbulb_outline,
                          size: 16,
                          color: theme.colorScheme.primary,
                        ),
                        const SizedBox(width: 8),
                        Text(
                          'Sugerencias:',
                          style: theme.textTheme.labelLarge?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    ...suggestions.map((suggestion) => Padding(
                          padding: const EdgeInsets.only(bottom: 4),
                          child: Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text('• ', style: theme.textTheme.bodySmall),
                              Expanded(
                                child: Text(
                                  suggestion,
                                  style: theme.textTheme.bodySmall,
                                ),
                              ),
                            ],
                          ),
                        )),
                  ],
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Color _getScoreColor(int score) {
    if (score >= 80) return Colors.green;
    if (score >= 60) return Colors.orange;
    return Colors.red;
  }

  IconData _getTrendIcon(String trend) {
    switch (trend) {
      case 'up':
        return Icons.trending_up;
      case 'down':
        return Icons.trending_down;
      default:
        return Icons.trending_flat;
    }
  }

  Color _getTrendColor(String trend) {
    switch (trend) {
      case 'up':
        return Colors.green;
      case 'down':
        return Colors.red;
      default:
        return Colors.grey;
    }
  }
}

class _MetricItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;

  const _MetricItem({
    required this.icon,
    required this.label,
    required this.value,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      children: [
        Icon(icon, size: 16, color: theme.colorScheme.onSurfaceVariant),
        const SizedBox(height: 4),
        Text(
          value,
          style: theme.textTheme.titleSmall?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        Text(
          label,
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
          ),
        ),
      ],
    );
  }
}

class _RecommendationCard extends StatelessWidget {
  final IconData icon;
  final String title;
  final String description;
  final String priority;
  final VoidCallback onTap;

  const _RecommendationCard({
    required this.icon,
    required this.title,
    required this.description,
    required this.priority,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final priorityColor = _getPriorityColor(priority);

    return Card(
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: priorityColor.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(icon, color: priorityColor),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            title,
                            style: theme.textTheme.titleSmall?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                        _PriorityBadge(priority: priority),
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      description,
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ],
                ),
              ),
              const Icon(Icons.chevron_right),
            ],
          ),
        ),
      ),
    );
  }

  Color _getPriorityColor(String priority) {
    switch (priority) {
      case 'high':
        return Colors.red;
      case 'medium':
        return Colors.orange;
      default:
        return Colors.blue;
    }
  }
}

class _PriorityBadge extends StatelessWidget {
  final String priority;

  const _PriorityBadge({required this.priority});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final color = _getPriorityColor(priority);
    final label = _getPriorityLabel(priority);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: color),
      ),
      child: Text(
        label,
        style: theme.textTheme.labelSmall?.copyWith(
          color: color,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Color _getPriorityColor(String priority) {
    switch (priority) {
      case 'high':
        return Colors.red;
      case 'medium':
        return Colors.orange;
      default:
        return Colors.blue;
    }
  }

  String _getPriorityLabel(String priority) {
    switch (priority) {
      case 'high':
        return 'ALTA';
      case 'medium':
        return 'MEDIA';
      default:
        return 'BAJA';
    }
  }
}

class _MarketInsightCard extends StatelessWidget {
  final String title;
  final String description;
  final IconData icon;
  final Color color;

  const _MarketInsightCard({
    required this.title,
    required this.description,
    required this.icon,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(icon, color: color),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    description,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

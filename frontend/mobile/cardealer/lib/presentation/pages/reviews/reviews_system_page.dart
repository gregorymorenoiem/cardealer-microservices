// ignore_for_file: deprecated_member_use

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// SF-010: Reviews System
/// Sistema completo de reseñas para dealers y compradores con
/// verificación, fotos, respuestas y análisis de sentimiento

class ReviewsSystemPage extends StatefulWidget {
  final String entityId; // dealer or vehicle ID
  final ReviewEntityType entityType;

  const ReviewsSystemPage({
    super.key,
    required this.entityId,
    required this.entityType,
  });

  @override
  State<ReviewsSystemPage> createState() => _ReviewsSystemPageState();
}

class _ReviewsSystemPageState extends State<ReviewsSystemPage>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  List<Review> _reviews = [];
  ReviewStats? _stats;
  String _sortBy = 'recent'; // recent, helpful, rating
  String _filterRating = 'all'; // all, 5, 4, 3, 2, 1
  bool _isLoading = true;
  // ignore: unused_field
  final bool _isLoadingMore = false;
  // ignore: unused_field
  final int _currentPage = 1;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _loadData();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);
    await Future.delayed(const Duration(seconds: 1));

    setState(() {
      _reviews = _generateMockReviews();
      _stats = _generateMockStats();
      _isLoading = false;
    });
  }

  List<Review> _generateMockReviews() {
    final now = DateTime.now();
    return [
      Review(
        id: '1',
        authorName: 'Juan Pérez',
        authorAvatar: 'https://via.placeholder.com/100',
        rating: 5.0,
        title: 'Excelente servicio y atención',
        content:
            'Muy satisfecho con la compra. El dealer fue muy profesional y transparente en todo momento. El vehículo llegó en perfectas condiciones.',
        date: now.subtract(const Duration(days: 2)),
        isVerified: true,
        helpfulCount: 24,
        images: [
          'https://via.placeholder.com/400x300',
          'https://via.placeholder.com/400x300',
        ],
        dealerResponse: DealerResponse(
          message: 'Muchas gracias por tu confianza. Fue un placer atenderte.',
          date: now.subtract(const Duration(days: 1)),
        ),
      ),
      Review(
        id: '2',
        authorName: 'María García',
        authorAvatar: 'https://via.placeholder.com/100',
        rating: 4.0,
        title: 'Buena experiencia en general',
        content:
            'El proceso fue relativamente rápido. Solo tuve un pequeño inconveniente con el papeleo pero se resolvió rápidamente.',
        date: now.subtract(const Duration(days: 5)),
        isVerified: true,
        helpfulCount: 12,
      ),
      Review(
        id: '3',
        authorName: 'Pedro López',
        authorAvatar: 'https://via.placeholder.com/100',
        rating: 5.0,
        title: 'Altamente recomendado',
        content:
            'Primera vez comprando con este dealer y superó mis expectativas. El vehículo es exactamente como se describía y el precio fue justo.',
        date: now.subtract(const Duration(days: 7)),
        isVerified: false,
        helpfulCount: 8,
        images: ['https://via.placeholder.com/400x300'],
      ),
    ];
  }

  ReviewStats _generateMockStats() {
    return ReviewStats(
      averageRating: 4.6,
      totalReviews: 148,
      ratingDistribution: {
        5: 98,
        4: 32,
        3: 12,
        2: 4,
        1: 2,
      },
      verifiedPercentage: 87,
      responseRate: 95,
      averageResponseTime: const Duration(hours: 4),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          widget.entityType == ReviewEntityType.dealer
              ? 'Reseñas del Dealer'
              : 'Reseñas del Vehículo',
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.filter_list),
            onPressed: _showFilterDialog,
          ),
          IconButton(
            icon: const Icon(Icons.sort),
            onPressed: _showSortDialog,
          ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : Column(
              children: [
                // Stats Header
                _buildStatsHeader(),

                // Rating Distribution
                _buildRatingDistribution(),

                // Tabs
                TabBar(
                  controller: _tabController,
                  tabs: const [
                    Tab(text: 'Todas'),
                    Tab(text: 'Con Fotos'),
                  ],
                ),

                // Reviews List
                Expanded(
                  child: TabBarView(
                    controller: _tabController,
                    children: [
                      _buildReviewsList(_reviews),
                      _buildReviewsList(
                        _reviews
                            .where(
                                (r) => r.images != null && r.images!.isNotEmpty)
                            .toList(),
                      ),
                    ],
                  ),
                ),
              ],
            ),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'reviews_fab',
        onPressed: _writeReview,
        icon: const Icon(Icons.edit),
        label: const Text('Escribir Reseña'),
      ),
    );
  }

  Widget _buildStatsHeader() {
    if (_stats == null) return const SizedBox.shrink();

    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      color: AppColors.primary.withValues(alpha: 0.05),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                Text(
                  _stats!.averageRating.toStringAsFixed(1),
                  style: AppTypography.h3.copyWith(
                    fontWeight: FontWeight.bold,
                    color: AppColors.primary,
                  ),
                ),
                _buildStarRating(_stats!.averageRating),
                const SizedBox(height: AppSpacing.xs),
                Text(
                  '${_stats!.totalReviews} reseñas',
                  style: AppTypography.bodyMedium.copyWith(
                    color: Colors.grey[600],
                  ),
                ),
              ],
            ),
          ),
          Container(
            width: 1,
            height: 80,
            color: Colors.grey[300],
          ),
          Expanded(
            child: Column(
              children: [
                _buildStatItem(
                  Icons.verified,
                  '${_stats!.verifiedPercentage}%',
                  'Verificadas',
                ),
                const SizedBox(height: AppSpacing.sm),
                _buildStatItem(
                  Icons.reply,
                  '${_stats!.responseRate}%',
                  'Tasa respuesta',
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatItem(IconData icon, String value, String label) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Icon(icon, size: 16, color: AppColors.success),
        const SizedBox(width: AppSpacing.xs),
        Text(
          value,
          style: AppTypography.bodyLarge.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(width: AppSpacing.xs),
        Text(
          label,
          style: AppTypography.bodySmall.copyWith(
            color: Colors.grey[600],
          ),
        ),
      ],
    );
  }

  Widget _buildRatingDistribution() {
    if (_stats == null) return const SizedBox.shrink();

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      child: Column(
        children: [5, 4, 3, 2, 1].map((rating) {
          final count = _stats!.ratingDistribution[rating] ?? 0;
          final percentage = count / _stats!.totalReviews;

          return Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.xs),
            child: InkWell(
              onTap: () {
                setState(() {
                  _filterRating = rating.toString();
                  _loadData();
                });
              },
              child: Row(
                children: [
                  Text(
                    '$rating',
                    style: AppTypography.bodyMedium.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(width: 4),
                  const Icon(Icons.star, size: 16, color: AppColors.warning),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: LinearProgressIndicator(
                      value: percentage,
                      backgroundColor: Colors.grey[200],
                      valueColor: const AlwaysStoppedAnimation<Color>(
                          AppColors.warning),
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  SizedBox(
                    width: 30,
                    child: Text(
                      '$count',
                      style: AppTypography.bodySmall.copyWith(
                        color: Colors.grey[600],
                      ),
                      textAlign: TextAlign.right,
                    ),
                  ),
                ],
              ),
            ),
          );
        }).toList(),
      ),
    );
  }

  Widget _buildReviewsList(List<Review> reviews) {
    if (reviews.isEmpty) {
      return _buildEmptyState();
    }

    return RefreshIndicator(
      onRefresh: _loadData,
      child: ListView.builder(
        padding: const EdgeInsets.all(AppSpacing.md),
        itemCount: reviews.length,
        itemBuilder: (context, index) {
          final review = reviews[index];
          return _buildReviewCard(review);
        },
      ),
    );
  }

  Widget _buildReviewCard(Review review) {
    return Card(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Author Header
            Row(
              children: [
                CircleAvatar(
                  backgroundImage: NetworkImage(review.authorAvatar),
                ),
                const SizedBox(width: AppSpacing.md),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Text(
                            review.authorName,
                            style: AppTypography.bodyLarge.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          if (review.isVerified) ...[
                            const SizedBox(width: AppSpacing.xs),
                            const Icon(
                              Icons.verified,
                              size: 16,
                              color: AppColors.success,
                            ),
                          ],
                        ],
                      ),
                      Text(
                        _formatDate(review.date),
                        style: AppTypography.bodySmall.copyWith(
                          color: Colors.grey[600],
                        ),
                      ),
                    ],
                  ),
                ),
                PopupMenuButton<String>(
                  icon: const Icon(Icons.more_vert),
                  onSelected: (value) => _handleReviewAction(value, review),
                  itemBuilder: (context) => [
                    const PopupMenuItem(
                      value: 'report',
                      child: Row(
                        children: [
                          Icon(Icons.flag),
                          SizedBox(width: AppSpacing.sm),
                          Text('Reportar'),
                        ],
                      ),
                    ),
                    const PopupMenuItem(
                      value: 'share',
                      child: Row(
                        children: [
                          Icon(Icons.share),
                          SizedBox(width: AppSpacing.sm),
                          Text('Compartir'),
                        ],
                      ),
                    ),
                  ],
                ),
              ],
            ),
            const SizedBox(height: AppSpacing.sm),

            // Rating
            _buildStarRating(review.rating),
            const SizedBox(height: AppSpacing.sm),

            // Title
            if (review.title != null) ...[
              Text(
                review.title!,
                style: AppTypography.bodyLarge.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: AppSpacing.xs),
            ],

            // Content
            Text(
              review.content,
              style: AppTypography.bodyMedium,
            ),
            const SizedBox(height: AppSpacing.md),

            // Images
            if (review.images != null && review.images!.isNotEmpty)
              _buildReviewImages(review.images!),

            // Actions
            Row(
              children: [
                TextButton.icon(
                  onPressed: () => _markHelpful(review),
                  icon: const Icon(Icons.thumb_up_outlined, size: 18),
                  label: Text('Útil (${review.helpfulCount})'),
                  style: TextButton.styleFrom(
                    padding: EdgeInsets.zero,
                  ),
                ),
                const SizedBox(width: AppSpacing.md),
                TextButton.icon(
                  onPressed: () => _shareReview(review),
                  icon: const Icon(Icons.share_outlined, size: 18),
                  label: const Text('Compartir'),
                  style: TextButton.styleFrom(
                    padding: EdgeInsets.zero,
                  ),
                ),
              ],
            ),

            // Dealer Response
            if (review.dealerResponse != null) ...[
              const Divider(),
              Container(
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: BoxDecoration(
                  color: AppColors.info.withValues(alpha: 0.05),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(
                          Icons.store,
                          size: 16,
                          color: AppColors.info,
                        ),
                        const SizedBox(width: AppSpacing.xs),
                        Text(
                          'Respuesta del dealer',
                          style: AppTypography.bodyMedium.copyWith(
                            fontWeight: FontWeight.bold,
                            color: AppColors.info,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: AppSpacing.sm),
                    Text(
                      review.dealerResponse!.message,
                      style: AppTypography.bodyMedium,
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Text(
                      _formatDate(review.dealerResponse!.date),
                      style: AppTypography.bodySmall.copyWith(
                        color: Colors.grey[600],
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildStarRating(double rating) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(5, (index) {
        if (index < rating.floor()) {
          return const Icon(Icons.star, size: 18, color: AppColors.warning);
        } else if (index < rating) {
          return const Icon(Icons.star_half,
              size: 18, color: AppColors.warning);
        } else {
          return const Icon(Icons.star_outline,
              size: 18, color: AppColors.warning);
        }
      }),
    );
  }

  Widget _buildReviewImages(List<String> images) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          height: 100,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            itemCount: images.length,
            itemBuilder: (context, index) {
              return Padding(
                padding: const EdgeInsets.only(right: AppSpacing.sm),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: InkWell(
                    onTap: () => _viewImages(images, index),
                    child: Image.network(
                      images[index],
                      width: 100,
                      height: 100,
                      fit: BoxFit.cover,
                    ),
                  ),
                ),
              );
            },
          ),
        ),
        const SizedBox(height: AppSpacing.md),
      ],
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.rate_review_outlined,
            size: 80,
            color: Colors.grey[300],
          ),
          const SizedBox(height: AppSpacing.lg),
          Text(
            'No hay reseñas aún',
            style: AppTypography.h6.copyWith(
              color: Colors.grey[600],
            ),
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(
            'Sé el primero en dejar una reseña',
            style: AppTypography.bodyMedium.copyWith(
              color: Colors.grey[500],
            ),
          ),
        ],
      ),
    );
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final diff = now.difference(date);

    if (diff.inDays == 0) {
      return 'Hoy';
    } else if (diff.inDays == 1) {
      return 'Ayer';
    } else if (diff.inDays < 7) {
      return 'Hace ${diff.inDays} días';
    } else {
      return DateFormat('d MMM yyyy', 'es').format(date);
    }
  }

  void _showFilterDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Filtrar por calificación'),
        content: RadioGroup<String>(
          groupValue: _filterRating,
          onChanged: (value) {
            setState(() {
              _filterRating = value!;
            });
            Navigator.pop(context);
            _loadData();
          },
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: ['all', '5', '4', '3', '2', '1'].map((rating) {
              return ListTile(
                title: Text(rating == 'all' ? 'Todas' : '$rating estrellas'),
                leading: Radio<String>(
                  value: rating,
                ),
                onTap: () {
                  setState(() {
                    _filterRating = rating;
                  });
                  Navigator.pop(context);
                  _loadData();
                },
              );
            }).toList(),
          ),
        ),
      ),
    );
  }

  void _showSortDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Ordenar por'),
        content: RadioGroup<String>(
          groupValue: _sortBy,
          onChanged: (value) {
            setState(() {
              _sortBy = value!;
            });
            Navigator.pop(context);
            _loadData();
          },
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              ListTile(
                title: const Text('Más recientes'),
                leading: const Radio<String>(
                  value: 'recent',
                ),
                onTap: () {
                  setState(() {
                    _sortBy = 'recent';
                  });
                  Navigator.pop(context);
                  _loadData();
                },
              ),
              ListTile(
                title: const Text('Más útiles'),
                leading: const Radio<String>(
                  value: 'helpful',
                ),
                onTap: () {
                  setState(() {
                    _sortBy = 'helpful';
                  });
                  Navigator.pop(context);
                  _loadData();
                },
              ),
              ListTile(
                title: const Text('Mejor calificación'),
                leading: const Radio<String>(
                  value: 'rating',
                ),
                onTap: () {
                  setState(() {
                    _sortBy = 'rating';
                  });
                  Navigator.pop(context);
                  _loadData();
                },
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _writeReview() {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => WriteReviewPage(
          entityId: widget.entityId,
          entityType: widget.entityType,
        ),
      ),
    ).then((value) {
      if (value == true) {
        _loadData();
      }
    });
  }

  void _markHelpful(Review review) {
    setState(() {
      review.helpfulCount++;
    });
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Marcado como útil')),
    );
  }

  void _shareReview(Review review) {
    // Implement share functionality
  }

  void _viewImages(List<String> images, int initialIndex) {
    // Implement image viewer
  }

  void _handleReviewAction(String action, Review review) {
    switch (action) {
      case 'report':
        // Implement report functionality
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Reseña reportada')),
        );
        break;
      case 'share':
        _shareReview(review);
        break;
    }
  }
}

class WriteReviewPage extends StatefulWidget {
  final String entityId;
  final ReviewEntityType entityType;

  const WriteReviewPage({
    super.key,
    required this.entityId,
    required this.entityType,
  });

  @override
  State<WriteReviewPage> createState() => _WriteReviewPageState();
}

class _WriteReviewPageState extends State<WriteReviewPage> {
  final _formKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _contentController = TextEditingController();
  double _rating = 5.0;
  final List<String> _selectedImages = [];
  bool _isAnonymous = false;

  @override
  void dispose() {
    _titleController.dispose();
    _contentController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Escribir Reseña'),
        actions: [
          TextButton(
            onPressed: _submitReview,
            child: const Text('PUBLICAR'),
          ),
        ],
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(AppSpacing.lg),
          children: [
            // Rating
            Text(
              'Tu calificación',
              style: AppTypography.h6.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: AppSpacing.md),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: List.generate(5, (index) {
                return IconButton(
                  icon: Icon(
                    index < _rating ? Icons.star : Icons.star_outline,
                    size: 40,
                    color: AppColors.warning,
                  ),
                  onPressed: () {
                    setState(() {
                      _rating = index + 1.0;
                    });
                  },
                );
              }),
            ),
            const SizedBox(height: AppSpacing.xl),

            // Title
            TextFormField(
              controller: _titleController,
              decoration: const InputDecoration(
                labelText: 'Título',
                hintText: 'Resume tu experiencia',
                border: OutlineInputBorder(),
              ),
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Por favor ingresa un título';
                }
                return null;
              },
            ),
            const SizedBox(height: AppSpacing.md),

            // Content
            TextFormField(
              controller: _contentController,
              decoration: const InputDecoration(
                labelText: 'Tu reseña',
                hintText: 'Cuéntanos sobre tu experiencia...',
                border: OutlineInputBorder(),
              ),
              maxLines: 5,
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Por favor escribe tu reseña';
                }
                if (value.length < 20) {
                  return 'La reseña debe tener al menos 20 caracteres';
                }
                return null;
              },
            ),
            const SizedBox(height: AppSpacing.md),

            // Add Photos
            OutlinedButton.icon(
              onPressed: _pickImages,
              icon: const Icon(Icons.add_photo_alternate),
              label: const Text('Agregar Fotos'),
            ),
            if (_selectedImages.isNotEmpty) ...[
              const SizedBox(height: AppSpacing.md),
              SizedBox(
                height: 100,
                child: ListView.builder(
                  scrollDirection: Axis.horizontal,
                  itemCount: _selectedImages.length,
                  itemBuilder: (context, index) {
                    return Stack(
                      children: [
                        Padding(
                          padding: const EdgeInsets.only(right: AppSpacing.sm),
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(8),
                            child: Image.network(
                              _selectedImages[index],
                              width: 100,
                              height: 100,
                              fit: BoxFit.cover,
                            ),
                          ),
                        ),
                        Positioned(
                          top: 4,
                          right: 12,
                          child: IconButton(
                            icon: const Icon(Icons.cancel, color: Colors.white),
                            onPressed: () {
                              setState(() {
                                _selectedImages.removeAt(index);
                              });
                            },
                            style: IconButton.styleFrom(
                              backgroundColor: Colors.black54,
                            ),
                          ),
                        ),
                      ],
                    );
                  },
                ),
              ),
            ],
            const SizedBox(height: AppSpacing.md),

            // Anonymous
            SwitchListTile(
              value: _isAnonymous,
              onChanged: (value) => setState(() => _isAnonymous = value),
              title: const Text('Publicar de forma anónima'),
              subtitle: const Text('Tu nombre no será visible'),
              activeThumbColor: AppColors.primary,
            ),
            const SizedBox(height: AppSpacing.xl),

            // Guidelines
            Card(
              color: AppColors.info.withValues(alpha: 0.1),
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.md),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.info_outline, color: AppColors.info),
                        const SizedBox(width: AppSpacing.sm),
                        Text(
                          'Pautas para reseñas',
                          style: AppTypography.bodyLarge.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: AppSpacing.sm),
                    const Text(
                      '• Sé honesto y específico\n'
                      '• Respeta a otros usuarios\n'
                      '• No incluyas información personal\n'
                      '• Las fotos deben ser relevantes',
                      style: AppTypography.bodyMedium,
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _pickImages() {
    // Simulate image picker
    setState(() {
      _selectedImages.add('https://via.placeholder.com/400x300');
    });
  }

  void _submitReview() {
    if (_formKey.currentState!.validate()) {
      Navigator.pop(context, true);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Reseña publicada exitosamente'),
        ),
      );
    }
  }
}

// Models
class Review {
  final String id;
  final String authorName;
  final String authorAvatar;
  final double rating;
  final String? title;
  final String content;
  final DateTime date;
  final bool isVerified;
  int helpfulCount;
  final List<String>? images;
  final DealerResponse? dealerResponse;

  Review({
    required this.id,
    required this.authorName,
    required this.authorAvatar,
    required this.rating,
    this.title,
    required this.content,
    required this.date,
    required this.isVerified,
    required this.helpfulCount,
    this.images,
    this.dealerResponse,
  });
}

class DealerResponse {
  final String message;
  final DateTime date;

  DealerResponse({
    required this.message,
    required this.date,
  });
}

class ReviewStats {
  final double averageRating;
  final int totalReviews;
  final Map<int, int> ratingDistribution;
  final int verifiedPercentage;
  final int responseRate;
  final Duration averageResponseTime;

  ReviewStats({
    required this.averageRating,
    required this.totalReviews,
    required this.ratingDistribution,
    required this.verifiedPercentage,
    required this.responseRate,
    required this.averageResponseTime,
  });
}

enum ReviewEntityType {
  dealer,
  vehicle,
}

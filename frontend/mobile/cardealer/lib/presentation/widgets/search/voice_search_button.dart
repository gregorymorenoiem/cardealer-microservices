import 'package:flutter/material.dart';
import 'package:speech_to_text/speech_to_text.dart' as stt;
import 'package:permission_handler/permission_handler.dart';

/// SE-002: Voice Search Integration
/// Voice search button with speech-to-text functionality
class VoiceSearchButton extends StatefulWidget {
  final Function(String) onVoiceResult;
  final Color? iconColor;

  const VoiceSearchButton({
    super.key,
    required this.onVoiceResult,
    this.iconColor,
  });

  @override
  State<VoiceSearchButton> createState() => _VoiceSearchButtonState();
}

class _VoiceSearchButtonState extends State<VoiceSearchButton>
    with SingleTickerProviderStateMixin {
  late stt.SpeechToText _speech;
  bool _isListening = false;
  bool _isAvailable = false;
  late AnimationController _animationController;
  late Animation<double> _pulseAnimation;

  @override
  void initState() {
    super.initState();
    _speech = stt.SpeechToText();
    _initializeSpeech();

    _animationController = AnimationController(
      duration: const Duration(milliseconds: 1000),
      vsync: this,
    )..repeat(reverse: true);

    _pulseAnimation = Tween<double>(
      begin: 1.0,
      end: 1.3,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeInOut,
    ));
  }

  Future<void> _initializeSpeech() async {
    final status = await Permission.microphone.request();
    if (status.isGranted) {
      _isAvailable = await _speech.initialize(
        onError: (error) {
          debugPrint('Speech recognition error: $error');
          setState(() => _isListening = false);
        },
        onStatus: (status) {
          debugPrint('Speech recognition status: $status');
          if (status == 'done' || status == 'notListening') {
            setState(() => _isListening = false);
          }
        },
      );
      setState(() {});
    }
  }

  @override
  void dispose() {
    _animationController.dispose();
    _speech.cancel();
    super.dispose();
  }

  Future<void> _startListening() async {
    if (!_isAvailable) {
      _showPermissionDialog();
      return;
    }

    setState(() => _isListening = true);

    await _speech.listen(
      onResult: (result) {
        if (result.finalResult) {
          final recognizedWords = result.recognizedWords;
          if (recognizedWords.isNotEmpty) {
            widget.onVoiceResult(recognizedWords);
          }
          setState(() => _isListening = false);
        }
      },
      listenFor: const Duration(seconds: 30),
      pauseFor: const Duration(seconds: 3),
      listenOptions: stt.SpeechListenOptions(
        partialResults: true,
        cancelOnError: true,
        listenMode: stt.ListenMode.confirmation,
      ),
    );
  }

  Future<void> _stopListening() async {
    await _speech.stop();
    setState(() => _isListening = false);
  }

  void _showPermissionDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Microphone Permission'),
        content: const Text(
          'This app needs microphone access to enable voice search. '
          'Please grant permission in your device settings.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              openAppSettings();
            },
            child: const Text('Open Settings'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_isListening) {
      return ScaleTransition(
        scale: _pulseAnimation,
        child: IconButton(
          onPressed: _stopListening,
          icon: Icon(
            Icons.mic,
            color: Theme.of(context).colorScheme.error,
          ),
          tooltip: 'Stop listening',
        ),
      );
    }

    return IconButton(
      onPressed: _startListening,
      icon: Icon(
        Icons.mic_outlined,
        color: widget.iconColor ?? Colors.grey.shade600,
      ),
      tooltip: 'Voice search',
    );
  }
}

/// Voice Search Dialog - Full screen listening experience
class VoiceSearchDialog extends StatefulWidget {
  final Function(String) onResult;

  const VoiceSearchDialog({
    super.key,
    required this.onResult,
  });

  @override
  State<VoiceSearchDialog> createState() => _VoiceSearchDialogState();
}

class _VoiceSearchDialogState extends State<VoiceSearchDialog>
    with SingleTickerProviderStateMixin {
  late stt.SpeechToText _speech;
  bool _isListening = false;
  String _currentText = '';
  late AnimationController _animationController;

  @override
  void initState() {
    super.initState();
    _speech = stt.SpeechToText();
    _initAndStart();

    _animationController = AnimationController(
      duration: const Duration(milliseconds: 1200),
      vsync: this,
    )..repeat();
  }

  Future<void> _initAndStart() async {
    final status = await Permission.microphone.request();
    if (status.isGranted) {
      final available = await _speech.initialize();
      if (available && mounted) {
        _startListening();
      }
    } else {
      if (mounted) {
        Navigator.pop(context);
      }
    }
  }

  Future<void> _startListening() async {
    setState(() => _isListening = true);

    await _speech.listen(
      onResult: (result) {
        setState(() {
          _currentText = result.recognizedWords;
        });

        if (result.finalResult && _currentText.isNotEmpty) {
          widget.onResult(_currentText);
          Navigator.pop(context);
        }
      },
      listenFor: const Duration(seconds: 30),
      pauseFor: const Duration(seconds: 3),
      listenOptions: stt.SpeechListenOptions(
        partialResults: true,
        cancelOnError: true,
      ),
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    _speech.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Dialog.fullscreen(
      backgroundColor: Colors.black.withValues(alpha: 0.9),
      child: SafeArea(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Close button
            Align(
              alignment: Alignment.topRight,
              child: IconButton(
                onPressed: () => Navigator.pop(context),
                icon: const Icon(
                  Icons.close,
                  color: Colors.white,
                  size: 32,
                ),
              ),
            ),

            const Spacer(),

            // Animated microphone icon
            Stack(
              alignment: Alignment.center,
              children: [
                // Outer pulse rings
                for (int i = 0; i < 3; i++)
                  AnimatedBuilder(
                    animation: _animationController,
                    builder: (context, child) {
                      final delay = i * 0.3;
                      final delayedValue =
                          (_animationController.value - delay).clamp(0.0, 1.0);

                      return Transform.scale(
                        scale: 1.0 + (delayedValue * 0.5),
                        child: Opacity(
                          opacity: 1.0 - delayedValue,
                          child: Container(
                            width: 200,
                            height: 200,
                            decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              border: Border.all(
                                color: Theme.of(context).primaryColor,
                                width: 2,
                              ),
                            ),
                          ),
                        ),
                      );
                    },
                  ),

                // Main microphone circle
                Container(
                  width: 120,
                  height: 120,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: Theme.of(context).primaryColor,
                    boxShadow: [
                      BoxShadow(
                        color: Theme.of(context)
                            .primaryColor
                            .withValues(alpha: 0.5),
                        blurRadius: 30,
                        spreadRadius: 10,
                      ),
                    ],
                  ),
                  child: Icon(
                    _isListening ? Icons.mic : Icons.mic_off,
                    color: Colors.white,
                    size: 60,
                  ),
                ),
              ],
            ),

            const SizedBox(height: 40),

            // Status text
            Text(
              _isListening ? 'Listening...' : 'Tap to speak',
              style: const TextStyle(
                color: Colors.white,
                fontSize: 24,
                fontWeight: FontWeight.w600,
              ),
            ),

            const SizedBox(height: 20),

            // Recognized text
            Container(
              margin: const EdgeInsets.symmetric(horizontal: 40),
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(16),
              ),
              constraints: const BoxConstraints(minHeight: 80),
              child: Center(
                child: Text(
                  _currentText.isEmpty
                      ? 'Say something like "Honda Civic 2020"'
                      : _currentText,
                  style: TextStyle(
                    color: _currentText.isEmpty
                        ? Colors.white.withValues(alpha: 0.5)
                        : Colors.white,
                    fontSize: 18,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
            ),

            const Spacer(),

            // Hint text
            Padding(
              padding: const EdgeInsets.all(20),
              child: Text(
                'Speak clearly and wait for the result',
                style: TextStyle(
                  color: Colors.white.withValues(alpha: 0.6),
                  fontSize: 14,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

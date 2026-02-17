import { motion } from 'framer-motion';
import { Check, X } from 'lucide-react';

interface Spec {
  label: string;
  value: string;
  icon?: React.ReactNode;
}

interface Feature {
  label: string;
  available: boolean;
}

interface OklaSpecsTableProps {
  specs: Spec[];
  features?: Feature[];
  variant?: 'grid' | 'list' | 'compact';
  title?: string;
  featuresTitle?: string;
}

export const OklaSpecsTable = ({
  specs,
  features,
  variant = 'grid',
  title = 'Especificaciones',
  featuresTitle = 'Características',
}: OklaSpecsTableProps) => {
  if (variant === 'compact') {
    return (
      <div className="flex flex-wrap gap-3">
        {specs.map((spec, index) => (
          <motion.div
            key={index}
            className="flex items-center gap-2 px-3 py-2 bg-okla-cream/50 rounded-lg"
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: index * 0.05 }}
          >
            {spec.icon && <span className="text-okla-gold">{spec.icon}</span>}
            <span className="text-xs text-okla-slate">{spec.label}</span>
            <span className="text-sm font-semibold text-okla-navy">{spec.value}</span>
          </motion.div>
        ))}
      </div>
    );
  }

  if (variant === 'list') {
    return (
      <div className="space-y-6">
        {/* Specs List */}
        <div>
          <h3 className="text-lg font-display font-bold text-okla-navy mb-4">{title}</h3>
          <div className="space-y-2">
            {specs.map((spec, index) => (
              <motion.div
                key={index}
                className="flex justify-between items-center py-3 border-b border-okla-cream last:border-b-0"
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: index * 0.03 }}
              >
                <span className="text-okla-slate flex items-center gap-2">
                  {spec.icon && <span className="text-okla-gold">{spec.icon}</span>}
                  {spec.label}
                </span>
                <span className="font-semibold text-okla-navy">{spec.value}</span>
              </motion.div>
            ))}
          </div>
        </div>

        {/* Features List */}
        {features && features.length > 0 && (
          <div>
            <h3 className="text-lg font-display font-bold text-okla-navy mb-4">{featuresTitle}</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
              {features.map((feature, index) => (
                <motion.div
                  key={index}
                  className={`flex items-center gap-3 p-3 rounded-lg ${
                    feature.available ? 'bg-green-50' : 'bg-gray-50'
                  }`}
                  initial={{ opacity: 0, y: 5 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.02 }}
                >
                  {feature.available ? (
                    <Check className="w-5 h-5 text-green-600 flex-shrink-0" />
                  ) : (
                    <X className="w-5 h-5 text-gray-400 flex-shrink-0" />
                  )}
                  <span
                    className={feature.available ? 'text-okla-navy' : 'text-gray-400 line-through'}
                  >
                    {feature.label}
                  </span>
                </motion.div>
              ))}
            </div>
          </div>
        )}
      </div>
    );
  }

  // Grid variant (default)
  return (
    <div className="space-y-6">
      {/* Specs Grid */}
      <div>
        <h3 className="text-lg font-display font-bold text-okla-navy mb-4">{title}</h3>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {specs.map((spec, index) => (
            <motion.div
              key={index}
              className="bg-okla-cream/50 rounded-xl p-4 text-center hover:bg-okla-cream transition-colors"
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: index * 0.04 }}
              whileHover={{ scale: 1.02 }}
            >
              {spec.icon && (
                <div className="w-10 h-10 bg-okla-gold/10 rounded-full flex items-center justify-center mx-auto mb-2">
                  <span className="text-okla-gold">{spec.icon}</span>
                </div>
              )}
              <p className="text-xs text-okla-slate mb-1">{spec.label}</p>
              <p className="font-bold text-okla-navy">{spec.value}</p>
            </motion.div>
          ))}
        </div>
      </div>

      {/* Features Grid */}
      {features && features.length > 0 && (
        <div>
          <h3 className="text-lg font-display font-bold text-okla-navy mb-4">{featuresTitle}</h3>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
            {features.map((feature, index) => (
              <motion.div
                key={index}
                className={`flex items-center gap-2 p-3 rounded-lg border ${
                  feature.available
                    ? 'border-green-200 bg-green-50'
                    : 'border-gray-200 bg-gray-50'
                }`}
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: index * 0.02 }}
              >
                {feature.available ? (
                  <div className="w-6 h-6 rounded-full bg-green-500 flex items-center justify-center flex-shrink-0">
                    <Check className="w-4 h-4 text-white" />
                  </div>
                ) : (
                  <div className="w-6 h-6 rounded-full bg-gray-300 flex items-center justify-center flex-shrink-0">
                    <X className="w-4 h-4 text-white" />
                  </div>
                )}
                <span
                  className={`text-sm ${
                    feature.available ? 'text-okla-navy' : 'text-gray-400'
                  }`}
                >
                  {feature.label}
                </span>
              </motion.div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default OklaSpecsTable;

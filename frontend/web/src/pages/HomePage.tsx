/**
 * HomePage - Main marketplace landing page
 * Multi-vertical marketplace with clean, scalable design
 */

import React from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { FiArrowRight, FiSearch, FiShield, FiStar, FiUsers } from 'react-icons/fi';
import { FaCar, FaHome } from 'react-icons/fa';

const HomePage: React.FC = () => {
  const { t } = useTranslation('common');

  // Vertical categories configuration - using translations
  const verticals = [
    {
      id: 'vehicles',
      name: t('home.verticals.vehicles.name'),
      description: t('home.verticals.vehicles.description'),
      icon: FaCar,
      href: '/vehicles',
      color: 'blue',
      stats: t('home.verticals.vehicles.stats'),
      gradient: 'from-blue-500 to-blue-600',
      bgLight: 'bg-blue-50',
      textColor: 'text-blue-600',
    },
    {
      id: 'properties',
      name: t('home.verticals.properties.name'),
      description: t('home.verticals.properties.description'),
      icon: FaHome,
      href: '/properties',
      color: 'emerald',
      stats: t('home.verticals.properties.stats'),
      gradient: 'from-emerald-500 to-emerald-600',
      bgLight: 'bg-emerald-50',
      textColor: 'text-emerald-600',
    },
  ];

  const features = [
    {
      icon: FiShield,
      title: t('home.features.verified.title'),
      description: t('home.features.verified.description'),
    },
    {
      icon: FiSearch,
      title: t('home.features.search.title'),
      description: t('home.features.search.description'),
    },
    {
      icon: FiUsers,
      title: t('home.features.support.title'),
      description: t('home.features.support.description'),
    },
    {
      icon: FiStar,
      title: t('home.features.reviews.title'),
      description: t('home.features.reviews.description'),
    },
  ];

  const stats = [
    { value: '15,000+', label: t('home.stats.activeListings') },
    { value: '8,500+', label: t('home.stats.happyCustomers') },
    { value: '250+', label: t('home.stats.verifiedSellers') },
    { value: '50+', label: t('home.stats.cities') },
  ];
  return (
    <MainLayout>
      {/* Hero Section - Clean and focused */}
      <section className="relative bg-gradient-to-b from-gray-50 to-white overflow-hidden">
        {/* Subtle background decoration */}
        <div className="absolute inset-0 overflow-hidden">
          <div className="absolute -top-40 -right-40 w-80 h-80 bg-blue-100 rounded-full opacity-50 blur-3xl" />
          <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-emerald-100 rounded-full opacity-50 blur-3xl" />
        </div>

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16 lg:py-24">
          <div className="text-center max-w-3xl mx-auto mb-12">
            <motion.h1
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="text-4xl sm:text-5xl lg:text-6xl font-bold text-gray-900 mb-6"
            >
              {t('home.hero.title')}{' '}
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-600 to-emerald-600">
                {t('home.hero.highlight')}
              </span>
            </motion.h1>
            <motion.p
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 }}
              className="text-xl text-gray-600"
            >
              {t('home.hero.subtitle')}
            </motion.p>
          </div>

          {/* Category Cards - Main Navigation */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-4xl mx-auto"
          >
            {verticals.map((vertical) => (
              <Link
                key={vertical.id}
                to={vertical.href}
                className="group relative bg-white rounded-2xl p-6 shadow-lg hover:shadow-xl transition-all duration-300 border border-gray-100 overflow-hidden"
              >
                {/* Hover gradient overlay */}
                <div className={`absolute inset-0 bg-gradient-to-br ${vertical.gradient} opacity-0 group-hover:opacity-5 transition-opacity`} />
                
                <div className="relative flex items-start gap-4">
                  <div className={`flex-shrink-0 w-14 h-14 ${vertical.bgLight} rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform`}>
                    <vertical.icon className={`w-7 h-7 ${vertical.textColor}`} />
                  </div>
                  
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between mb-1">
                      <h3 className="text-xl font-bold text-gray-900 group-hover:text-gray-800">
                        {vertical.name}
                      </h3>
                      <FiArrowRight className="w-5 h-5 text-gray-400 group-hover:text-gray-600 group-hover:translate-x-1 transition-all" />
                    </div>
                    <p className="text-gray-600 text-sm mb-2">{vertical.description}</p>
                    <span className={`inline-flex items-center text-xs font-medium ${vertical.textColor} ${vertical.bgLight} px-2 py-1 rounded-full`}>
                      {vertical.stats}
                    </span>
                  </div>
                </div>
              </Link>
            ))}
          </motion.div>

          {/* Quick search hint */}
          <motion.p
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.4 }}
            className="text-center text-gray-500 text-sm mt-8"
          >
            {t('home.hero.searchHint')}
          </motion.p>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-16 lg:py-24 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">
              {t('home.whyUs.title')}
            </h2>
            <p className="text-gray-600 max-w-2xl mx-auto">
              {t('home.whyUs.subtitle')}
            </p>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
            {features.map((feature, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="text-center p-6"
              >
                <div className="w-14 h-14 bg-gray-100 rounded-xl flex items-center justify-center mx-auto mb-4">
                  <feature.icon className="w-6 h-6 text-gray-700" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {feature.title}
                </h3>
                <p className="text-gray-600 text-sm">
                  {feature.description}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="py-16 lg:py-20 bg-gradient-to-br from-gray-900 to-gray-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-8">
            {stats.map((stat, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, scale: 0.9 }}
                whileInView={{ opacity: 1, scale: 1 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="text-center"
              >
                <p className="text-3xl lg:text-4xl font-bold text-white mb-2">
                  {stat.value}
                </p>
                <p className="text-gray-400 text-sm">
                  {stat.label}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* How it Works */}
      <section className="py-16 lg:py-24 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">
              {t('home.howItWorks.title')}
            </h2>
            <p className="text-gray-600">
              {t('home.howItWorks.subtitle')}
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 max-w-4xl mx-auto">
            {[
              { step: '1', icon: '🔍', title: t('home.howItWorks.step1.title'), desc: t('home.howItWorks.step1.description') },
              { step: '2', icon: '💬', title: t('home.howItWorks.step2.title'), desc: t('home.howItWorks.step2.description') },
              { step: '3', icon: '✅', title: t('home.howItWorks.step3.title'), desc: t('home.howItWorks.step3.description') },
            ].map((item, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="relative text-center"
              >
                {/* Connector line */}
                {index < 2 && (
                  <div className="hidden md:block absolute top-10 left-[60%] w-[80%] h-0.5 bg-gray-200" />
                )}
                
                <div className="relative z-10 w-20 h-20 bg-white rounded-2xl shadow-md flex items-center justify-center mx-auto mb-4 text-3xl">
                  {item.icon}
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {item.title}
                </h3>
                <p className="text-gray-600 text-sm">
                  {item.desc}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-16 lg:py-24 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-gradient-to-br from-gray-50 to-gray-100 rounded-3xl p-8 lg:p-12 text-center">
            <h2 className="text-2xl lg:text-3xl font-bold text-gray-900 mb-4">
              {t('home.cta.title')}
            </h2>
            <p className="text-gray-600 mb-8 max-w-xl mx-auto">
              {t('home.cta.subtitle')}
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/vehicles/sell"
                className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-xl transition-colors"
              >
                <FaCar className="w-5 h-5" />
                {t('home.cta.publishVehicle')}
              </Link>
              <Link
                to="/properties/new"
                className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded-xl transition-colors"
              >
                <FaHome className="w-5 h-5" />
                {t('home.cta.publishProperty')}
              </Link>
            </div>
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default HomePage;

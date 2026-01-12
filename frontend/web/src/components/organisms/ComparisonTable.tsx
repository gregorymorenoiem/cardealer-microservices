import React from 'react';
import { FiCheck, FiX } from 'react-icons/fi';
import type { Vehicle } from '@/services/vehicleService';

interface ComparisonTableProps {
  vehicles: Vehicle[];
}

interface ComparisonRow {
  label: string;
  category: string;
  getValue: (vehicle: Vehicle) => string | number | boolean;
  format?: (value: string | number | boolean) => string;
}

const comparisonRows: ComparisonRow[] = [
  // Basic Info
  { label: 'Year', category: 'Basic', getValue: (v) => v.year },
  { label: 'Make', category: 'Basic', getValue: (v) => v.make },
  { label: 'Model', category: 'Basic', getValue: (v) => v.model },
  {
    label: 'Price',
    category: 'Basic',
    getValue: (v) => v.price,
    format: (v) => `$${Number(v).toLocaleString()}`,
  },
  { label: 'Condition', category: 'Basic', getValue: (v) => (v.isNew ? 'New' : 'Used') },

  // Performance
  {
    label: 'Mileage',
    category: 'Performance',
    getValue: (v) => v.mileage,
    format: (v) => `${Number(v).toLocaleString()} mi`,
  },
  { label: 'Transmission', category: 'Performance', getValue: (v) => v.transmission },
  { label: 'Fuel Type', category: 'Performance', getValue: (v) => v.fuelType },
  { label: 'Engine', category: 'Performance', getValue: (v) => v.engine || 'N/A' },
  {
    label: 'Horsepower',
    category: 'Performance',
    getValue: (v) => v.horsepower || 'N/A',
    format: (v) => (v === 'N/A' ? 'N/A' : `${v} hp`),
  },

  // Details
  { label: 'Body Type', category: 'Details', getValue: (v) => v.bodyType || 'N/A' },
  { label: 'Drivetrain', category: 'Details', getValue: (v) => v.drivetrain || 'N/A' },
  { label: 'Exterior Color', category: 'Details', getValue: (v) => v.color || 'N/A' },
  { label: 'Interior Color', category: 'Details', getValue: (v) => v.interiorColor || 'N/A' },
  {
    label: 'MPG',
    category: 'Details',
    getValue: (v) => (v.mpg ? `${v.mpg.city}/${v.mpg.highway}` : 'N/A'),
    format: (v) => (v === 'N/A' ? 'N/A' : `${v} (city/hwy)`),
  },

  // Location & Seller
  { label: 'Location', category: 'Other', getValue: (v) => v.location },
  { label: 'Seller', category: 'Other', getValue: (v) => v.seller?.name || 'N/A' },
];

export default function ComparisonTable({ vehicles }: ComparisonTableProps) {
  // Group rows by category
  const categories = Array.from(new Set(comparisonRows.map((r) => r.category)));

  // Check if values are different across vehicles for highlighting
  const isDifferent = (row: ComparisonRow) => {
    const values = vehicles.map((v) => row.getValue(v));
    return new Set(values).size > 1;
  };

  return (
    <div className="bg-white rounded-xl shadow-card overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="sticky left-0 bg-gray-50 z-10 px-6 py-4 text-left text-sm font-semibold text-gray-900 w-48">
                Specification
              </th>
              {vehicles.map((vehicle) => (
                <th
                  key={vehicle.id}
                  className="px-6 py-4 text-center text-sm font-semibold text-gray-900 min-w-[200px]"
                >
                  {vehicle.year} {vehicle.make} {vehicle.model}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {categories.map((category) => {
              const categoryRows = comparisonRows.filter((r) => r.category === category);

              return (
                <React.Fragment key={category}>
                  {/* Category Header */}
                  <tr className="bg-gray-100">
                    <td
                      colSpan={vehicles.length + 1}
                      className="px-6 py-3 text-sm font-bold text-gray-900"
                    >
                      {category}
                    </td>
                  </tr>

                  {/* Category Rows */}
                  {categoryRows.map((row, index) => {
                    const hasDifference = isDifferent(row);

                    return (
                      <tr
                        key={row.label}
                        className={`
                          border-b border-gray-100
                          ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'}
                          hover:bg-blue-50/50 transition-colors
                        `}
                      >
                        <td className="sticky left-0 bg-inherit z-10 px-6 py-4 text-sm font-medium text-gray-700">
                          {row.label}
                          {hasDifference && (
                            <span className="ml-2 text-xs text-blue-600" title="Values differ">
                              ●
                            </span>
                          )}
                        </td>
                        {vehicles.map((vehicle) => {
                          const value = row.getValue(vehicle);
                          const displayValue = row.format ? row.format(value) : String(value);

                          // Determine if this is the "best" value for highlighting
                          const allValues = vehicles.map((v) => row.getValue(v));
                          const isBest = (() => {
                            if (row.label === 'Price') {
                              return value === Math.min(...allValues.map(Number));
                            }
                            if (row.label === 'Horsepower') {
                              const numValues = allValues.map(Number).filter((n) => !isNaN(n));
                              return numValues.length > 0 && value === Math.max(...numValues);
                            }
                            if (row.label === 'Mileage') {
                              return value === Math.min(...allValues.map(Number));
                            }
                            return false;
                          })();

                          return (
                            <td
                              key={vehicle.id}
                              className={`
                                px-6 py-4 text-sm text-center
                                ${hasDifference ? 'font-semibold' : ''}
                                ${isBest ? 'text-green-600 bg-green-50' : 'text-gray-900'}
                              `}
                            >
                              {typeof value === 'boolean' ? (
                                value ? (
                                  <FiCheck className="inline text-green-500" size={20} />
                                ) : (
                                  <FiX className="inline text-red-500" size={20} />
                                )
                              ) : (
                                displayValue
                              )}
                            </td>
                          );
                        })}
                      </tr>
                    );
                  })}
                </React.Fragment>
              );
            })}

            {/* Features Section */}
            <tr className="bg-gray-100">
              <td
                colSpan={vehicles.length + 1}
                className="px-6 py-3 text-sm font-bold text-gray-900"
              >
                Features
              </td>
            </tr>
            <tr className="border-b border-gray-100">
              <td className="sticky left-0 bg-white z-10 px-6 py-4 text-sm font-medium text-gray-700 align-top">
                Available Features
              </td>
              {vehicles.map((vehicle) => (
                <td key={vehicle.id} className="px-6 py-4 text-sm text-gray-900 align-top">
                  {vehicle.features && vehicle.features.length > 0 ? (
                    <ul className="space-y-1">
                      {vehicle.features.slice(0, 10).map((feature, idx) => (
                        <li key={idx} className="flex items-center gap-2">
                          <FiCheck className="text-green-500 flex-shrink-0" size={16} />
                          <span className="text-xs">{feature}</span>
                        </li>
                      ))}
                      {vehicle.features.length > 10 && (
                        <li className="text-xs text-gray-500 italic">
                          +{vehicle.features.length - 10} more features
                        </li>
                      )}
                    </ul>
                  ) : (
                    <span className="text-gray-400 italic">No features listed</span>
                  )}
                </td>
              ))}
            </tr>
          </tbody>
        </table>
      </div>

      {/* Legend */}
      <div className="bg-gray-50 border-t border-gray-200 px-6 py-4">
        <div className="flex flex-wrap items-center gap-6 text-xs text-gray-600">
          <div className="flex items-center gap-2">
            <span className="text-blue-600">●</span>
            <span>Different values across vehicles</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-4 h-4 bg-green-50 border border-green-200 rounded"></div>
            <span>Best value (lower price, higher horsepower, lower mileage)</span>
          </div>
        </div>
      </div>
    </div>
  );
}

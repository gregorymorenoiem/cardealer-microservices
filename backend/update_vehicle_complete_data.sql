-- =====================================================
-- Complete Vehicle Data - Add Missing Fields
-- =====================================================
-- This script updates Location, Color, Engine, and Features
-- for all existing vehicles in the database
-- =====================================================

-- Mercedes-Benz Clase C AMG
UPDATE vehicles 
SET 
  "City" = 'Miami',
  "State" = 'FL',
  "ExteriorColor" = 'Obsidian Black Metallic',
  "InteriorColor" = 'Black Leather',
  "EngineSize" = '2.0L Turbo I4',
  "Horsepower" = 255,
  "Torque" = 273,
  "Doors" = 4,
  "Seats" = 5,
  "FeaturesJson" = '["Leather Seats","Sunroof","Navigation System","Backup Camera","Bluetooth","Keyless Entry","Premium Sound System","Heated Seats","Adaptive Cruise Control","Lane Departure Warning","Blind Spot Monitor","Apple CarPlay","Android Auto","LED Headlights","Parking Sensors"]'::jsonb
WHERE "Make" = 'Mercedes-Benz' AND "Model" = 'Clase C AMG';

-- BMW Serie 7 Executive
UPDATE vehicles 
SET 
  "City" = 'Los Angeles',
  "State" = 'CA',
  "ExteriorColor" = 'Mineral White Metallic',
  "InteriorColor" = 'Cognac Leather',
  "EngineSize" = '3.0L Twin-Turbo I6',
  "Horsepower" = 335,
  "Torque" = 330,
  "Doors" = 4,
  "Seats" = 5,
  "FeaturesJson" = '["Executive Lounge Seating","Massage Seats","Rear Entertainment","Premium Sound System","Heads-Up Display","Night Vision","360° Camera","Wireless Charging","Gesture Control","Ambient Lighting","Soft-Close Doors","Power Rear Sunshade","Ventilated Seats","Adaptive LED Lights","Active Driving Assistant"]'::jsonb
WHERE "Make" = 'BMW' AND "Model" = 'Serie 7';

-- Porsche 911 Carrera S
UPDATE vehicles 
SET 
  "City" = 'New York',
  "State" = 'NY',
  "ExteriorColor" = 'GT Silver Metallic',
  "InteriorColor" = 'Black/Alcantara',
  "EngineSize" = '3.0L Twin-Turbo Flat-6',
  "Horsepower" = 443,
  "Torque" = 390,
  "Doors" = 2,
  "Seats" = 4,
  "FeaturesJson" = '["Sport Chrono Package","PASM Suspension","Sport Exhaust","Carbon Fiber Interior","Alcantara Steering Wheel","Sport Seats Plus","Porsche Communication Management","Bose Surround Sound","LED Matrix Headlights","Premium Leather","18-Way Power Seats","Track Precision App","Launch Control","Active Aero"]'::jsonb
WHERE "Make" = 'Porsche' AND "Model" = '911 Carrera S';

-- Audi RS7 Sportback
UPDATE vehicles 
SET 
  "City" = 'Dallas',
  "State" = 'TX',
  "ExteriorColor" = 'Nardo Gray',
  "InteriorColor" = 'Black Valcona Leather',
  "EngineSize" = '4.0L Twin-Turbo V8',
  "Horsepower" = 591,
  "Torque" = 590,
  "Doors" = 4,
  "Seats" = 5,
  "FeaturesJson" = '["RS Sport Suspension","Carbon Ceramic Brakes","Bang & Olufsen Sound","Virtual Cockpit Plus","Matrix LED Headlights","Sport Differential","RS Sport Exhaust","Massage Seats","Head-Up Display","Ambient Lighting Plus","360° Camera","Night Vision","Adaptive Cruise","Lane Assist","Carbon Fiber Trim"]'::jsonb
WHERE "Make" = 'Audi' AND "Model" = 'RS7 Sportback';

-- Tesla Model S Plaid
UPDATE vehicles 
SET 
  "City" = 'San Francisco',
  "State" = 'CA',
  "ExteriorColor" = 'Deep Blue Metallic',
  "InteriorColor" = 'Black and White',
  "EngineSize" = 'Tri-Motor Electric',
  "Horsepower" = 1020,
  "Torque" = 1050,
  "Doors" = 4,
  "Seats" = 5,
  "FeaturesJson" = '["Autopilot","Full Self-Driving Capability","Premium Interior","Glass Roof","22-inch Wheels","Premium Audio","Heated Seats Front & Rear","Wireless Charging","HEPA Air Filter","Bioweapon Defense Mode","Ludicrous Mode","Track Mode","Over-the-Air Updates","Mobile Connectivity","Gaming Computer"]'::jsonb
WHERE "Make" = 'Tesla' AND "Model" = 'Model S Plaid';

-- Range Rover Sport HSE
UPDATE vehicles 
SET 
  "City" = 'Phoenix',
  "State" = 'AZ',
  "ExteriorColor" = 'Santorini Black',
  "InteriorColor" = 'Ebony/Ivory Leather',
  "EngineSize" = '3.0L Supercharged V6',
  "Horsepower" = 355,
  "Torque" = 365,
  "Doors" = 4,
  "Seats" = 7,
  "FeaturesJson" = '["Terrain Response 2","Air Suspension","Meridian Sound System","Panoramic Roof","Windsor Leather","Heated & Cooled Seats","Configurable Dynamics","Wade Sensing","Blind Spot Assist","Surround Camera","Interactive Driver Display","Gesture Tailgate","Power Deploy Steps","Adaptive Cruise","Lane Keep Assist"]'::jsonb
WHERE "Make" = 'Range Rover' AND "Model" = 'Sport HSE';

-- Verify updates
SELECT 
  "Make",
  "Model",
  "City",
  "State",
  "ExteriorColor",
  "EngineSize",
  "Horsepower",
  jsonb_array_length("FeaturesJson") as features_count
FROM vehicles
ORDER BY "Make", "Model";

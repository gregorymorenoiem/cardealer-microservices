/**
 * Script para migrar vehículos de mock data a base de datos con imágenes en S3
 * 
 * Ejecutar con:
 * npx ts-node scripts/seed-vehicles-with-s3.ts
 * 
 * Requiere:
 * - AWS credentials configuradas (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY)
 * - PostgreSQL corriendo con VehiclesSaleService DB
 * - npm install @aws-sdk/client-s3 pg uuid @types/node @types/pg @types/uuid
 */

/* eslint-disable @typescript-eslint/no-require-imports */
// @ts-nocheck - Script ejecutado con ts-node, no parte del build del proyecto
const fs = require('fs');
const path = require('path');
const https = require('https');
const { S3Client, PutObjectCommand } = require('@aws-sdk/client-s3');
const { Client } = require('pg');
const { v4: uuidv4 } = require('uuid');

// ============================================
// CONFIGURACIÓN
// ============================================
const CONFIG = {
  // AWS S3
  s3: {
    region: process.env.AWS_REGION || 'us-east-1',
    bucket: process.env.S3_BUCKET || 'cardealer-vehicles',
    accessKeyId: process.env.AWS_ACCESS_KEY_ID || '',
    secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY || '',
  },
  // PostgreSQL
  db: {
    host: process.env.DB_HOST || 'localhost',
    port: parseInt(process.env.DB_PORT || '25432'),
    database: process.env.DB_NAME || 'vehiclessaleservice',
    user: process.env.DB_USER || 'postgres',
    password: process.env.DB_PASSWORD || 'password',
  },
  // Directorios
  downloadDir: path.join(__dirname, '../temp-unsplash-downloads'),
  // Default dealer ID para seed
  defaultDealerId: '00000000-0000-0000-0000-000000000001',
  defaultSellerId: '00000000-0000-0000-0000-000000000002',
};

// ============================================
// DATOS DE VEHÍCULOS (extraídos de mockVehicles.ts)
// ============================================
interface VehicleSeedData {
  id: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  location: string;
  transmission: 'Automatic' | 'Manual' | 'CVT';
  fuelType: 'Gasoline' | 'Diesel' | 'Electric' | 'Hybrid';
  bodyType: 'Sedan' | 'SUV' | 'Truck' | 'Coupe' | 'Hatchback' | 'Van' | 'Convertible';
  drivetrain: 'FWD' | 'RWD' | 'AWD' | '4WD';
  engine: string;
  horsepower: number;
  color: string;
  interiorColor: string;
  condition: 'New' | 'Used' | 'Certified Pre-Owned';
  description: string;
  features: string[];
  images: string[]; // URLs de Unsplash
  isFeatured: boolean;
}

const VEHICLES_TO_SEED: VehicleSeedData[] = [
  {
    id: '1',
    make: 'Tesla',
    model: 'Model 3',
    year: 2023,
    price: 42990,
    mileage: 5200,
    location: 'Los Angeles, CA',
    transmission: 'Automatic',
    fuelType: 'Electric',
    bodyType: 'Sedan',
    drivetrain: 'AWD',
    engine: 'Dual Motor Electric',
    horsepower: 480,
    color: 'Pearl White',
    interiorColor: 'Black',
    condition: 'New',
    description: 'Tesla Model 3 Long Range con Autopilot, audio premium y todas las actualizaciones recientes.',
    features: ['Autopilot', 'Premium Audio', 'Glass Roof', 'Navigation', 'Heated Seats'],
    images: [
      'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&h=600&fit=crop',
      'https://images.unsplash.com/photo-1561580125-028ee3bd62eb?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '2',
    make: 'BMW',
    model: '3 Series',
    year: 2022,
    price: 38500,
    mileage: 15000,
    location: 'Miami, FL',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'RWD',
    engine: '2.0L Turbo I4',
    horsepower: 255,
    color: 'Alpine White',
    interiorColor: 'Cognac',
    condition: 'Used',
    description: 'BMW 330i con M Sport Package, asientos de cuero y tecnología avanzada.',
    features: ['M Sport Package', 'Leather Seats', 'Navigation', 'Sunroof', 'Apple CarPlay'],
    images: [
      'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '3',
    make: 'Toyota',
    model: 'Camry',
    year: 2021,
    price: 24900,
    mileage: 28000,
    location: 'Houston, TX',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'FWD',
    engine: '2.5L I4',
    horsepower: 203,
    color: 'Midnight Black',
    interiorColor: 'Gray',
    condition: 'Certified Pre-Owned',
    description: 'Toyota Camry SE certificado con garantía extendida y historial de servicio completo.',
    features: ['Toyota Safety Sense', 'Lane Departure', 'Adaptive Cruise', 'Apple CarPlay'],
    images: [
      'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '4',
    make: 'Ford',
    model: 'Mustang',
    year: 2023,
    price: 55990,
    mileage: 3500,
    location: 'Dallas, TX',
    transmission: 'Manual',
    fuelType: 'Gasoline',
    bodyType: 'Coupe',
    drivetrain: 'RWD',
    engine: '5.0L V8',
    horsepower: 450,
    color: 'Race Red',
    interiorColor: 'Black',
    condition: 'New',
    description: 'Ford Mustang GT con Performance Package, escape activo y frenos Brembo.',
    features: ['Performance Package', 'Active Exhaust', 'Brembo Brakes', 'Track Apps'],
    images: [
      'https://images.unsplash.com/photo-1544636331-e26879cd4d9b?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '5',
    make: 'Honda',
    model: 'Accord',
    year: 2023,
    price: 32500,
    mileage: 8000,
    location: 'Chicago, IL',
    transmission: 'CVT',
    fuelType: 'Hybrid',
    bodyType: 'Sedan',
    drivetrain: 'FWD',
    engine: '2.0L Hybrid',
    horsepower: 212,
    color: 'Lunar Silver',
    interiorColor: 'Black',
    condition: 'Used',
    description: 'Honda Accord Hybrid Touring con navegación y asientos de cuero.',
    features: ['Hybrid System', 'Leather Seats', 'Honda Sensing', 'Sunroof', 'Navigation'],
    images: [
      'https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '6',
    make: 'Jeep',
    model: 'Grand Cherokee',
    year: 2022,
    price: 48700,
    mileage: 22000,
    location: 'Denver, CO',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'SUV',
    drivetrain: '4WD',
    engine: '3.6L V6',
    horsepower: 293,
    color: 'Diamond Black',
    interiorColor: 'Black',
    condition: 'Used',
    description: 'Jeep Grand Cherokee Limited con sistema 4x4 Quadra-Trac II.',
    features: ['Quadra-Trac II', 'Leather Seats', 'Panoramic Sunroof', 'Uconnect 5'],
    images: [
      'https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '7',
    make: 'Audi',
    model: 'A4',
    year: 2023,
    price: 45900,
    mileage: 5000,
    location: 'San Diego, CA',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'AWD',
    engine: '2.0L Turbo I4',
    horsepower: 261,
    color: 'Glacier White',
    interiorColor: 'Brown',
    condition: 'New',
    description: 'Audi A4 Premium Plus con Quattro AWD y Virtual Cockpit.',
    features: ['Quattro AWD', 'Virtual Cockpit', 'S Line Package', 'B&O Audio'],
    images: [
      'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '8',
    make: 'Mercedes-Benz',
    model: 'GLC',
    year: 2023,
    price: 52900,
    mileage: 12000,
    location: 'Seattle, WA',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'SUV',
    drivetrain: 'AWD',
    engine: '2.0L Turbo I4',
    horsepower: 258,
    color: 'Obsidian Black',
    interiorColor: 'Macchiato Beige',
    condition: 'Used',
    description: 'Mercedes-Benz GLC 300 4MATIC con AMG Line y Burmester audio.',
    features: ['AMG Line', '4MATIC AWD', 'Burmester Audio', 'MBUX Infotainment'],
    images: [
      'https://images.unsplash.com/photo-1563720223185-11003d516935?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '9',
    make: 'Lexus',
    model: 'ES 350',
    year: 2022,
    price: 44200,
    mileage: 18000,
    location: 'Phoenix, AZ',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'FWD',
    engine: '3.5L V6',
    horsepower: 302,
    color: 'Atomic Silver',
    interiorColor: 'Black',
    condition: 'Certified Pre-Owned',
    description: 'Lexus ES 350 F Sport con suspensión deportiva adaptativa.',
    features: ['F Sport Package', 'Adaptive Suspension', 'Mark Levinson Audio', 'Sunroof'],
    images: [
      'https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '10',
    make: 'Porsche',
    model: 'Cayenne',
    year: 2022,
    price: 89500,
    mileage: 15000,
    location: 'Las Vegas, NV',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'SUV',
    drivetrain: 'AWD',
    engine: '3.0L Turbo V6',
    horsepower: 335,
    color: 'Carrara White',
    interiorColor: 'Black',
    condition: 'Used',
    description: 'Porsche Cayenne con paquete Sport Chrono y suspensión neumática.',
    features: ['Sport Chrono', 'Air Suspension', 'Bose Audio', 'Panoramic Roof'],
    images: [
      'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '11',
    make: 'Chevrolet',
    model: 'Tahoe',
    year: 2023,
    price: 58900,
    mileage: 10000,
    location: 'Atlanta, GA',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'SUV',
    drivetrain: '4WD',
    engine: '5.3L V8',
    horsepower: 355,
    color: 'Black',
    interiorColor: 'Jet Black',
    condition: 'Used',
    description: 'Chevrolet Tahoe Z71 con asientos para 7 pasajeros y remolque.',
    features: ['Z71 Package', 'Magnetic Ride Control', 'Bose Audio', 'Towing Package'],
    images: [
      'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '12',
    make: 'Land Rover',
    model: 'Range Rover Sport',
    year: 2023,
    price: 95000,
    mileage: 8000,
    location: 'San Francisco, CA',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'SUV',
    drivetrain: 'AWD',
    engine: '3.0L Turbo I6',
    horsepower: 395,
    color: 'Santorini Black',
    interiorColor: 'Ebony',
    condition: 'New',
    description: 'Range Rover Sport HSE Dynamic con Terrain Response 2.',
    features: ['Terrain Response 2', 'Air Suspension', 'Meridian Audio', 'Panoramic Roof'],
    images: [
      'https://images.unsplash.com/photo-1551830820-330a71b99659?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '13',
    make: 'Volvo',
    model: 'XC90',
    year: 2022,
    price: 62500,
    mileage: 20000,
    location: 'Orlando, FL',
    transmission: 'Automatic',
    fuelType: 'Hybrid',
    bodyType: 'SUV',
    drivetrain: 'AWD',
    engine: '2.0L Turbo Hybrid',
    horsepower: 455,
    color: 'Crystal White',
    interiorColor: 'Blonde',
    condition: 'Used',
    description: 'Volvo XC90 Recharge con propulsión híbrida enchufable.',
    features: ['Plug-in Hybrid', 'Pilot Assist', 'Bowers & Wilkins Audio', 'Air Cleaner'],
    images: [
      'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '14',
    make: 'Ford',
    model: 'F-150',
    year: 2023,
    price: 55900,
    mileage: 5000,
    location: 'Houston, TX',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Truck',
    drivetrain: '4WD',
    engine: '3.5L EcoBoost V6',
    horsepower: 400,
    color: 'Iconic Silver',
    interiorColor: 'Black',
    condition: 'New',
    description: 'Ford F-150 Lariat con motor EcoBoost y capacidad de remolque de 14,000 lbs.',
    features: ['EcoBoost', 'Pro Power Onboard', 'Max Tow Package', 'B&O Audio'],
    images: [
      'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '15',
    make: 'RAM',
    model: '1500',
    year: 2022,
    price: 48500,
    mileage: 25000,
    location: 'Denver, CO',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Truck',
    drivetrain: '4WD',
    engine: '5.7L HEMI V8',
    horsepower: 395,
    color: 'Patriot Blue',
    interiorColor: 'Black',
    condition: 'Used',
    description: 'RAM 1500 Laramie con motor HEMI y suspensión neumática.',
    features: ['HEMI V8', 'Air Suspension', 'Harman Kardon Audio', 'Uconnect 5'],
    images: [
      'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '16',
    make: 'Chevrolet',
    model: 'Silverado',
    year: 2023,
    price: 52000,
    mileage: 12000,
    location: 'Dallas, TX',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Truck',
    drivetrain: '4WD',
    engine: '6.2L V8',
    horsepower: 420,
    color: 'Summit White',
    interiorColor: 'Jet Black',
    condition: 'Used',
    description: 'Chevrolet Silverado High Country con el potente motor 6.2L V8.',
    features: ['6.2L V8', 'Super Cruise', 'Multi-Flex Tailgate', 'Bose Audio'],
    images: [
      'https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '17',
    make: 'GMC',
    model: 'Sierra',
    year: 2022,
    price: 54900,
    mileage: 18000,
    location: 'Phoenix, AZ',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Truck',
    drivetrain: '4WD',
    engine: '6.2L V8',
    horsepower: 420,
    color: 'Onyx Black',
    interiorColor: 'Jet Black',
    condition: 'Used',
    description: 'GMC Sierra Denali Ultimate con tecnología CarbonPro bed.',
    features: ['Denali Ultimate', 'CarbonPro Bed', 'Super Cruise', 'Head-Up Display'],
    images: [
      'https://images.unsplash.com/photo-1625231334168-5bf4c59cbb21?w=800&h=600&fit=crop',
    ],
    isFeatured: false,
  },
  {
    id: '18',
    make: 'Toyota',
    model: 'Tacoma',
    year: 2023,
    price: 42500,
    mileage: 8000,
    location: 'Seattle, WA',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Truck',
    drivetrain: '4WD',
    engine: '3.5L V6',
    horsepower: 278,
    color: 'Army Green',
    interiorColor: 'Black',
    condition: 'Used',
    description: 'Toyota Tacoma TRD Pro con suspensión FOX y capacidad off-road.',
    features: ['TRD Pro', 'FOX Suspension', 'Crawl Control', 'Multi-Terrain Select'],
    images: [
      'https://images.unsplash.com/photo-1612544448445-b8232cff3b6c?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '19',
    make: 'Rivian',
    model: 'R1T',
    year: 2023,
    price: 73000,
    mileage: 5000,
    location: 'Portland, OR',
    transmission: 'Automatic',
    fuelType: 'Electric',
    bodyType: 'Truck',
    drivetrain: 'AWD',
    engine: 'Quad Motor Electric',
    horsepower: 835,
    color: 'Rivian Blue',
    interiorColor: 'Ocean Coast',
    condition: 'New',
    description: 'Rivian R1T Adventure Package con rango de 314 millas.',
    features: ['Quad Motor', 'Air Suspension', 'Camp Kitchen', 'Adventure Gear Tunnel'],
    images: [
      'https://images.unsplash.com/photo-1583121274602-3e2820c69888?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
  {
    id: '20',
    make: 'Porsche',
    model: '911 Carrera',
    year: 2022,
    price: 115000,
    mileage: 8000,
    location: 'Beverly Hills, CA',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Coupe',
    drivetrain: 'RWD',
    engine: '3.0L Twin-Turbo Flat-6',
    horsepower: 379,
    color: 'Guards Red',
    interiorColor: 'Black',
    condition: 'Used',
    description: 'Porsche 911 Carrera con Sport Chrono Package y PASM.',
    features: ['Sport Chrono', 'PASM Suspension', 'Bose Audio', 'Sport Exhaust'],
    images: [
      'https://images.unsplash.com/photo-1596468138838-8c50b8d9b2c5?w=800&h=600&fit=crop',
    ],
    isFeatured: true,
  },
];

// ============================================
// FUNCIONES DE UTILIDAD
// ============================================

/**
 * Descarga una imagen de una URL y la guarda localmente
 */
async function downloadImage(url: string, filename: string): Promise<string> {
  const filepath = path.join(CONFIG.downloadDir, filename);
  
  // Crear directorio si no existe
  if (!fs.existsSync(CONFIG.downloadDir)) {
    fs.mkdirSync(CONFIG.downloadDir, { recursive: true });
  }
  
  // Si ya existe, no volver a descargar
  if (fs.existsSync(filepath)) {
    console.log(`  ⏭️  Imagen ya existe: ${filename}`);
    return filepath;
  }
  
  return new Promise((resolve, reject) => {
    const file = fs.createWriteStream(filepath);
    
    https.get(url, (response) => {
      // Manejar redirects
      if (response.statusCode === 301 || response.statusCode === 302) {
        const redirectUrl = response.headers.location;
        if (redirectUrl) {
          https.get(redirectUrl, (redirectResponse) => {
            redirectResponse.pipe(file);
            file.on('finish', () => {
              file.close();
              console.log(`  ✅ Descargada: ${filename}`);
              resolve(filepath);
            });
          }).on('error', reject);
        } else {
          reject(new Error('Redirect sin location header'));
        }
        return;
      }
      
      response.pipe(file);
      file.on('finish', () => {
        file.close();
        console.log(`  ✅ Descargada: ${filename}`);
        resolve(filepath);
      });
    }).on('error', (err) => {
      fs.unlink(filepath, () => {}); // Eliminar archivo parcial
      reject(err);
    });
  });
}

/**
 * Sube una imagen a S3
 */
async function uploadToS3(s3Client: S3Client, filepath: string, key: string): Promise<string> {
  const fileContent = fs.readFileSync(filepath);
  const contentType = 'image/jpeg';
  
  const command = new PutObjectCommand({
    Bucket: CONFIG.s3.bucket,
    Key: key,
    Body: fileContent,
    ContentType: contentType,
    ACL: 'public-read', // Hacer público para acceso directo
  });
  
  await s3Client.send(command);
  
  // Retornar URL pública
  const s3Url = `https://${CONFIG.s3.bucket}.s3.${CONFIG.s3.region}.amazonaws.com/${key}`;
  console.log(`  ☁️  Subida a S3: ${key}`);
  return s3Url;
}

/**
 * Parsea la ubicación en ciudad y estado
 */
function parseLocation(location: string): { city: string; state: string } {
  const parts = location.split(', ');
  return {
    city: parts[0] || '',
    state: parts[1] || '',
  };
}

/**
 * Mapea condition string a valor de enum
 */
function mapCondition(condition: string): number {
  switch (condition) {
    case 'New': return 0;
    case 'Used': return 1;
    case 'Certified Pre-Owned': return 2;
    default: return 1;
  }
}

/**
 * Mapea bodyType string a valor de enum
 */
function mapBodyStyle(bodyType: string): number {
  const mapping: Record<string, number> = {
    'Sedan': 0,
    'SUV': 1,
    'Truck': 2,
    'Coupe': 3,
    'Hatchback': 4,
    'Van': 5,
    'Convertible': 6,
    'Wagon': 7,
  };
  return mapping[bodyType] ?? 0;
}

/**
 * Mapea transmission string a valor de enum
 */
function mapTransmission(transmission: string): number {
  switch (transmission) {
    case 'Manual': return 0;
    case 'Automatic': return 1;
    case 'CVT': return 2;
    default: return 1;
  }
}

/**
 * Mapea fuelType string a valor de enum
 */
function mapFuelType(fuelType: string): number {
  const mapping: Record<string, number> = {
    'Gasoline': 0,
    'Diesel': 1,
    'Electric': 2,
    'Hybrid': 3,
    'Plug-in Hybrid': 4,
  };
  return mapping[fuelType] ?? 0;
}

/**
 * Mapea drivetrain string a valor de enum
 */
function mapDriveType(drivetrain: string): number {
  const mapping: Record<string, number> = {
    'FWD': 0,
    'RWD': 1,
    'AWD': 2,
    '4WD': 3,
  };
  return mapping[drivetrain] ?? 0;
}

// ============================================
// FUNCIÓN PRINCIPAL
// ============================================

async function main() {
  console.log('🚗 Iniciando migración de vehículos a base de datos con S3...\n');
  
  // Verificar credenciales de S3
  if (!CONFIG.s3.accessKeyId || !CONFIG.s3.secretAccessKey) {
    console.error('❌ Error: Faltan credenciales de AWS S3');
    console.log('   Configura las variables de entorno:');
    console.log('   - AWS_ACCESS_KEY_ID');
    console.log('   - AWS_SECRET_ACCESS_KEY');
    console.log('   - S3_BUCKET (opcional, default: cardealer-vehicles)');
    process.exit(1);
  }
  
  // Inicializar clientes
  const s3Client = new S3Client({
    region: CONFIG.s3.region,
    credentials: {
      accessKeyId: CONFIG.s3.accessKeyId,
      secretAccessKey: CONFIG.s3.secretAccessKey,
    },
  });
  
  const dbClient = new Client({
    host: CONFIG.db.host,
    port: CONFIG.db.port,
    database: CONFIG.db.database,
    user: CONFIG.db.user,
    password: CONFIG.db.password,
  });
  
  try {
    // Conectar a la base de datos
    console.log('📦 Conectando a PostgreSQL...');
    await dbClient.connect();
    console.log('   ✅ Conectado\n');
    
    // Procesar cada vehículo
    for (const vehicle of VEHICLES_TO_SEED) {
      console.log(`\n🚙 Procesando: ${vehicle.year} ${vehicle.make} ${vehicle.model}`);
      
      const vehicleId = uuidv4();
      const s3ImageUrls: string[] = [];
      
      // 1. Descargar y subir imágenes
      console.log('   📸 Procesando imágenes...');
      for (let i = 0; i < vehicle.images.length; i++) {
        const imageUrl = vehicle.images[i];
        const filename = `${vehicle.make.toLowerCase()}-${vehicle.model.toLowerCase().replace(/\s+/g, '-')}-${i + 1}.jpg`;
        const s3Key = `vehicles/${vehicleId}/${filename}`;
        
        try {
          // Descargar
          const localPath = await downloadImage(imageUrl, filename);
          
          // Subir a S3
          const s3Url = await uploadToS3(s3Client, localPath, s3Key);
          s3ImageUrls.push(s3Url);
        } catch (err) {
          console.error(`   ❌ Error con imagen: ${err}`);
        }
      }
      
      // 2. Insertar vehículo en la base de datos
      const { city, state } = parseLocation(vehicle.location);
      
      const insertVehicleQuery = `
        INSERT INTO "Vehicles" (
          "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
          "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
          "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
          "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
          "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
          "CreatedAt", "UpdatedAt"
        ) VALUES (
          $1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16, $17, $18, $19, $20, $21, $22, $23, $24, $25, $26, $27, $28, $29, $30, $31
        )
        ON CONFLICT ("Id") DO UPDATE SET
          "Title" = EXCLUDED."Title",
          "Price" = EXCLUDED."Price",
          "UpdatedAt" = EXCLUDED."UpdatedAt"
      `;
      
      const vehicleValues = [
        vehicleId,                                      // Id
        CONFIG.defaultDealerId,                         // DealerId
        `${vehicle.year} ${vehicle.make} ${vehicle.model}`, // Title
        vehicle.description,                            // Description
        vehicle.price,                                  // Price
        'USD',                                          // Currency
        1,                                              // Status (Active = 1)
        CONFIG.defaultSellerId,                         // SellerId
        'CarDealer Demo',                               // SellerName
        vehicle.make,                                   // Make
        vehicle.model,                                  // Model
        vehicle.year,                                   // Year
        mapCondition(vehicle.condition),                // Condition
        mapBodyStyle(vehicle.bodyType),                 // BodyStyle
        mapTransmission(vehicle.transmission),          // Transmission
        mapFuelType(vehicle.fuelType),                  // FuelType
        mapDriveType(vehicle.drivetrain),               // DriveType
        0,                                              // VehicleType (Car = 0)
        vehicle.mileage,                                // Mileage
        0,                                              // MileageUnit (Miles = 0)
        vehicle.color,                                  // ExteriorColor
        vehicle.interiorColor,                          // InteriorColor
        vehicle.engine,                                 // EngineSize
        vehicle.horsepower,                             // Horsepower
        city,                                           // City
        state,                                          // State
        'USA',                                          // Country
        JSON.stringify(vehicle.features),               // FeaturesJson
        vehicle.isFeatured,                             // IsFeatured
        new Date(),                                     // CreatedAt
        new Date(),                                     // UpdatedAt
      ];
      
      await dbClient.query(insertVehicleQuery, vehicleValues);
      console.log(`   ✅ Vehículo insertado: ${vehicleId}`);
      
      // 3. Insertar imágenes
      for (let i = 0; i < s3ImageUrls.length; i++) {
        const imageId = uuidv4();
        const insertImageQuery = `
          INSERT INTO "VehicleImages" (
            "Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt"
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
          ON CONFLICT ("Id") DO NOTHING
        `;
        
        await dbClient.query(insertImageQuery, [
          imageId,
          CONFIG.defaultDealerId,
          vehicleId,
          s3ImageUrls[i],
          0, // ImageType.Exterior
          i,
          i === 0, // Primera imagen es primaria
          new Date(),
        ]);
      }
      console.log(`   ✅ ${s3ImageUrls.length} imágenes vinculadas`);
    }
    
    console.log('\n\n✅ ¡Migración completada exitosamente!');
    console.log(`   📊 ${VEHICLES_TO_SEED.length} vehículos insertados`);
    console.log(`   🖼️  Imágenes almacenadas en S3: ${CONFIG.s3.bucket}`);
    
  } catch (error) {
    console.error('❌ Error durante la migración:', error);
    process.exit(1);
  } finally {
    await dbClient.end();
  }
}

// Ejecutar
main();

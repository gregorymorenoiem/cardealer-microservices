/**
 * Script para migrar imágenes de Unsplash a AWS S3
 * 
 * Estructura S3: okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/{category}/{type}/{photoId}.jpg
 * 
 * Categorías:
 * - vehicles/sale/     -> Vehículos en venta
 * - vehicles/rent/     -> Renta de vehículos
 * - properties/sale/   -> Propiedades en venta
 * - lodging/           -> Hospedaje
 * 
 * Usage: npx tsx migrate-images-to-s3.ts
 */

import { S3Client, PutObjectCommand, HeadObjectCommand } from '@aws-sdk/client-s3';
import { readFileSync, writeFileSync, existsSync, mkdirSync } from 'fs';
import { join } from 'path';
import https from 'https';

// ========== CONFIGURACIÓN ==========
const S3_BUCKET = 'okla-images-2026';
const S3_REGION = 'us-east-2';
const S3_BASE_URL = `https://${S3_BUCKET}.s3.${S3_REGION}.amazonaws.com`;

// Directorio temporal para descargas
const TEMP_DIR = join(__dirname, '../temp-unsplash-downloads');

// ========== IMÁGENES A MIGRAR ==========
interface ImageToMigrate {
  photoId: string;
  unsplashUrl: string;
  category: 'vehicles' | 'properties' | 'lodging';
  type: 'sale' | 'rent' | '';
  description: string;
}

// Extraer photoId de una URL de Unsplash
function extractPhotoId(url: string): string {
  // URL format: https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&h=600&fit=crop
  const match = url.match(/photo-([a-zA-Z0-9-]+)/);
  return match ? `photo-${match[1]}` : '';
}

// Todas las imágenes del HomePage.tsx
const imagesToMigrate: ImageToMigrate[] = [
  // ===== VEHÍCULOS EN VENTA =====
  { 
    photoId: 'photo-1618843479313-40f8afb4b4d8',
    unsplashUrl: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'sale', description: 'Mercedes-Benz Clase C AMG 2024'
  },
  { 
    photoId: 'photo-1555215695-3004980ad54e',
    unsplashUrl: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'sale', description: 'BMW Serie 7 Executive Package'
  },
  { 
    photoId: 'photo-1503376780353-7e6692767b70',
    unsplashUrl: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'sale', description: 'Porsche 911 Carrera S'
  },
  { 
    photoId: 'photo-1606664515524-ed2f786a0bd6',
    unsplashUrl: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'sale', description: 'Audi RS7 Sportback 2024'
  },
  { 
    photoId: 'photo-1617788138017-80ad40651399',
    unsplashUrl: 'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'sale', description: 'Tesla Model S Plaid'
  },
  { 
    photoId: 'photo-1606016159991-dfe4f2746ad5',
    unsplashUrl: 'https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'sale', description: 'Range Rover Sport HSE'
  },

  // ===== RENTA DE VEHÍCULOS =====
  { 
    photoId: 'photo-1549317661-bd32c8ce0db2',
    unsplashUrl: 'https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'rent', description: 'BMW X5 - Renta por Día'
  },
  { 
    photoId: 'photo-1563720223185-11003d516935',
    unsplashUrl: 'https://images.unsplash.com/photo-1563720223185-11003d516935?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'rent', description: 'Mercedes GLE Coupe - Renta Semanal'
  },
  { 
    photoId: 'photo-1619767886558-efdc259cde1a',
    unsplashUrl: 'https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'rent', description: 'Porsche Cayenne - Renta Premium'
  },
  { 
    photoId: 'photo-1533473359331-0135ef1b58bf',
    unsplashUrl: 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'rent', description: 'Cadillac Escalade - Eventos'
  },
  { 
    photoId: 'photo-1560958089-b8a1929cea89',
    unsplashUrl: 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'rent', description: 'Tesla Model X - Renta Ecológica'
  },
  { 
    photoId: 'photo-1551830820-330a71b99659',
    unsplashUrl: 'https://images.unsplash.com/photo-1551830820-330a71b99659?w=1200&h=800&fit=crop',
    category: 'vehicles', type: 'rent', description: 'Range Rover Velar - Lujo'
  },

  // ===== PROPIEDADES EN VENTA =====
  { 
    photoId: 'photo-1600607687939-ce8a6c25118c',
    unsplashUrl: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=1200&h=800&fit=crop',
    category: 'properties', type: 'sale', description: 'Penthouse de Lujo con Vista al Mar'
  },
  { 
    photoId: 'photo-1600596542815-ffad4c1539a9',
    unsplashUrl: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=1200&h=800&fit=crop',
    category: 'properties', type: 'sale', description: 'Villa Contemporánea con Piscina'
  },
  { 
    photoId: 'photo-1502672260266-1c1ef2d93688',
    unsplashUrl: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=1200&h=800&fit=crop',
    category: 'properties', type: 'sale', description: 'Apartamento Moderno Centro'
  },
  { 
    photoId: 'photo-1600585154340-be6161a56a0c',
    unsplashUrl: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=1200&h=800&fit=crop',
    category: 'properties', type: 'sale', description: 'Casa Colonial con Jardín Amplio'
  },
  { 
    photoId: 'photo-1600607688969-a5bfcd646154',
    unsplashUrl: 'https://images.unsplash.com/photo-1600607688969-a5bfcd646154?w=1200&h=800&fit=crop',
    category: 'properties', type: 'sale', description: 'Loft Industrial Renovado'
  },
  { 
    photoId: 'photo-1613490493576-7fde63acd811',
    unsplashUrl: 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=1200&h=800&fit=crop',
    category: 'properties', type: 'sale', description: 'Mansión con Vista Panorámica'
  },

  // ===== HOSPEDAJE =====
  { 
    photoId: 'photo-1582719478250-c89cae4dc85b',
    unsplashUrl: 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=1200&h=800&fit=crop',
    category: 'lodging', type: '', description: 'Suite Premium Frente al Mar'
  },
  { 
    photoId: 'photo-1560448204-e02f11c3d0e2',
    unsplashUrl: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=1200&h=800&fit=crop',
    category: 'lodging', type: '', description: 'Apartamento Ejecutivo Centro'
  },
  { 
    photoId: 'photo-1602002418082-a4443e081dd1',
    unsplashUrl: 'https://images.unsplash.com/photo-1602002418082-a4443e081dd1?w=1200&h=800&fit=crop',
    category: 'lodging', type: '', description: 'Villa Privada con Alberca'
  },
  { 
    photoId: 'photo-1587061949409-02df41d5e562',
    unsplashUrl: 'https://images.unsplash.com/photo-1587061949409-02df41d5e562?w=1200&h=800&fit=crop',
    category: 'lodging', type: '', description: 'Cabaña Rústica en la Montaña'
  },
  { 
    photoId: 'photo-1578683010236-d716f9a3f461',
    unsplashUrl: 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=1200&h=800&fit=crop',
    category: 'lodging', type: '', description: 'Penthouse con Terraza Privada'
  },
  { 
    photoId: 'photo-1499793983690-e29da59ef1c2',
    unsplashUrl: 'https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=1200&h=800&fit=crop',
    category: 'lodging', type: '', description: 'Casa de Playa con Jacuzzi'
  },

  // ===== CATEGORÍAS (verticales) =====
  { 
    photoId: 'photo-1492144534655-ae79c964c9d7',
    unsplashUrl: 'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=800&h=800&fit=crop',
    category: 'vehicles', type: 'sale', description: 'Categoría Vehículos'
  },
  { 
    photoId: 'photo-1449965408869-eaa3f722e40d',
    unsplashUrl: 'https://images.unsplash.com/photo-1449965408869-eaa3f722e40d?w=800&h=800&fit=crop',
    category: 'vehicles', type: 'rent', description: 'Categoría Renta de Vehículos'
  },
  { 
    photoId: 'photo-1566073771259-6a8506099945',
    unsplashUrl: 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800&h=800&fit=crop',
    category: 'lodging', type: '', description: 'Categoría Hospedaje'
  },
];

// ========== FUNCIONES DE DESCARGA ==========
function downloadImage(url: string, filepath: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const file = require('fs').createWriteStream(filepath);
    https.get(url, (response) => {
      if (response.statusCode === 301 || response.statusCode === 302) {
        // Handle redirect
        const redirectUrl = response.headers.location;
        if (redirectUrl) {
          downloadImage(redirectUrl, filepath).then(resolve).catch(reject);
          return;
        }
      }
      response.pipe(file);
      file.on('finish', () => {
        file.close();
        resolve();
      });
    }).on('error', (err) => {
      require('fs').unlink(filepath, () => {}); // Delete the file on error
      reject(err);
    });
  });
}

// ========== FUNCIONES DE S3 ==========
function getS3Path(image: ImageToMigrate): string {
  if (image.type) {
    return `frontend/assets/${image.category}/${image.type}/${image.photoId}.jpg`;
  }
  return `frontend/assets/${image.category}/${image.photoId}.jpg`;
}

async function checkImageExistsInS3(s3Client: S3Client, key: string): Promise<boolean> {
  try {
    await s3Client.send(new HeadObjectCommand({
      Bucket: S3_BUCKET,
      Key: key,
    }));
    return true;
  } catch {
    return false;
  }
}

async function uploadToS3(s3Client: S3Client, filepath: string, key: string): Promise<void> {
  const fileContent = readFileSync(filepath);
  
  await s3Client.send(new PutObjectCommand({
    Bucket: S3_BUCKET,
    Key: key,
    Body: fileContent,
    ContentType: 'image/jpeg',
    CacheControl: 'public, max-age=31536000', // 1 year cache
  }));
}

// ========== MAIN ==========
async function main() {
  console.log('🚀 Iniciando migración de imágenes a S3...\n');
  console.log(`📦 Bucket: ${S3_BUCKET}`);
  console.log(`🌎 Region: ${S3_REGION}`);
  console.log(`📁 Base URL: ${S3_BASE_URL}\n`);

  // Verificar credenciales AWS
  if (!process.env.AWS_ACCESS_KEY_ID || !process.env.AWS_SECRET_ACCESS_KEY) {
    console.log('⚠️  AWS credentials no encontradas en variables de entorno.');
    console.log('   Configurar: AWS_ACCESS_KEY_ID y AWS_SECRET_ACCESS_KEY\n');
    console.log('   Generando archivo de referencia de imágenes...\n');
    
    // Generar archivo de referencia sin subir
    generateReferenceFile();
    return;
  }

  // Crear cliente S3
  const s3Client = new S3Client({
    region: S3_REGION,
    credentials: {
      accessKeyId: process.env.AWS_ACCESS_KEY_ID,
      secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY,
    },
  });

  // Crear directorio temporal
  if (!existsSync(TEMP_DIR)) {
    mkdirSync(TEMP_DIR, { recursive: true });
  }

  let uploaded = 0;
  let skipped = 0;
  let errors = 0;

  for (const image of imagesToMigrate) {
    const s3Key = getS3Path(image);
    const localPath = join(TEMP_DIR, `${image.photoId}.jpg`);
    
    try {
      // Verificar si ya existe en S3
      const exists = await checkImageExistsInS3(s3Client, s3Key);
      if (exists) {
        console.log(`⏭️  Skipped (exists): ${s3Key}`);
        skipped++;
        continue;
      }

      // Descargar de Unsplash
      console.log(`📥 Downloading: ${image.description}...`);
      await downloadImage(image.unsplashUrl, localPath);

      // Subir a S3
      console.log(`📤 Uploading to: ${s3Key}`);
      await uploadToS3(s3Client, localPath, s3Key);
      
      console.log(`✅ Done: ${S3_BASE_URL}/${s3Key}\n`);
      uploaded++;

      // Pequeña pausa para no saturar Unsplash
      await new Promise(resolve => setTimeout(resolve, 500));

    } catch (error) {
      console.error(`❌ Error with ${image.photoId}:`, error);
      errors++;
    }
  }

  console.log('\n========== RESUMEN ==========');
  console.log(`✅ Subidas: ${uploaded}`);
  console.log(`⏭️  Skipped: ${skipped}`);
  console.log(`❌ Errores: ${errors}`);
  console.log(`📊 Total: ${imagesToMigrate.length}`);

  // Generar archivo de referencia
  generateReferenceFile();
}

function generateReferenceFile() {
  const reference = imagesToMigrate.map(img => ({
    photoId: img.photoId,
    s3Path: getS3Path(img),
    s3Url: `${S3_BASE_URL}/${getS3Path(img)}`,
    category: img.category,
    type: img.type,
    description: img.description,
  }));

  const outputPath = join(__dirname, 's3-image-reference.json');
  writeFileSync(outputPath, JSON.stringify(reference, null, 2));
  console.log(`\n📄 Referencia guardada en: ${outputPath}`);

  // También generar SQL para insertar en DB
  generateSeedSQL();
}

function generateSeedSQL() {
  const vehiclesSale = imagesToMigrate.filter(i => i.category === 'vehicles' && i.type === 'sale');
  
  let sql = `-- Seed vehicles con photoIds para S3
-- Ejecutar después de crear las tablas

-- Limpiar datos existentes
TRUNCATE vehicles CASCADE;

-- Insertar vehículos de prueba
`;

  vehiclesSale.forEach((img, index) => {
    const id = `'${crypto.randomUUID()}'`;
    const dealerId = "'00000000-0000-0000-0000-000000000001'";
    
    sql += `
INSERT INTO vehicles (id, dealer_id, title, make, model, year, price, mileage, condition, transmission, fuel_type, body_type, exterior_color, city, state, country, status, is_featured, created_at)
VALUES (
  ${id},
  ${dealerId},
  '${img.description}',
  '${img.description.split(' ')[0]}', -- make
  '${img.description.split(' ').slice(1, 3).join(' ')}', -- model
  2024,
  ${75000 + index * 10000},
  ${1000 + index * 5000},
  'New',
  'Automatic',
  'Gasoline',
  'Sedan',
  'Black',
  'Miami',
  'FL',
  'USA',
  'Active',
  ${index < 3},
  NOW()
);

INSERT INTO vehicle_images (id, vehicle_id, image_id, is_primary, sort_order, created_at)
VALUES (
  '${crypto.randomUUID()}',
  (SELECT id FROM vehicles WHERE title = '${img.description}' LIMIT 1),
  '${img.photoId}',
  true,
  0,
  NOW()
);
`;
  });

  const sqlPath = join(__dirname, 'seed-vehicles-photoid.sql');
  writeFileSync(sqlPath, sql);
  console.log(`📄 SQL seed guardado en: ${sqlPath}`);
}

main().catch(console.error);

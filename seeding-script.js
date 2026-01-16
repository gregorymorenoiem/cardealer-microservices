#!/usr/bin/env node

const axios = require("axios");

const API_BASE = "http://localhost:15070/api";

// Datos de seeding
const makes = [
  {
    name: "Toyota",
    logoUrl: "https://picsum.photos/seed/toyota/200/200",
    country: "Japan",
  },
  {
    name: "Honda",
    logoUrl: "https://picsum.photos/seed/honda/200/200",
    country: "Japan",
  },
  {
    name: "Nissan",
    logoUrl: "https://picsum.photos/seed/nissan/200/200",
    country: "Japan",
  },
  {
    name: "Ford",
    logoUrl: "https://picsum.photos/seed/ford/200/200",
    country: "USA",
  },
  {
    name: "BMW",
    logoUrl: "https://picsum.photos/seed/bmw/200/200",
    country: "Germany",
  },
  {
    name: "Mercedes-Benz",
    logoUrl: "https://picsum.photos/seed/mercedes/200/200",
    country: "Germany",
  },
  {
    name: "Tesla",
    logoUrl: "https://picsum.photos/seed/tesla/200/200",
    country: "USA",
  },
  {
    name: "Hyundai",
    logoUrl: "https://picsum.photos/seed/hyundai/200/200",
    country: "South Korea",
  },
  {
    name: "Porsche",
    logoUrl: "https://picsum.photos/seed/porsche/200/200",
    country: "Germany",
  },
  {
    name: "Chevrolet",
    logoUrl: "https://picsum.photos/seed/chevrolet/200/200",
    country: "USA",
  },
];

const modelsByMake = {
  Toyota: ["Corolla", "Camry", "RAV4", "4Runner", "Highlander", "Prius"],
  Honda: ["Civic", "Accord", "CR-V", "Pilot", "HR-V", "Fit"],
  Nissan: ["Altima", "Maxima", "Rogue", "Murano", "Frontier", "Sentra"],
  Ford: ["F-150", "Mustang", "Explorer", "Escape", "Edge", "Focus"],
  BMW: ["3 Series", "5 Series", "X5", "X3", "M340i", "M440i"],
  "Mercedes-Benz": ["C-Class", "E-Class", "GLE", "GLA", "AMG GT", "S-Class"],
  Tesla: ["Model S", "Model 3", "Model X", "Model Y"],
  Hyundai: ["Elantra", "Sonata", "Santa Fe", "Tucson", "Ioniq", "Kona"],
  Porsche: ["911", "Cayenne", "Panamera", "Macan", "Taycan"],
  Chevrolet: ["Silverado", "Colorado", "Equinox", "Blazer", "Trax"],
};

const distribution = {
  Toyota: 45,
  Nissan: 22,
  Ford: 22,
  Honda: 16,
  BMW: 15,
  "Mercedes-Benz": 15,
  Tesla: 12,
  Hyundai: 10,
  Porsche: 8,
  Chevrolet: 5,
};

// Enums según las entidades
const fuelTypes = ["Gasoline", "Diesel", "Hybrid", "Electric", "PlugInHybrid"];
const transmissionTypes = [
  "Manual",
  "Automatic",
  "SemiAutomatic",
  "CVT",
  "DualClutch",
];
const bodyStyles = [
  "Sedan",
  "SUV",
  "Coupe",
  "Hatchback",
  "Pickup",
  "Van",
  "Convertible",
  "Wagon",
];
const vehicleConditions = ["New", "Used", "Certified"];
const vehicleStatuses = ["Draft", "Active", "Sold", "Reserved", "Inactive"];
const driveTypes = ["FWD", "RWD", "AWD", "FourWD"];

const colors = [
  "White",
  "Black",
  "Silver",
  "Gray",
  "Blue",
  "Red",
  "Green",
  "Yellow",
  "Brown",
];
const interiorColors = ["Black", "Beige", "Gray", "Brown"];

function random(max) {
  return Math.floor(Math.random() * max);
}

function randomChoice(arr) {
  return arr[random(arr.length)];
}

async function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function createMakesAndModels() {
  console.log("🏭 Creando marcas y modelos...");

  const createdMakes = {};

  for (const make of makes) {
    try {
      const response = await axios.post(`${API_BASE}/catalog/makes`, {
        name: make.name,
        logoUrl: make.logoUrl,
        country: make.country,
        isPopular: true,
      });

      createdMakes[make.name] = response.data.id || response.data;
      console.log(`  ✓ Marca creada: ${make.name}`);

      // Crear modelos para esta marca
      const models = modelsByMake[make.name];
      for (const modelName of models) {
        await axios.post(`${API_BASE}/catalog/models`, {
          makeId: createdMakes[make.name],
          name: modelName,
        });
        console.log(`    ✓ Modelo creado: ${modelName}`);
      }

      await sleep(500); // Evitar sobrecarga
    } catch (error) {
      console.log(
        `  ⚠️  Error creando ${make.name}:`,
        error.response?.data || error.message
      );
    }
  }

  return createdMakes;
}

async function createVehicles(makesMap) {
  console.log("\n🚗 Creando vehículos...");

  const allMakes = Object.keys(makesMap);
  let totalCreated = 0;

  for (const makeName of allMakes) {
    const count = distribution[makeName] || 0;
    const makeId = makesMap[makeName];
    const models = modelsByMake[makeName];

    console.log(`\n📦 Creando ${count} vehículos ${makeName}...`);

    for (let i = 0; i < count; i++) {
      const modelName = randomChoice(models);
      const year = 2018 + random(7); // 2018-2024
      const mileage = 5000 + random(145000);
      const price = 15000 + random(70000);

      const vehicle = {
        title: `${year} ${makeName} ${modelName}`,
        description: `Excelente ${year} ${makeName} ${modelName} en muy buenas condiciones. Solo ${mileage.toLocaleString()} millas.`,
        price: price,
        currency: "USD",
        status: "Active",

        // Seller (temporal - se puede mejorar)
        sellerId: "00000000-0000-0000-0000-000000000001",
        sellerName: "AutoDealer RD",
        sellerType: "Dealer",
        sellerPhone: "809-555-0100",
        sellerCity: "Santo Domingo",
        sellerState: "Distrito Nacional",

        // Identificación
        makeId: makeId,
        make: makeName,
        model: modelName,
        year: year,
        vin: generateVIN(),

        // Tipo y carrocería
        vehicleType: "Car",
        bodyStyle: getBodyStyle(modelName),
        doors:
          modelName.includes("Mustang") || modelName.includes("911") ? 2 : 4,
        seats:
          modelName.includes("Highlander") || modelName.includes("Pilot")
            ? 7
            : 5,

        // Motor
        fuelType:
          makeName === "Tesla"
            ? "Electric"
            : randomChoice(fuelTypes.filter((f) => f !== "Electric")),
        engineSize: randomChoice([
          "1.5L",
          "2.0L",
          "2.5L",
          "3.0L",
          "3.5L",
          "4.0L",
        ]),
        transmission: randomChoice(transmissionTypes),
        driveType: randomChoice(driveTypes),

        // Kilometraje
        mileage: mileage,
        mileageUnit: "Miles",
        condition:
          mileage < 30000 ? "New" : mileage < 80000 ? "Certified" : "Used",

        // Apariencia
        exteriorColor: randomChoice(colors),
        interiorColor: randomChoice(interiorColors),

        // Ubicación
        city: "Santo Domingo",
        state: "Distrito Nacional",
        country: "Dominican Republic",
      };

      try {
        await axios.post(`${API_BASE}/vehicles`, vehicle);
        totalCreated++;
        process.stdout.write(
          `\r  Progreso: ${totalCreated}/150 vehículos creados`
        );
      } catch (error) {
        console.log(
          `\n  ⚠️  Error creando vehículo ${i + 1}:`,
          error.response?.data?.errors || error.message
        );
      }

      await sleep(200); // Evitar sobrecarga
    }
  }

  console.log(`\n✓ Total de vehículos creados: ${totalCreated}/150`);
}

function getBodyStyle(modelName) {
  if (
    modelName.includes("CR-V") ||
    modelName.includes("RAV4") ||
    modelName.includes("X")
  )
    return "SUV";
  if (modelName.includes("F-150") || modelName.includes("Silverado"))
    return "Pickup";
  if (modelName.includes("Mustang") || modelName.includes("911"))
    return "Coupe";
  return "Sedan";
}

function generateVIN() {
  const chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
  let vin = "";
  for (let i = 0; i < 17; i++) {
    vin += chars[random(chars.length)];
  }
  return vin;
}

async function main() {
  console.log("🌱 SEEDING V2.0 - Iniciando...\n");

  try {
    // Paso 1: Crear marcas y modelos
    const makesMap = await createMakesAndModels();

    // Paso 2: Crear vehículos
    await createVehicles(makesMap);

    console.log("\n\n🎉 SEEDING COMPLETADO EXITOSAMENTE!");
  } catch (error) {
    console.error("\n❌ Error durante el seeding:", error);
    process.exit(1);
  }
}

main();

/**
 * Unique SEO descriptions for all brand+model combinations.
 *
 * Key format: "{make-slug}-{model-slug}" (lowercase, hyphenated).
 * Used by /marcas/[marca]/[modelo] pages to avoid thin/duplicate content.
 *
 * Each description is 150–250 chars and includes:
 * - Model identity + segment positioning
 * - Key features relevant to the DR market
 * - Local context (roads, climate, usage patterns)
 *
 * @generated 2026-03-09 — Reviewed and localized for the DR market.
 */
export const MODEL_DESCRIPTIONS: Record<string, string> = {
  // ────────────────────────────────────────────────────────────────────
  // Toyota (14 models — #1 brand in DR)
  // ────────────────────────────────────────────────────────────────────
  'toyota-corolla':
    'El Toyota Corolla es el sedán más popular de República Dominicana. Reconocido por su confiabilidad, bajo consumo de combustible y excelente valor de reventa. Ideal para la ciudad y carreteras dominicanas.',
  'toyota-camry':
    'El Toyota Camry es el sedán ejecutivo referencia en RD. Motor potente, interior espacioso y equipamiento premium. Preferido por profesionales que buscan confort y estatus sin sacrificar economía.',
  'toyota-rav4':
    'La Toyota RAV4 domina el segmento SUV compacto en RD. Con tracción integral disponible, amplio espacio interior y tecnología de seguridad avanzada. Perfecta para familias dominicanas.',
  'toyota-hilux':
    'La Toyota Hilux es la pickup indestructible preferida en República Dominicana. Motor diésel turbo, capacidad de carga superior y durabilidad legendaria para trabajo y aventura.',
  'toyota-fortuner':
    'La Toyota Fortuner es la SUV de 7 plazas más confiable del mercado dominicano. Basada en la plataforma Hilux, ofrece robustez todoterreno con comodidad familiar y diésel eficiente.',
  'toyota-land-cruiser':
    'El Toyota Land Cruiser es el rey del off-road en República Dominicana. Lujo, capacidad todoterreno extrema y una reputación de fiabilidad que lo convierte en inversión de largo plazo.',
  'toyota-prado':
    'La Toyota Prado combina capacidad off-road con elegancia urbana. SUV premium de 7 plazas muy demandada en RD por su versatilidad entre ciudad, playa y montaña.',
  'toyota-yaris':
    'El Toyota Yaris es el compacto económico ideal para Santo Domingo. Consumo ultra-bajo, fácil de estacionar y con la confiabilidad Toyota. Opción inteligente para el primer vehículo.',
  'toyota-c-hr':
    'El Toyota C-HR fusiona diseño audaz con eficiencia híbrida. Crossover urbano con estilo único, seguridad Toyota Safety Sense y un manejo ágil para las calles dominicanas.',
  'toyota-highlander':
    'La Toyota Highlander es la SUV familiar de 3 filas ideal para familias grandes en RD. Espacio generoso, consumo moderado y la confiabilidad que define a Toyota.',
  'toyota-tacoma':
    'La Toyota Tacoma es la pickup mediana preferida para aventura y uso diario en RD. Tracción TRD, chasis robusto y un valor de reventa que supera a la competencia.',
  'toyota-4runner':
    'La Toyota 4Runner es una SUV todoterreno pura con chasis sobre bastidor. Capacidad off-road real, espacio familiar y durabilidad legendaria. Ideal para explorar la geografía dominicana.',
  'toyota-tundra':
    'La Toyota Tundra es la pickup full-size con motor V8 o i-FORCE MAX Twin Turbo. Capacidad de remolque masiva, tecnología avanzada y la robustez japonesa para el mercado dominicano.',
  'toyota-sequoia':
    'La Toyota Sequoia es la SUV full-size más grande de la línea Toyota. Tres filas espaciosas, motor híbrido i-FORCE MAX y lujo familiar. Ideal para quienes necesitan máximo espacio en RD.',

  // ────────────────────────────────────────────────────────────────────
  // Honda (8 models)
  // ────────────────────────────────────────────────────────────────────
  'honda-civic':
    'El Honda Civic combina deportividad con eficiencia. Uno de los sedanes más vendidos en RD, con motor VTEC, excelente manejo y alta durabilidad. Ideal para conductores jóvenes y profesionales.',
  'honda-accord':
    'El Honda Accord es el sedán mediano que define la categoría en RD. Interior lujoso, motor turbo eficiente y tecnología Honda Sensing. Preferido por ejecutivos dominicanos.',
  'honda-cr-v':
    'La Honda CR-V es la SUV familiar por excelencia. Espacioso interior, confiabilidad Honda y excelente consumo. Muy buscada en el mercado dominicano por su versatilidad.',
  'honda-hr-v':
    'La Honda HR-V es el crossover compacto perfecto para la ciudad. Interior tipo Magic Seat ultra-versátil, bajo consumo y dimensiones ideales para el tráfico de Santo Domingo.',
  'honda-pilot':
    'La Honda Pilot es la SUV de 3 filas más práctica para familias en RD. Hasta 8 pasajeros, tecnología avanzada y un manejo suave que no esperarías de una SUV de este tamaño.',
  'honda-odyssey':
    'La Honda Odyssey es la minivan premium para familias dominicanas. Asientos Magic Slide, sistema de entretenimiento trasero y el espacio más versátil de su segmento.',
  'honda-fit':
    'El Honda Fit es el subcompacto más inteligente del mercado dominicano. Interior sorprendentemente amplio gracias a Magic Seat, consumo excepcional y un precio accesible.',
  'honda-ridgeline':
    'La Honda Ridgeline es la pickup con alma de SUV. Cajón con compartimiento oculto, tracción AWD inteligente y comodidad tipo sedán. Ideal para uso diario y fin de semana en RD.',

  // ────────────────────────────────────────────────────────────────────
  // Hyundai (8 models)
  // ────────────────────────────────────────────────────────────────────
  'hyundai-elantra':
    'El Hyundai Elantra destaca por su diseño futurista y equipamiento completo. Sedán accesible con tecnología de punta, ideal para el tráfico de Santo Domingo.',
  'hyundai-sonata':
    'El Hyundai Sonata es el sedán mediano que compite con los premium. Diseño atrevido, motor turbo disponible y pantalla panorámica. Excelente opción ejecutiva en República Dominicana.',
  'hyundai-tucson':
    'La Hyundai Tucson ofrece diseño moderno, tecnología premium y excelente relación calidad-precio. Una de las SUV más vendidas en República Dominicana con garantía extendida.',
  'hyundai-santa-fe':
    'La Hyundai Santa Fe es la SUV mediana más completa del mercado dominicano. 5 o 7 plazas, motor turbo o híbrido, y un diseño que marca tendencia en RD.',
  'hyundai-accent':
    'El Hyundai Accent es el sedán de entrada más equipado en su rango de precio en RD. Confiable, económico y con garantía de 5 años. El compañero ideal para el día a día.',
  'hyundai-kona':
    'El Hyundai Kona es un crossover compacto con personalidad única. Diseño bicolor, motor turbo disponible y tecnología BlueLink. Perfecto para conductores urbanos en República Dominicana.',
  'hyundai-palisade':
    'La Hyundai Palisade redefine el lujo accesible en RD. SUV de 3 filas con acabados premium, asientos ventilados y sistema de sonido Harman Kardon. La alternativa inteligente al lujo alemán.',
  'hyundai-creta':
    'La Hyundai Creta es la SUV subcompacta ideal para la ciudad dominicana. Diseño moderno, pantalla táctil amplia y un precio que la hace accesible para jóvenes profesionales.',

  // ────────────────────────────────────────────────────────────────────
  // Kia (7 models)
  // ────────────────────────────────────────────────────────────────────
  'kia-sportage':
    'La Kia Sportage combina diseño audaz con practicidad. SUV versátil con excelente garantía, tecnología ADAS y consumo eficiente para las calles dominicanas.',
  'kia-sorento':
    'La Kia Sorento es la SUV de 7 plazas con mejor relación equipamiento-precio en RD. Motor turbo, acabados premium y la garantía más larga del mercado automotriz dominicano.',
  'kia-forte':
    'El Kia Forte ofrece más equipamiento de serie que cualquier sedán de su precio en RD. Motor eficiente, diseño deportivo y tecnología de conectividad completa.',
  'kia-seltos':
    'La Kia Seltos llena el espacio entre SUV subcompacta y compacta. AWD disponible, pantalla de 10.25" y seguridad avanzada. Muy popular entre compradores jóvenes en República Dominicana.',
  'kia-rio':
    'El Kia Rio es el subcompacto coreano con acabado premium. Económico, confiable y con garantía de 7 años. Una de las opciones más inteligentes para primer vehículo en RD.',
  'kia-soul':
    'El Kia Soul desafía las categorías con su diseño cuadrado único. Interior espacioso, personalidad propia y una comunidad de fans en República Dominicana que valora la diferencia.',
  'kia-carnival':
    'La Kia Carnival es la minivan que parece SUV premium. Asientos para 7–8 pasajeros, puertas corredizas eléctricas y un interior que rivaliza con vehículos de lujo. Ideal para familias en RD.',

  // ────────────────────────────────────────────────────────────────────
  // Nissan (8 models)
  // ────────────────────────────────────────────────────────────────────
  'nissan-sentra':
    'El Nissan Sentra ha sido un pilar del mercado dominicano durante décadas. Sedán confiable, eficiente y accesible con la nueva generación que eleva el diseño y la tecnología.',
  'nissan-altima':
    'El Nissan Altima es el sedán mediano con tracción AWD disponible. Motor VC-Turbo innovador, interior premium y tecnología ProPILOT Assist para las autopistas de República Dominicana.',
  'nissan-pathfinder':
    'La Nissan Pathfinder es la SUV de 3 filas aventurera con 7 modos de manejo. Transmisión de 9 velocidades y espacio familiar generoso. Ideal para escapadas desde Santo Domingo.',
  'nissan-frontier':
    'La Nissan Frontier es la pickup mediana robusta y accesible en RD. Chasis resistente, motor V6 potente y una capacidad de carga que satisface al trabajador dominicano.',
  'nissan-kicks':
    'El Nissan Kicks es el crossover urbano ideal para RD. Compacto por fuera, espacioso por dentro, con consumo excepcional y conectividad moderna.',
  'nissan-rogue':
    'La Nissan Rogue es la SUV compacta más tecnológica de Nissan. ProPILOT Assist, cámara de 360° y espacio inteligente. Una de las SUV más cómodas para el tráfico dominicano.',
  'nissan-versa':
    'El Nissan Versa es el sedán subcompacto con más espacio trasero de su clase. Económico, práctico y ahora con diseño moderno. Excelente primera opción en República Dominicana.',
  'nissan-x-trail':
    'La Nissan X-Trail es la SUV familiar versátil con opción de 7 plazas y e-POWER. Tracción inteligente para las carreteras montañosas de RD y consumo eficiente en ciudad.',

  // ────────────────────────────────────────────────────────────────────
  // Ford (6 models)
  // ────────────────────────────────────────────────────────────────────
  'ford-explorer':
    'La Ford Explorer ofrece espacio, potencia y tecnología. SUV de 3 filas ideal para familias grandes, con tracción integral y asistencia de conducción avanzada.',
  'ford-escape':
    'La Ford Escape es la SUV compacta con opción híbrida plug-in. Diseño aerodinámico, SYNC 4 y co-pilot360. Versatilidad y eficiencia para el conductor dominicano moderno.',
  'ford-ranger':
    'La Ford Ranger es la pickup mediana que equilibra trabajo y confort. Motor diésel turbo, capacidad de remolque y tecnología FordPass. Muy demandada en el campo dominicano.',
  'ford-f-150':
    'La Ford F-150 es la pickup más vendida del mundo y un símbolo de potencia en RD. Motor EcoBoost, aluminio militar y Pro Power Onboard. La herramienta definitiva sobre ruedas.',
  'ford-bronco':
    'El Ford Bronco regresó para dominar el off-road. Techo removible, puertas desmontables y capacidad 4×4 extrema. El rival directo del Wrangler en las aventuras dominicanas.',
  'ford-expedition':
    'La Ford Expedition es la SUV full-size con espacio para toda la familia. Hasta 8 pasajeros, motor twin-turbo y tecnología BlueCruise. Lujo americano para las autopistas de RD.',

  // ────────────────────────────────────────────────────────────────────
  // Chevrolet (6 models)
  // ────────────────────────────────────────────────────────────────────
  'chevrolet-silverado':
    'La Chevrolet Silverado es la pickup full-size americana con motor Duramax diésel o V8. Capacidad de carga y remolque masivas. Una bestia confiable para el trabajo pesado en RD.',
  'chevrolet-equinox':
    'La Chevrolet Equinox es una SUV compacta con diseño renovado y motor turbo eficiente. Espaciosa, tecnológica y accesible. Opción inteligente para familias en República Dominicana.',
  'chevrolet-tahoe':
    'La Chevrolet Tahoe es la SUV full-size con espacio monumental y motor V8. Tres filas, suspensión neumática disponible y presencia imponente en las calles dominicanas.',
  'chevrolet-trailblazer':
    'La Chevrolet Trailblazer es el crossover subcompacto con estilo bicolor. Motor turbo eficiente, pantalla de 7" y un precio que la hace accesible para jóvenes en RD.',
  'chevrolet-traverse':
    'La Chevrolet Traverse ofrece el interior más amplio de su segmento SUV en RD. Tres filas para 8 pasajeros, AWD disponible y tecnología Chevrolet Safety Assist completa.',
  'chevrolet-colorado':
    'La Chevrolet Colorado es la pickup mediana versátil con motor diésel o gasolina. Trim ZR2 para off-road extremo y capacidad de trabajo para el mercado dominicano.',

  // ────────────────────────────────────────────────────────────────────
  // Jeep (5 models)
  // ────────────────────────────────────────────────────────────────────
  'jeep-wrangler':
    'El Jeep Wrangler es un ícono del off-road en República Dominicana. Capacidad todoterreno insuperable, perfecto para explorar las montañas y playas dominicanas.',
  'jeep-grand-cherokee':
    'La Jeep Grand Cherokee combina lujo con capacidad 4×4 real. Interior premium, tecnología Uconnect 5 y sistemas Quadra-Trac. La SUV premium más deseada en RD.',
  'jeep-compass':
    'La Jeep Compass es la SUV compacta con ADN todoterreno. Diseño elegante, tracción 4×4 disponible y precio accesible. La puerta de entrada al mundo Jeep en República Dominicana.',
  'jeep-renegade':
    'La Jeep Renegade es el SUV subcompacto más capaz del mercado. Estilo retro, personalización extrema y capacidad off-road Trailhawk. Ideal para la aventura urbana en RD.',
  'jeep-gladiator':
    'La Jeep Gladiator es la única pickup mediana con capacidad off-road de Wrangler. Techo y puertas removibles, Trail Rated y cajón funcional. Única en las calles dominicanas.',

  // ────────────────────────────────────────────────────────────────────
  // Mitsubishi (5 models)
  // ────────────────────────────────────────────────────────────────────
  'mitsubishi-outlander':
    'La Mitsubishi Outlander es una SUV familiar con opción híbrida enchufable. 3 filas, tracción S-AWC y precio competitivo. Tecnología verde accesible en República Dominicana.',
  'mitsubishi-l200':
    'La Mitsubishi L200 es una pickup robusta y confiable. Motor diésel eficiente, chasis resistente y tecnología Super Select 4WD. Muy popular en el campo dominicano.',
  'mitsubishi-asx':
    'La Mitsubishi ASX es la SUV compacta con mejor relación precio-equipamiento. AWD disponible, diseño actualizado y garantía extendida. Favorita del comprador práctico en RD.',
  'mitsubishi-montero':
    'La Mitsubishi Montero es la SUV todoterreno legendaria en República Dominicana. Super Select 4WD-II, motor V6 o diésel y una reputación de indestructibilidad en terreno difícil.',
  'mitsubishi-eclipse-cross':
    'La Mitsubishi Eclipse Cross combina diseño coupé con funcionalidad SUV. Turbo, AWD y pantalla táctil amplia. El crossover que rompe moldes en el mercado dominicano.',

  // ────────────────────────────────────────────────────────────────────
  // Suzuki (4 models)
  // ────────────────────────────────────────────────────────────────────
  'suzuki-vitara':
    'La Suzuki Vitara es una SUV compacta con tracción ALLGRIP y motor turbo BoosterJet. Ágil en ciudad, capaz en caminos rurales. Relación calidad-precio excepcional en RD.',
  'suzuki-swift':
    'El Suzuki Swift es el hatchback deportivo más divertido de manejar en su precio. Ligero, ágil y económico. El favorito de conductores jóvenes en Santo Domingo y Santiago.',
  'suzuki-jimny':
    'El Suzuki Jimny es el mini todoterreno de culto. Chasis sobre bastidor, 4×4 real y estilo retro irresistible. Capaz de ir donde SUV más grandes no pueden en la geografía dominicana.',
  'suzuki-grand-vitara':
    'La Suzuki Grand Vitara regresa con motor híbrido y diseño moderno. SUV compacta con ALLGRIP-e, bajo consumo y capacidad off-road. Una evolución inteligente para el mercado de RD.',

  // ────────────────────────────────────────────────────────────────────
  // Mazda (4 models)
  // ────────────────────────────────────────────────────────────────────
  'mazda-cx-5':
    'La Mazda CX-5 ofrece diseño Kodo premium y motor SkyActiv eficiente. Interior tipo luxury con materiales nobles. La SUV compacta más refinada disponible en República Dominicana.',
  'mazda-cx-30':
    'La Mazda CX-30 cierra la brecha entre Mazda3 y CX-5. Crossover elegante con motor SkyActiv-X disponible y un interior que supera su segmento de precio en RD.',
  'mazda-mazda3':
    'El Mazda3 es el compacto con alma premium. Diseño galardonado, motor SkyActiv-G y un manejo Jinba Ittai que lo diferencia de la competencia en República Dominicana.',
  'mazda-cx-9':
    'La Mazda CX-9 es la SUV de 3 filas con refinamiento japonés. Motor turbo, interior Nappa y tecnología i-Activsense. Lujo familiar accesible en el mercado dominicano.',

  // ────────────────────────────────────────────────────────────────────
  // BMW (3 models)
  // ────────────────────────────────────────────────────────────────────
  'bmw-x3':
    'La BMW X3 es la SAV deportiva de referencia. Motor TwinPower Turbo, xDrive inteligente y tecnología iDrive 8. El equilibrio perfecto entre lujo y practicidad en las calles de RD.',
  'bmw-x5':
    'La BMW X5 es la SUV premium que definió el segmento. Presencia imponente, motores de 6 y 8 cilindros, y un interior que representa la cúspide del lujo alemán en República Dominicana.',
  'bmw-serie-3':
    'El BMW Serie 3 es el sedán deportivo referencia mundial. Tracción trasera pura, motores turbo eficientes y tecnología BMW Live Cockpit. El sueño de los entusiastas en RD.',

  // ────────────────────────────────────────────────────────────────────
  // Mercedes-Benz (3 models)
  // ────────────────────────────────────────────────────────────────────
  'mercedes-benz-clase-c':
    'El Mercedes-Benz Clase C redefine el lujo compacto. Pantalla OLED, asistente MBUX con IA y un refinamiento que solo la estrella de Stuttgart puede ofrecer en República Dominicana.',
  'mercedes-benz-gle':
    'La Mercedes-Benz GLE es la SUV de lujo con sistema E-Active Body Control. Interior de primera clase, MBUX con realidad aumentada y la presencia que impone respeto en RD.',
  'mercedes-benz-glc':
    'La Mercedes-Benz GLC combina elegancia coupé con funcionalidad SUV. Motor turbo eficiente, suspensión neumática AIRMATIC y tecnología MBUX. El crossover premium más deseado en RD.',
};

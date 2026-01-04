-- Update Mercedes-Benz Clase C AMG with missing fields
UPDATE vehicles 
SET 
  "DriveType" = '1',  -- AWD (enum value)
  "VIN" = 'WDDWF4HB9NR123456',
  "Condition" = '1',  -- Used (enum value)
  "CustomFieldsJson" = jsonb_set(
    COALESCE("CustomFieldsJson", '{}'::jsonb),
    '{mpgCity}', '24'::jsonb
  ),
  "CustomFieldsJson" = jsonb_set(
    "CustomFieldsJson",
    '{mpgHighway}', '35'::jsonb
  )
WHERE "Id" = 'a1111111-1111-1111-1111-111111111111';

-- Update BMW Serie 7 with missing fields
UPDATE vehicles 
SET 
  "DriveType" = '1',  -- AWD
  "VIN" = 'WBA7U2C57KGU12345',
  "Condition" = '1',  -- Used
  "CustomFieldsJson" = jsonb_set(
    jsonb_set(
      COALESCE("CustomFieldsJson", '{}'::jsonb),
      '{mpgCity}', '18'::jsonb
    ),
    '{mpgHighway}', '27'::jsonb
  )
WHERE "Id" = 'a2222222-2222-2222-2222-222222222222';

-- Update Porsche 911 Carrera S with missing fields
UPDATE vehicles 
SET 
  "DriveType" = '2',  -- RWD
  "VIN" = 'WP0AB2A95MS123456',
  "Condition" = '1',  -- Used
  "CustomFieldsJson" = jsonb_set(
    jsonb_set(
      COALESCE("CustomFieldsJson", '{}'::jsonb),
      '{mpgCity}', '18'::jsonb
    ),
    '{mpgHighway}', '24'::jsonb
  )
WHERE "Id" = 'a3333333-3333-3333-3333-333333333333';

-- Update Audi RS7 Sportback with missing fields
UPDATE vehicles 
SET 
  "DriveType" = '1',  -- AWD
  "VIN" = 'WUAPAAF77FN123456',
  "Condition" = '1',  -- Used
  "CustomFieldsJson" = jsonb_set(
    jsonb_set(
      COALESCE("CustomFieldsJson", '{}'::jsonb),
      '{mpgCity}', '15'::jsonb
    ),
    '{mpgHighway}', '22'::jsonb
  )
WHERE "Id" = 'a4444444-4444-4444-4444-444444444444';

-- Update Tesla Model S Plaid with missing fields
UPDATE vehicles 
SET 
  "DriveType" = '1',  -- AWD
  "VIN" = '5YJSA1E20LF123456',
  "Condition" = '1',  -- Used
  "CustomFieldsJson" = jsonb_set(
    jsonb_set(
      COALESCE("CustomFieldsJson", '{}'::jsonb),
      '{mpgCity}', '0'::jsonb
    ),
    '{mpgHighway}', '0'::jsonb
  )
WHERE "Id" = 'a5555555-5555-5555-5555-555555555555';

-- Update Range Rover Sport HSE with missing fields
UPDATE vehicles 
SET 
  "DriveType" = '1',  -- AWD
  "VIN" = 'SALGS2VF7KA123456',
  "Condition" = '1',  -- Used
  "CustomFieldsJson" = jsonb_set(
    jsonb_set(
      COALESCE("CustomFieldsJson", '{}'::jsonb),
      '{mpgCity}', '17'::jsonb
    ),
    '{mpgHighway}', '23'::jsonb
  )
WHERE "Id" = 'a6666666-6666-6666-6666-666666666666';

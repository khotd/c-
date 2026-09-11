-- Create animals table
CREATE TABLE IF NOT EXISTS animals (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    species VARCHAR(50) NOT NULL,
    breed VARCHAR(100) NOT NULL,
    age INTEGER NOT NULL,
    gender VARCHAR(10) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Available',
    description TEXT,
    shelter_id INTEGER NOT NULL,
    arrival_date DATE NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    CONSTRAINT fk_animals_shelter FOREIGN KEY (shelter_id) REFERENCES shelters(id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_animals_shelter_id ON animals(shelter_id);
CREATE INDEX IF NOT EXISTS idx_animals_species ON animals(species);
CREATE INDEX IF NOT EXISTS idx_animals_status ON animals(status);
CREATE INDEX IF NOT EXISTS idx_animals_name ON animals(name);

-- Create animal_volunteers table (many-to-many relationship)
CREATE TABLE IF NOT EXISTS animal_volunteers (
    id SERIAL PRIMARY KEY,
    animal_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    role VARCHAR(50) NOT NULL,
    assigned_date DATE NOT NULL,
    unassigned_date DATE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_animal_volunteers_animal FOREIGN KEY (animal_id) REFERENCES animals(id) ON DELETE RESTRICT,
    CONSTRAINT fk_animal_volunteers_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_animal_volunteers_animal_id ON animal_volunteers(animal_id);
CREATE INDEX IF NOT EXISTS idx_animal_volunteers_user_id ON animal_volunteers(user_id);
CREATE INDEX IF NOT EXISTS idx_animal_volunteers_role ON animal_volunteers(role);
CREATE INDEX IF NOT EXISTS idx_animal_volunteers_composite ON animal_volunteers(animal_id, user_id, role);

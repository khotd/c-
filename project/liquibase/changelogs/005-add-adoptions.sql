-- Create adoptions table
CREATE TABLE IF NOT EXISTS adoptions (
    id SERIAL PRIMARY KEY,
    animal_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    adoption_date DATE NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    notes TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    CONSTRAINT fk_adoptions_animal FOREIGN KEY (animal_id) REFERENCES animals(id) ON DELETE RESTRICT,
    CONSTRAINT fk_adoptions_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_adoptions_animal_id ON adoptions(animal_id);
CREATE INDEX IF NOT EXISTS idx_adoptions_user_id ON adoptions(user_id);
CREATE INDEX IF NOT EXISTS idx_adoptions_status ON adoptions(status);

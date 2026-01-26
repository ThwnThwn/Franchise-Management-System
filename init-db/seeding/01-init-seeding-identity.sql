-- Seed Roles
INSERT INTO roles (name, slug, description)
VALUES
('Admin', 'admin', 'System administrator'),
('User', 'user', 'Normal user')
ON CONFLICT (slug) DO NOTHING;

-- Seed Admin user (password: admin123)
INSERT INTO users (email, password, username, role_id)
VALUES (
    'admin@gmail.com',
    '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW',
    'admin',
    (SELECT id FROM roles WHERE slug = 'admin')
)
ON CONFLICT (email) DO NOTHING;

-- Seed Normal users (password: user123)
INSERT INTO users (email, password, username, role_id)
VALUES
('user@gmail.com',   '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user',   (SELECT id FROM roles WHERE slug = 'user')),
('user1@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user1',  (SELECT id FROM roles WHERE slug = 'user')),
('user2@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user2',  (SELECT id FROM roles WHERE slug = 'user')),
('user3@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user3',  (SELECT id FROM roles WHERE slug = 'user')),
('user4@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user4',  (SELECT id FROM roles WHERE slug = 'user')),
('user5@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user5',  (SELECT id FROM roles WHERE slug = 'user')),
('user6@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user6',  (SELECT id FROM roles WHERE slug = 'user')),
('user7@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user7',  (SELECT id FROM roles WHERE slug = 'user')),
('user8@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user8',  (SELECT id FROM roles WHERE slug = 'user')),
('user9@gmail.com',  '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user9',  (SELECT id FROM roles WHERE slug = 'user')),
('user10@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user10', (SELECT id FROM roles WHERE slug = 'user')),
('user11@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user11', (SELECT id FROM roles WHERE slug = 'user')),
('user12@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user12', (SELECT id FROM roles WHERE slug = 'user')),
('user13@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user13', (SELECT id FROM roles WHERE slug = 'user')),
('user14@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user14', (SELECT id FROM roles WHERE slug = 'user')),
('user15@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user15', (SELECT id FROM roles WHERE slug = 'user')),
('user16@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user16', (SELECT id FROM roles WHERE slug = 'user')),
('user17@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user17', (SELECT id FROM roles WHERE slug = 'user')),
('user18@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user18', (SELECT id FROM roles WHERE slug = 'user')),
('user19@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user19', (SELECT id FROM roles WHERE slug = 'user')),
('user20@gmail.com', '$2a$12$Qr568FhLXUaY915P8eO3Iuj1IPw3PQW9Pt4K42gNdMCO.KQYk7wvW','user20', (SELECT id FROM roles WHERE slug = 'user'))
ON CONFLICT (email) DO NOTHING;

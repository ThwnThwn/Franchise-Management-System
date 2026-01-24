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
    '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK',
    'admin',
    (SELECT id FROM roles WHERE slug = 'admin')
)
ON CONFLICT (email) DO NOTHING;

-- Seed Normal users (password: user123)
INSERT INTO users (email, password, username, role_id)
VALUES
('user@gmail.com',   '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user',   (SELECT id FROM roles WHERE slug = 'user')),
('user1@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user1',  (SELECT id FROM roles WHERE slug = 'user')),
('user2@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user2',  (SELECT id FROM roles WHERE slug = 'user')),
('user3@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user3',  (SELECT id FROM roles WHERE slug = 'user')),
('user4@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user4',  (SELECT id FROM roles WHERE slug = 'user')),
('user5@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user5',  (SELECT id FROM roles WHERE slug = 'user')),
('user6@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user6',  (SELECT id FROM roles WHERE slug = 'user')),
('user7@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user7',  (SELECT id FROM roles WHERE slug = 'user')),
('user8@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user8',  (SELECT id FROM roles WHERE slug = 'user')),
('user9@gmail.com',  '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user9',  (SELECT id FROM roles WHERE slug = 'user')),
('user10@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user10', (SELECT id FROM roles WHERE slug = 'user')),
('user11@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user11', (SELECT id FROM roles WHERE slug = 'user')),
('user12@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user12', (SELECT id FROM roles WHERE slug = 'user')),
('user13@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user13', (SELECT id FROM roles WHERE slug = 'user')),
('user14@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user14', (SELECT id FROM roles WHERE slug = 'user')),
('user15@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user15', (SELECT id FROM roles WHERE slug = 'user')),
('user16@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user16', (SELECT id FROM roles WHERE slug = 'user')),
('user17@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user17', (SELECT id FROM roles WHERE slug = 'user')),
('user18@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user18', (SELECT id FROM roles WHERE slug = 'user')),
('user19@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user19', (SELECT id FROM roles WHERE slug = 'user')),
('user20@gmail.com', '$2a$11$u8Q7aP1QH3oH1Wl9kGf3eO8hX2z2m3V9rH1uQ8v2lq9tqzKk2p1yK','user20', (SELECT id FROM roles WHERE slug = 'user'))
ON CONFLICT (email) DO NOTHING;

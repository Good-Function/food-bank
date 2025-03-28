INSERT INTO users(Id, Email, Password)
VALUES (0, 'developer.admin@bzsos.pl', 'f00d!')
ON CONFLICT DO NOTHING;

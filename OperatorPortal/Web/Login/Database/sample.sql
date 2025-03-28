INSERT INTO users(Id, Email, Password)
VALUES (0, 'developer.admin@bzsos.pl', '$FirstPassword$')
ON CONFLICT DO NOTHING;

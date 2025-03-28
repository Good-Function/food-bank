CREATE TABLE IF NOT EXISTS users
(
    Id BIGINT CONSTRAINT users_pk PRIMARY KEY,
    Email Text,
    Password Text
);

INSERT INTO users(Id, Email, Password)
VALUES (1, 'admin@admin.pl', '$FirstPassword$')
ON CONFLICT DO NOTHING;

CREATE TABLE IF NOT EXISTS users
(
    Id BIGINT CONSTRAINT users_pk PRIMARY KEY,
    Name Text,
    Password Text
);

INSERT INTO users(Id, Name, Password)
VALUES(1, 'Admin', 'f00d!') 
ON CONFLICT DO NOTHING;

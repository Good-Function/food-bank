CREATE TABLE IF NOT EXISTS users
(
    Id BIGINT CONSTRAINT users_pk PRIMARY KEY,
    Email Text,
    Password Text
);

INSERT INTO users(Id, Email, Password)
VALUES (1, 'admin@admin.pl', '$2a$11$CMUfl4AgQKyhyXd50dRCDe5V7okv0C/MhrnlvFtXI.ya2t1Wy8IXS')
ON CONFLICT DO NOTHING;

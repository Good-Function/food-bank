INSERT INTO users(Id, Email, Password)
VALUES (0, 'developer.admin@bzsos.pl', '$2a$11$CMUfl4AgQKyhyXd50dRCDe5V7okv0C/MhrnlvFtXI.ya2t1Wy8IXS')
ON CONFLICT DO NOTHING;

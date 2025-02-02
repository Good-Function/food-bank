CREATE TABLE students (
    id bigint constraint pk_students primary key,
    name VARCHAR(255) NOT NULL,
    surname VARCHAR(255) NOT NULL,
    age INT NOT NULL
);
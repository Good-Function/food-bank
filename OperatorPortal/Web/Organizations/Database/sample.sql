DELETE FROM organizacje;
INSERT INTO organizacje (Teczka,
                         IdentyfikatorEnova,
                         NIP,
                         Regon,
                         KrsNr,
                         FormaPrawna,
                         OPP,
                         NazwaOrganizacjiPodpisujacejUmowe,
                         AdresRejestrowy,
                         NazwaPlacowkiTrafiaZywnosc,
                         AdresPlacowkiTrafiaZywnosc,
                         GminaDzielnica,
                         Powiat,
                         NazwaOrganizacjiKsiegowanieDarowizn,
                         KsiegowanieAdres,
                         TelOrganProwadzacegoKsiegowosc,
                         WwwFacebook,
                         Telefon,
                         Przedstawiciel,
                         Kontakt,
                         Email,
                         Dostepnosc,
                         OsobaDoKontaktu,
                         TelefonOsobyKontaktowej,
                         MailOsobyKontaktowej,
                         OsobaOdbierajacaZywnosc,
                         TelefonOsobyOdbierajacej,
                         LiczbaBeneficjentow,
                         Beneficjenci,
                         Sieci,
                         Bazarki,
                         Machfit,
                         FEPZ2024,
                         OdbiorKrotkiTermin,
                         TylkoNaszMagazyn,
                         Kategoria,
                         RodzajPomocy,
                         SposobUdzielaniaPomocy,
                         WarunkiMagazynowe,
                         HACCP,
                         Sanepid,
                         TransportOpis,
                         TransportKategoria,
                         Wniosek,
                         UmowaZDn,
                         UmowaRODO,
                         KartyOrganizacjiData,
                         OstatnieOdwiedzinyData)
VALUES (101,
        1234567890123,
        1234567890,
        123456789,
        '0000123456',
        'Stowarzyszenie',
        FALSE,
        'Stowarzyszenie Pomocy Społecznej "Nadzieja"',
        'ul. Kwiatowa 15, 00-001 Warszawa',
        'Jadłodajnia "Nadzieja"',
        'ul. Kwiatowa 15, 00-001 Warszawa',
        'Śródmieście',
        'Warszawa',
        'Stowarzyszenie Pomocy Społecznej "Nadzieja"',
        'ul. Kwiatowa 15, 00-001 Warszawa',
        '+48 22 123 45 67',
        'www.nadzieja.org',
        '+48 22 123 45 67',
        'Jan Kowalski',
        'Jan Kowalski, +48 22 123 45 67',
        'kontakt@nadzieja.org',
        'Pon-Pt 8:00-16:00',
        'Anna Nowak',
        '+48 22 123 45 69',
        'anna.nowak@nadzieja.org',
        'Maria Wiśniewska',
        '+48 22 123 45 70',
        150,
        'Rodziny wielodzietne, osoby starsze',
        TRUE,
        FALSE,
        TRUE,
        TRUE,
        TRUE,
        FALSE,
        'Pomoc żywnościowa',
        'Dystrybucja paczek żywnościowych',
        'Dostawa do placówki',
        'Magazyn chłodniczy',
        TRUE,
        TRUE,
        'Własny transport',
        'Chłodnia',
        '2023-10-01',
        '2023-10-05',
        NULL,
        '2023-10-10',
        '2023-10-15'),
       (102,
        9876543210987,
        9876543210,
        987654321,
        '0000654321',
        'Fundacja',
        TRUE,
        'Fundacja "Dobroczynność"',
        'ul. Lipowa 7, 30-001 Kraków',
        'Świetlica "Dobroczynność"',
        'ul. Lipowa 7, 30-001 Kraków',
        'Stare Miasto',
        'Kraków',
        'Fundacja "Dobroczynność"',
        'ul. Lipowa 7, 30-001 Kraków',
        '+48 12 345 67 89',
        'www.dobroczynnosc.org',
        '+48 12 345 67 89',
        'Piotr Nowak',
        'Piotr Nowak, +48 12 345 67 89',
        'kontakt@dobroczynnosc.org',
        'Pon-Pt 9:00-17:00',
        'Katarzyna Kowalska',
        '+48 12 345 67 91',
        'katarzyna.kowalska@dobroczynnosc.org',
        'Tomasz Wiśniewski',
        '+48 12 345 67 92',
        200,
        'Dzieci i młodzież z ubogich rodzin',
        FALSE,
        TRUE,
        FALSE,
        TRUE,
        TRUE,
        FALSE,
        'Pomoc edukacyjna',
        'Dystrybucja materiałów szkolnych',
        'Dostawa do placówki',
        'Magazyn suchy',
        FALSE,
        TRUE,
        'Wynajęty transport',
        'Suchy',
        '2023-09-15',
        '2023-09-20',
        '2023-09-30',
        '2023-09-25',
        '2023-09-30'),
       (103,
        4567890123456,
        4567890123,
        456789012,
        '0000789012',
        'Spółka z o.o.',
        FALSE,
        'Spółka "Zdrowie i Smak"',
        'ul. Ogrodowa 22, 50-001 Wrocław',
        'Restauracja "Zdrowie i Smak"',
        'ul. Ogrodowa 22, 50-001 Wrocław',
        'Krzyki',
        'Wrocław',
        'Spółka "Zdrowie i Smak"',
        'ul. Ogrodowa 22, 50-001 Wrocław',
        '+48 71 234 56 78',
        'www.zdrowieismak.pl',
        '+48 71 234 56 78',
        'Marek Zieliński',
        'Marek Zieliński, +48 71 234 56 78',
        'kontakt@zdrowieismak.pl',
        'Pon-Pt 10:00-18:00',
        'Agnieszka Nowak',
        '+48 71 234 56 80',
        'agnieszka.nowak@zdrowieismak.pl',
        'Jan Kowalczyk',
        '+48 71 234 56 81',
        100,
        'Osoby bezdomne',
        TRUE,
        FALSE,
        TRUE,
        FALSE,
        FALSE,
        TRUE,
        'Pomoc żywnościowa',
        'Dostawa posiłków',
        'Dostawa do placówki',
        'Magazyn suchy',
        TRUE,
        FALSE,
        'Własny transport',
        'Suchy',
        '2023-08-01',
        '2023-08-05',
        '2023-03-23',
        '2023-08-10',
        '2023-08-15'),
       (104,
        7890123456789,
        7890123456,
        789012345,
        '0000456789',
        'Stowarzyszenie',
        TRUE,
        'Stowarzyszenie "Razem dla Dzieci"',
        'ul. Słoneczna 5, 80-001 Gdańsk',
        'Przedszkole "Razem dla Dzieci"',
        'ul. Słoneczna 5, 80-001 Gdańsk',
        'Śródmieście',
        'Gdańsk',
        'Stowarzyszenie "Razem dla Dzieci"',
        'ul. Słoneczna 5, 80-001 Gdańsk',
        '+48 58 345 67 89',
        'www.razemdladzieci.org',
        '+48 58 345 67 89',
        'Ewa Wiśniewska',
        'Ewa Wiśniewska, +48 58 345 67 89',
        'kontakt@razemdladzieci.org',
        'Pon-Pt 7:00-15:00',
        'Tomasz Kowalski',
        '+48 58 345 67 91',
        'tomasz.kowalski@razemdladzieci.org',
        'Anna Zielińska',
        '+48 58 345 67 92',
        120,
        'Dzieci z rodzin ubogich',
        FALSE,
        TRUE,
        FALSE,
        TRUE,
        TRUE,
        TRUE,
        'Pomoc żywnościowa',
        'Dystrybucja paczek żywnościowych',
        'Dostawa do placówki',
        'Magazyn chłodniczy',
        TRUE,
        TRUE,
        'Wynajęty transport',
        'Chłodnia',
        '2023-07-01',
        '2023-07-05',
        NULL,
        '2023-07-10',
        '2023-07-15'),
       (105,
        3210987654321,
        3210987654,
        321098765,
        '0000321098',
        'Fundacja',
        FALSE,
        'Fundacja "Pomocna Dłoń"',
        'ul. Leśna 12, 90-001 Łódź',
        'Jadłodajnia "Pomocna Dłoń"',
        'ul. Leśna 12, 90-001 Łódź',
        'Widzew',
        'Łódź',
        'Fundacja "Pomocna Dłoń"',
        'ul. Leśna 12, 90-001 Łódź',
        '+48 42 123 45 67',
        'www.pomocnadlon.org',
        '+48 42 123 45 67',
        'Krzysztof Nowak',
        'Krzysztof Nowak, +48 42 123 45 67',
        'kontakt@pomocnadlon.org',
        'Pon-Pt 8:00-16:00',
        'Magdalena Kowalska',
        '+48 42 123 45 69',
        'magdalena.kowalska@pomocnadlon.org',
        'Piotr Wiśniewski',
        '+48 42 123 45 70',
        180,
        'Osoby starsze, samotne',
        TRUE,
        FALSE,
        TRUE,
        FALSE,
        TRUE,
        FALSE,
        'Pomoc żywnościowa',
        'Dystrybucja paczek żywnościowych',
        'Dostawa do placówki',
        'Magazyn suchy',
        FALSE,
        TRUE,
        'Własny transport',
        'Suchy',
        '2023-06-01',
        '2023-06-05',
        '2023-06-07',
        '2023-06-10',
        '2023-06-15');
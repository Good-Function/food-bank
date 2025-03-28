CREATE TABLE IF NOT EXISTS organizacje
(
    Teczka                              BIGINT CONSTRAINT organizacje_pk PRIMARY KEY,
    IdentyfikatorEnova                  BIGINT  NOT NULL,
    NIP                                 BIGINT  NOT NULL,
    Regon                               BIGINT  NOT NULL,
    KrsNr                               TEXT    NOT NULL,
    FormaPrawna                         TEXT    NOT NULL,
    OPP                                 BOOLEAN NOT NULL,
    NazwaOrganizacjiPodpisujacejUmowe   TEXT    NOT NULL,
    AdresRejestrowy                     TEXT    NOT NULL,
    NazwaPlacowkiTrafiaZywnosc          TEXT    NOT NULL,
    AdresPlacowkiTrafiaZywnosc          TEXT    NOT NULL,
    GminaDzielnica                      TEXT    NOT NULL,
    Powiat                              TEXT    NOT NULL,
    NazwaOrganizacjiKsiegowanieDarowizn TEXT    NOT NULL,
    KsiegowanieAdres                    TEXT    NOT NULL,
    TelOrganProwadzacegoKsiegowosc      TEXT    NOT NULL,
    WwwFacebook                         TEXT    NOT NULL,
    Telefon                             TEXT    NOT NULL,
    Przedstawiciel                      TEXT    NOT NULL,
    Kontakt                             TEXT    NOT NULL,
    Email                               TEXT    NOT NULL,
    Dostepnosc                          TEXT    NOT NULL,
    OsobaDoKontaktu                     TEXT    NOT NULL,
    TelefonOsobyKontaktowej             TEXT    NOT NULL,
    MailOsobyKontaktowej                TEXT    NOT NULL,
    OsobaOdbierajacaZywnosc             TEXT    NOT NULL,
    TelefonOsobyOdbierajacej            TEXT    NOT NULL,
    LiczbaBeneficjentow                 INT     NOT NULL,
    Beneficjenci                        TEXT    NOT NULL,
    Sieci                               BOOLEAN NOT NULL,
    Bazarki                             BOOLEAN NOT NULL,
    Machfit                             BOOLEAN NOT NULL,
    FEPZ2024                            BOOLEAN NOT NULL,
    Kategoria                           TEXT    NOT NULL,
    RodzajPomocy                        TEXT    NOT NULL,
    SposobUdzielaniaPomocy              TEXT    NOT NULL,
    WarunkiMagazynowe                   TEXT    NOT NULL,
    HACCP                               BOOLEAN NOT NULL,
    Sanepid                             BOOLEAN NOT NULL,
    TransportOpis                       TEXT    NOT NULL,
    TransportKategoria                  TEXT    NOT NULL,
    Wniosek                             DATE    NULL,
    UmowaZDn                            DATE    NULL,
    UmowaRODO                           DATE    NULL,
    KartyOrganizacjiData                DATE    NULL,
    OstatnieOdwiedzinyData              DATE    NULL
);

# Specifikace projektu – Turnajový systém

## 1. Slovník pojmů
* **Turnaj:** Organizovaná soutěž mezi více účastníky (týmy nebo jednotlivci) probíhající v jednom či více kolech.
* **Zápas (Utkání):** Konfrontace dvou soupeřů v rámci jednoho kola, jejímž výsledkem je výhra jednoho, prohra druhého, případně remíza.
* **Účastník (Hráč / Tým):** Subjekt přihlášený do turnaje, který odehrává zápasy a sbírá body do tabulky.
* **Volný los (Bye):** Situace, kdy má v daném kole účastník (z důvodu lichého počtu hráčů) volno, automaticky postupuje nebo získává body bez boje.
* **Tabulka (Žebříček):** Průběžné či konečné pořadí účastníků seřazené primárně podle získaných bodů a sekundárně podle pomocných kritérií (např. skóre).

---

## 2. Pravidla bodování
Bodové ohodnocení zápasů v základní části:
* **Výhra:** `1` body 
* **Remíza:** `0.5` bod
* **Prohra:** `0` bodů
* **Volný los (Bye):** `1` body 

---

## 3. Seznam funkcí (Features)
* **Správa účastníků:**
  * Přidání a odebrání účastníků turnaje.
  * Generování rozlosování (vsichni proti vsem).
* **Správa zápasů a výsledků:**
  * Zadávání a úprava výsledků jednotlivých utkání.
  * Ošetření situací s lichým počtem hráčů (automatické přidělení volného losu).
* **Výpočet pořadí a tabulka:**
  * Automatický přepočet bodů podle definovaného bodovacího systému.
  * Řazení účastníků v tabulce podle bodů (a případného skóre / vzájemného zápasu).
* **Ukládání a načítání dat:**
  * Možnost uložit stav turnaje (do souboru / databáze) a později jej načíst.

# Tasks

## 1. Theme Content

- [x] 1.1 Add the `DraftTheme` enum and `CardDef.DraftTheme`; update the clone path. Verify with `dotnet build SnapClone.sln`.
- [x] 1.2 Assign a theme to all 59 cards with the table in `design.md`; validate that every draftable card has a theme and tokens do not. Verify with a theme-coverage harness.
- [x] 1.3 Add the draft theme to the card detail and the draft card view. Verify both show the theme at the smallest viewport. (Ajustado: la coleccion aun no existe; el tema se anadio al detalle compartido `DetailPreviewView`, que la coleccion reutilizara, y a `CardView` en el draft.)

## 2. Starter Set and Catalog

- [x] 2.1 Replace the 13 starters with the new Control/Destroy/OnReveal/Neutral set and keep Abomination, Misty Knight and Shocker in the catalog as non-starters. Verify the 59 unique ids and names with a catalog harness.
- [x] 2.2 Update the first-run owned set and the catalog union rule; verify the collection and the global-store partition derive from the new starter set. (Ajustado: la tienda global y la coleccion pertenecen a `meta-store-and-collection`, aun no implementado; se verifico la derivacion de datos: perfil nuevo = 13 starters, catalogo = 59 sin duplicados.)
- [x] 2.3 Verify every starter ability still resolves and the new starters have their existing effects.

## 3. Themed Draft

- [x] 3.1 Replace `DraftSystem` with `DraftState`, the three themed groups and the five random rounds; generate the groups once and keep them disjoint. Verify the 3+5 flow with a seeded run.
- [x] 3.2 Enforce the 1-copy limit during the draft and return unpicked themed cards to the random pool. Verify a card is never offered twice and the second copy is not added.
- [x] 3.3 Make the draft use only owned cards; verify an unowned card is never offered and the starter set covers the three themes.
- [x] 3.4 Persist phase, step, groups, offers, picks and copy counts after every pick; verify a resumed draft shows the same theme and offers.

## 4. Level-Up and UI

- [x] 4.1 Make the level-up card branch offer owned cards only. Verify with a profile that owns the 13 starters and one store card.
- [x] 4.2 Update `DraftScene` with the theme label for rounds 0-2, `ALEATORIO` for rounds 3-7 and the 8-pick progress. Verify the themed and random rounds.
- [x] 4.3 Remove the draft perk-assignment step and its dead code. Verify the draft hands a plain 8-card deck to combat.

## 5. Integration Verification

- [x] 5.1 Run the theme, starter, draft-flow, copy-limit and resume harnesses together.
- [x] 5.2 Manual pass: complete a draft with the three themes, verify the 8-card deck, resume a mid-draft checkpoint and play the first combat.
- [x] 5.3 Run the desktop and Android builds and regenerate the APK.

# Perks de habilidad

Los perks de la run son **habilidades que viajan con una carta**. No son reglas
globales del lado: cuando un perk se adjunta a una carta, solo esa carta gana la
habilidad y la usa desde su propia mano, mazo o zona segun el texto.

Este documento describe el catalogo vigente de 28 perks, las reglas de
compatibilidad y el modelo de adquisicion. La fuente de verdad en codigo es
`SnapClone/Data/Definitions/PerkCatalog.cs`; el contenido se registra en
`ContentLoader` y se resuelve con `EffectProcessor`.

## Modelo

- **Anfitrion:** carta del mazo de la run que recibe el perk.
- **Copia:** cada perk puede aparecer como maximo dos veces en el mazo.
- **Disponibilidad:** los 28 perks existen desde el inicio. No hay perks de
  tienda ni desbloqueos.
- **Resolucion:** los efectos usan al anfitrion como `SourceCard`, su zona como
  `SourceZone` y a su dueno como `Owner`. Se despachan por el mismo procesador
  que las habilidades impresas de las cartas.
- **Estado persistido:** `CardInstance.AttachedPerks` guarda los ids del perk y
  sobrevive a combates, movimientos y retornos a la mano.

## Catalogo

| # | Perk | Tipo | Requisito | Efecto |
|---|---|---|---|---|
| 1 | Afterimage | Habilidad | Cualquier carta | Al moverse, una copia sin mejoras ni perks queda en la zona anterior. |
| 2 | Amplify | Meta | Efecto Continuo | El efecto Continuo del anfitrion se aplica una vez mas por copia. |
| 3 | Apex | Habilidad | Cualquier carta | Continuo: si es la unica carta aliada de su zona, duplica su Poder final. |
| 4 | Clone | Habilidad | Cualquier carta | Al iniciar el combate, baraja una copia sin mejoras al mazo. |
| 5 | Deadlink | Habilidad | Cualquier carta | Al revelarse, cada Horda Zombie aliada gana +4 de Poder. |
| 6 | Encore | Meta | Efecto Al revelarse | La habilidad Al revelarse del anfitrion se repite por copia. |
| 7 | Hot Pursuit | Regla | Cualquier carta | Cuesta 3 menos si una carta se movio el turno anterior. |
| 8 | Maneuverable | Regla | Cualquier carta | Permite un movimiento manual por combate; los de efecto no lo consumen. |
| 9 | Mentor | Habilidad | Cualquier carta | Al revelarse, la carta superior del mazo gana el Poder actual del anfitrion. |
| 10 | Overcharge | Habilidad | Cualquier carta | Al revelarse, si ganas su zona el siguiente turno, +1 de Energia. |
| 11 | Overload | Habilidad | Cualquier carta | Inicio del turno: +1 de Energia si ganas la zona del anfitrion. |
| 12 | Overtime | Meta | Efecto Fin de turno | La habilidad de Fin de turno del anfitrion se repite por copia. |
| 13 | Quickdraw: Discount | Regla | Cualquier carta | Al robarla, cuesta 2 menos el resto del combate. |
| 14 | Quickdraw: Power | Habilidad | Cualquier carta | Al robarla, gana Al revelarse: +3 de Poder el resto del combate. |
| 15 | Rally | Habilidad | Cualquier carta | Continuo: +1 de Poder por cada otra carta aliada en su zona. |
| 16 | Reality Warp | Habilidad | Cualquier carta | Al revelarse, reemplaza su locacion por una nueva. |
| 17 | Regeneration | Habilidad | Cualquier carta | Al ser descartada o destruida, vuelve con +2 de Poder a una zona con espacio. |
| 18 | Regroup | Habilidad | Cualquier carta | Al revelarse, devuelve a la mano la carta de menor coste de su zona y le da +3. |
| 19 | Reinforce | Meta | Cualquier carta | Cuando otro efecto la potencia, gana +1 de Poder por copia. |
| 20 | Salvage | Habilidad | Cualquier carta | Al revelarse, destruye la otra carta de menor Poder de su zona y gana +5. |
| 21 | Sidekick | Habilidad | Cualquier carta | Al revelarse, anade un personaje aleatorio a la mano con -2 de coste. |
| 22 | Silence | Habilidad | Cualquier carta | Apaga el texto impreso del anfitrion; sus perks siguen activos. |
| 23 | Slingshot | Habilidad | Cualquier carta | Al revelarse, otra carta aliada de su zona gana +1 y se mueve a la derecha. |
| 24 | Superstar | Regla | Cualquier carta | Empieza en la mano inicial antes que las cartas aleatorias. |
| 25 | Tag In | Habilidad | Cualquier carta | Cuando destruyen una carta aliada, entra desde mano o mazo a esa zona. |
| 26 | Taunt | Regla | Cualquier carta | Al revelarse, la primera carta rival del siguiente turno va a esa zona. |
| 27 | Unstoppable | Regla | Cualquier carta | Juega en cualquier zona, no puede ser destruida ni perder Poder. |
| 28 | Wakandan Tech | Habilidad | Cualquier carta | Al revelarse, duplica su Poder con un bono persistente igual al poder previo. |

## Compatibilidad

Un perk solo se ofrece si al menos una carta del mazo puede hospedarlo, y la
grilla de anfitriones deshabilita las cartas incompatibles mostrando el motivo.

| Requisito | Quien cumple |
|---|---|
| Cualquier carta | Todo el catalogo base, salvo que el perk pida otra cosa. |
| Efecto Continuo | Cartas con al menos un efecto `Ongoing`, incluyendo los que aporten otros perks. |
| Efecto Al revelarse | Cartas con al menos un efecto `OnReveal`. |
| Efecto Fin de turno | Cartas con al menos un efecto `EndOfTurn`. |
| Cualquier carta (potenciable) | Reinforce acepta cualquier anfitrion porque cualquier carta puede recibir Empowered. |

Una oferta ya generada no se rebaraja al cambiar el mazo. Una carta nueva que
aporte el rasgo requerido habilita el perk para las **siguientes** ofertas.

## Adquisicion

- **Level-up:** la rama de perk ofrece hasta tres perks del catalogo completo
  que tengan un anfitrion compatible y menos de dos copias en el mazo.
- **Sin perks compatibles:** la rama de perk se muestra no disponible y la rama
  de carta sigue accesible.
- **Eventos de run:** un evento que concede un perk usa el mismo filtro de
  compatibilidad y adjunta el perk a un anfitrion valido.
- **Tienda global:** no vende perks. Solo vende vida extra, cartas directas,
  lotes y el objeto de combate reservado.
- **Coleccion:** la pestana de perks lista los 28 como propios, con texto,
  requisito y copias adjuntas en el mazo actual.

## Triggers y eventos

El procesador resuelve `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`,
`AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `GameStart`, `StartOfTurn`,
`EndOfTurn` y `Empowered`. `Activate` sigue declarado pero no dispara porque no
hay contenido que lo use.

- **Start of Turn** se resuelve antes de la fase de juego y antes de cerrar la
  energia del turno. La energia concedida vale solo para ese turno.
- **End of Turn** se resuelve despues de la secuencia de reveal y antes de
  prioridad; dispara una vez por instancia y turno.
- **Empowered** se dispara cuando un efecto distinto de Reinforce aumenta el
  Poder de una carta. Reinforce agrega su +1 sin volver a emitir el evento.
- **Repeticiones:** Amplify, Encore y Overtime se limitan a las copias adjuntas,
  excluyen su propia meta-habilidad y nunca recursan; el limite de eventos del
  procesador corta cualquier cascada forzada.

## Persistencia y migracion

- No se agrega un archivo de guardado ni cambia el esquema de `profile.json` o
  `run.json`.
- Al cargar un perfil se eliminan de `OwnedPerkIds` y `PerkCopyCounts` los ids
  retirados o desconocidos, y se garantiza que los 28 actuales esten presentes.
- Al cargar una run se eliminan de cada carta los perks retirados o
  desconocidos; la carta, sus mejoras, su delta de coste y su delta de Poder
  sobreviven, y la run continua en el mismo nodo.
- El siguiente guardado escribe el estado limpio.

### Perdida irreversible

La migracion **no mapea** los ocho perks de regla retirados a perks nuevos:
`perk_vanguardia`, `perk_aura`, `perk_francotirador`, `perk_letal`,
`perk_agil`, `perk_guardian`, `perk_imparable` y `perk_codicioso` desaparecen de
los saves. Volver a una version anterior del codigo no restaura esos adjuntos;
el perfil y la run se conservan, pero empiezan sin esos perks. La perdida es
deliberada porque no existe una equivalencia honesta entre una regla global y
una habilidad de anfitrion.

## Cobertura de verificacion

`SnapClone.Harness` comprueba:

- los 28 ids, la ausencia de los retirados, los efectos compartidos y el token
  `token_zombie_horde` (0 coste, 0 Poder, no draftable);
- el lookup de habilidades efectivas y el despacho de perks del anfitrion;
- compatibilidad, filtrado de ofertas, rama no disponible y reentrada sin
  rebarajar;
- cada comportamiento del catalogo, el orden Start/End of Turn, Empowered,
  Quickdraw, Taunt (jugador e IA), Unstoppable, Maneuverable y los limites de
  repeticion y de eventos;
- la migracion de perfil y de run, incluido el guardado limpio posterior.

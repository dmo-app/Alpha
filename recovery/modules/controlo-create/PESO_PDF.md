# Controlo Create — Peso PDF

The Peso PDF is the final production document derived from one approved peso_id.

It is a presentation/read artifact. It does not own production, Tool, calculation, approval or historical-comparison truth.

~~~text
approved peso_id
→ backend resolves truthful source relations
→ backend builds one purpose-specific Peso PDF read model
→ document renderer formats that packet
→ frontend/document layer does not re-derive domain facts
~~~

## Canonical content structure

~~~text
Controlo de Peso e Volume
├── approval/document header
├── Produção
├── Contra Molde (CM)
├── Referências técnicas
├── comparison with selected historical control
├── comparison by CM
├── technical/reference values
└── traceability
~~~

The PDF must avoid repeating the same production facts in several unrelated header blocks.

## Production block

| PDF field | Canonical source | Backend resolution |
| --- | --- | --- |
| Referência | Job On production reference | peso_id → production/Controlo context → jobon_id |
| Produção | Job On production number | same Job On relation |
| Linha | Job On machine/line | same Job On relation |
| Data | Job On production start date | same Job On relation |

The PDF must not persist a second copy merely to render these values.

## Contra Molde (CM) block

CM-specific facts stay grouped with the CM. Lot must not be displayed as a generic production fact.

| PDF field | Canonical source | Backend resolution |
| --- | --- | --- |
| Referência CM | selected CM Tool/context | peso_id → cm_id → tool_id |
| Lote | selected canonical CM Tool / preserved CM production context where historical truth requires it | cm_id → tool_id |
| Estado | selected CM Tool/context state | cm_id → tool_id / preserved production context |

Normal display uses the raw lot value:

~~~text
Lote: 8
~~~

not:

~~~text
Lote: L8
~~~

The L prefix is only a filename-formatting convention when a filename explicitly requires a lot token. It is not part of the lot domain value.

## Top technical-reference block

The former generic Boquilha block is replaced by the technical references required by Peso.

| PDF field | Canonical source | Backend resolution |
| --- | --- | --- |
| Processo | Tool fact of the selected Peso/CM Tool; current values are NNPB or PS | peso_id → cm_id → tool_id → Tool.process |
| Volume BQ | BQ Tool-owned technical value used by Peso; product-facing label for volume_marisa | peso_id → jobon_id → bq_id → BQ tool_id → tool_technical_values.volume_marisa |
| Volume PU | CM Tool-owned technical value used by Peso; product-facing label for volume_puncao | peso_id → cm_id → CM tool_id → tool_technical_values.volume_puncao |
| Volume TP | production-specific TP/Calote value | peso_id → jobon_id → TP/Calote production context |

Volume TP is shown as technical context. It does not become a term in the main Peso formula merely because it appears in the PDF.

Missing owner values must remain missing/refused according to the owning workflow. The PDF must never invent substitute technical values.

## Current Peso values

The document compares glass weight, not water weight.

Per measurement/CM:

~~~text
measured water weight
→ water density at the consumed/frozen temperature
→ water volume/capacity

water volume/capacity
+ Volume BQ
- Volume PU
→ apply configured/frozen glass density
→ calculated glass weight
~~~

The canonical formula remains:

~~~text
capacity = water weight / water density

glass weight =
(capacity + volume_marisa - volume_puncao) * glass density
~~~

Therefore the product labels in the PDF are:

- Peso do vidro (g) / Peso médio do vidro (g);
- Volume da água (cm³) / Volume médio da água (cm³).

The backend returns these calculated results. The frontend/PDF renderer must not reproduce the industrial formula independently.

## Historical-control comparison

The normal Peso comparison is against the historical peso_id explicitly chosen by the user.

It is not an automatically inferred last production.

~~~text
current peso_id
→ historical candidate list for same production reference
→ user explicitly selects historical peso_id
→ backend resolves comparison packet
~~~

The PDF identifies this source as Último controlo usado, or equivalent wording that clearly means the selected historical DMO control.

That identification is composed from the selected historical Peso production context, for example:

- production number;
- production date;
- machine/line.

The comparison must never silently change to another historical Peso because a newer production exists.

## Comparison by CM

The detailed table uses the valid CM/measurement counterpart from the historical-difference contract.

Do not prefix the visible CM identifier with artificial row labels such as Leitura 1, Leitura 2, etc.

Preferred display:

~~~text
CM 61
CM 95
CM 63
CM 36
~~~

For each participating CM, the backend supplies:

- current calculated glass weight;
- historical calculated glass weight;
- signed difference;
- current water volume/capacity;
- historical water volume/capacity;
- signed difference.

Only valid counterparts participate. Unequal measurement counts remain valid under the Peso historical-difference rules.

## Technical/reference section

This section is distinct from the selected historical Peso comparison.

~~~text
Último controlo usado
→ selected historical DMO Peso record

Peso médio SAP da produção anterior
→ SAP/reference value for the previous production
~~~

These are not the same source and must not be joined or substituted for one another.

| PDF field | Source rule |
| --- | --- |
| Peso nominal do desenho | CM Tool-owned `peso_nominal` technical value originating from the applicable drawing/reference |
| Diferença para novo | backend-derived difference using the applicable drawing/reference weight |
| Variação | backend-derived percentage for that reference comparison |
| Peso médio SAP da produção anterior | SAP/reference data for the previous production |
| Período SAP | period attached to that SAP/reference dataset |
| Temperatura da água | temperature consumed/frozen by the current Peso calculation |
| Densidade da água | canonical density value consumed/frozen for that calculation |

Drawing/SAP/reference values must be supplied through an owning backend read contract. The renderer must not scrape them from display text or calculate them from unrelated fields.

Where the exact external adapter is implementation-specific, that adapter may vary without changing the PDF domain meaning.

## Traceability

Traceability comes from persisted backend attribution/history, never from the user generating the PDF.

The packet may show:

- verified/submitted-by actor for the Peso workflow where that attribution exists;
- approval actor;
- approval timestamp;
- document revision metadata.

Generating the PDF must not replace those actors with the currently logged-in generator.

## Backend read model

The PDF read is purpose-specific.

~~~text
READ Peso PDF by peso_id
→ validate peso_id and approved state
→ resolve current Job On / Controlo / CM context
→ resolve CM Tool facts
→ resolve the BQ production context/Tool required for `volume_marisa`
→ resolve only required Tool technical values from their actual owner Tools
→ resolve frozen Peso calculation inputs/results
→ resolve explicitly selected historical peso_id and comparison results
→ resolve drawing/SAP reference data required by the document
→ resolve approval/audit facts
→ return one compact PesoPdfReadModel
~~~

The backend may join several truthful owners to construct this response.

~~~text
READ MODEL != ENTITY
QUERY JOIN != DOMAIN OWNERSHIP
~~~

The read model must not cause duplicated write-model ownership.

## Frontend/document-renderer contract

The frontend/document layer receives one prepared read model and is responsible for:

- layout;
- labels;
- number/unit formatting;
- pagination/printing;
- explicit generation/send actions.

It must not:

- derive Tool identity from reference/lot text;
- refetch arbitrary tables to reconstruct the packet;
- calculate glass weight or water volume as a second formula source;
- decide which historical Peso was intended;
- prepend L to the displayed lot;
- reinterpret SAP previous-production values as the selected historical DMO control;
- synthesize approval actor/time.

## Regeneration after reopen/correction

The same peso_id survives reopen, correction, resubmission and later approval.

After a corrected Peso is approved again, its production PDF may be generated again from the newly approved current state.

~~~text
same peso_id
→ reopen
→ correction
→ submit
→ approval
→ explicit PDF regeneration/replacement
~~~

This does not create a second Peso identity and does not make the old PDF the source of truth.

Replacement must be explicit; silent overwrite remains forbidden.

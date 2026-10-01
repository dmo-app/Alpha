/* Boquilhas frontend boundary — temporary mock/scaffolding.
 *
 * This file is implementation scaffolding, NOT product authority.
 * It replaces page-owned Boquilhas domain logic with a narrow boundary that
 * pages consume as if it were a real backend. Keep it small and replaceable.
 */
(function () {
  'use strict';

  let nextTraceId = 1002;
  let nextMovementId = 1003;

  // Demo production contexts that may be associated with BQ traces.
  const demoProductions = [
    { jobon_id: 'JO-202601-B3', bq_id: 'BQ-CTX-202601-B3', reference: '9389T194', production: '202601', machine: 'B3', tool_id: '2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d6e' },
    { jobon_id: 'JO-202512-B3', bq_id: 'BQ-CTX-202512-B3', reference: '9389T194', production: '202512', machine: 'B3', tool_id: '2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d6e' }
  ];

  // Demo repair traces and movements.
  const traces = [
    {
      trace_id: 'BQT-1001',
      tool_id: '4e5f6a7b-8c9d-4e0f-8a1b-2c3d4e5f6a7b',
      bq_id: 'BQ-CTX-202601-B3',
      jobon_id: 'JO-202601-B3',
      created_at: '2026-08-10T08:00:00Z',
      movements: [
        { movement_id: 'BQM-1001', type: 'Saída', quantity: 5, date: '2026-08-10', reason: '', discrepancy: null, recorded_at: '2026-08-10T08:05:00Z' },
        { movement_id: 'BQM-1002', type: 'Entrada', quantity: 7, date: '2026-08-12', reason: '', discrepancy: -2, recorded_at: '2026-08-12T10:00:00Z' }
      ]
    }
  ];

  function refusal(reason) {
    return { ok: false, reason };
  }

  function ok(data) {
    return { ok: true, data };
  }

  function findTrace(traceId) {
    return traces.find(t => t.trace_id === traceId);
  }

  function resolveTool(toolId) {
    if (!window.toolBoundary) return null;
    const res = window.toolBoundary.get(toolId);
    if (res.ok) return res.data;
    return null;
  }

  function getTraceReadModel(trace) {
    const tool = resolveTool(trace.tool_id);
    const production = demoProductions.find(p => p.bq_id === trace.bq_id) || null;
    return {
      trace_id: trace.trace_id,
      tool_id: trace.tool_id,
      tool: tool ? { tool_id: tool.tool_id, type: tool.type, reference: tool.reference, lot: tool.lot } : { tool_id: trace.tool_id, reference: '—', lot: '—' },
      bq_id: trace.bq_id,
      jobon_id: trace.jobon_id,
      production: production,
      created_at: trace.created_at,
      movements: trace.movements.map(m => ({
        movement_id: m.movement_id,
        type: m.type,
        quantity: m.quantity,
        date: m.date,
        reason: m.reason,
        discrepancy: m.discrepancy
      }))
    };
  }

  function calculateMovementDiscrepancy(movements, newMovement) {
    // Compute outstanding quantity before the new movement using the same trace history.
    let outstanding = 0;
    for (const m of movements) {
      if (m.type === 'Saída') {
        outstanding += m.quantity;
      } else if (m.type === 'Entrada' || m.type === 'EntradaSemReparação') {
        const matched = Math.min(outstanding, m.quantity);
        outstanding -= matched;
      }
    }

    if (newMovement.type === 'Saída') {
      return { discrepancy: null, outstandingAfter: outstanding + newMovement.quantity };
    }
    if (newMovement.type === 'Entrada' || newMovement.type === 'EntradaSemReparação') {
      const discrepancy = outstanding - newMovement.quantity; // negative = excess returned
      return { discrepancy, outstandingAfter: Math.max(0, outstanding - newMovement.quantity) };
    }
    return { discrepancy: null, outstandingAfter: outstanding };
  }

  function listTraces(filters) {
    let pool = traces.slice();
    if (filters) {
      if (filters.tool_id) pool = pool.filter(t => t.tool_id === filters.tool_id);
      if (filters.bq_id) pool = pool.filter(t => t.bq_id === filters.bq_id);
      if (filters.jobon_id) pool = pool.filter(t => t.jobon_id === filters.jobon_id);
      if (filters.has_pending === true) pool = pool.filter(t => !t.bq_id);
    }
    return ok(pool.map(getTraceReadModel));
  }

  function getTrace(traceId) {
    const trace = findTrace(traceId);
    if (!trace) return refusal('Registo de Boquilhas não encontrado.');
    return ok(getTraceReadModel(trace));
  }

  function createTrace({ tool_id, bq_id, jobon_id }) {
    if (!tool_id) return refusal('tool_id é obrigatório para criar um registo de Boquilhas.');
    const tool = resolveTool(tool_id);
    if (!tool) return refusal('BQ (ferramenta) não encontrada no catálogo.');

    const production = bq_id ? demoProductions.find(p => p.bq_id === bq_id) || null : null;

    const trace = {
      trace_id: `BQT-${nextTraceId++}`,
      tool_id: tool.tool_id,
      bq_id: bq_id || null,
      jobon_id: jobon_id || (production ? production.jobon_id : null),
      created_at: new Date().toISOString(),
      movements: []
    };
    traces.push(trace);
    return ok(getTraceReadModel(trace));
  }

  function recordMovement(traceId, { type, quantity, date, reason }) {
    const trace = findTrace(traceId);
    if (!trace) return refusal('Registo de Boquilhas não encontrado.');
    const validTypes = ['Saída', 'Entrada', 'EntradaSemReparação'];
    if (!validTypes.includes(type)) return refusal('Tipo de movimento inválido.');
    const qty = Number(quantity);
    if (!Number.isFinite(qty) || qty <= 0 || !Number.isInteger(qty)) {
      return refusal('Quantidade deve ser um número inteiro positivo.');
    }

    const { discrepancy } = calculateMovementDiscrepancy(trace.movements, { type, quantity: qty });
    const movement = {
      movement_id: `BQM-${nextMovementId++}`,
      type,
      quantity: qty,
      date: date || new Date().toISOString().slice(0, 10),
      reason: reason || '',
      discrepancy: discrepancy,
      recorded_at: new Date().toISOString()
    };
    trace.movements.push(movement);
    return ok({
      trace_id: trace.trace_id,
      movement_id: movement.movement_id,
      type: movement.type,
      quantity: movement.quantity,
      discrepancy: movement.discrepancy
    });
  }

  function associateTrace(traceId, { bq_id, jobon_id }) {
    const trace = findTrace(traceId);
    if (!trace) return refusal('Registo de Boquilhas não encontrado.');
    if (!bq_id) return refusal('bq_id é obrigatório para associar o registo a uma produção.');
    if (trace.bq_id) return refusal('Registo já associado a uma produção. Reassociação não implementada nesta slice.');
    const production = demoProductions.find(p => p.bq_id === bq_id);
    if (!production) return refusal('Produção (bq_id) não encontrada.');
    if (production.tool_id !== trace.tool_id) {
      return refusal('A produção selecionada refere uma BQ diferente. A associação não é permitida.');
    }
    trace.bq_id = bq_id;
    trace.jobon_id = jobon_id || production.jobon_id;
    return ok(getTraceReadModel(trace));
  }

  function listProductions(filters) {
    let pool = demoProductions.slice();
    if (filters) {
      if (filters.tool_id) pool = pool.filter(p => p.tool_id === filters.tool_id);
      if (filters.machine) pool = pool.filter(p => p.machine === filters.machine);
    }
    return ok(pool);
  }

  const REGISTER_STATE = {
    production: { label: 'EM PRODUÇÃO', pill: 'ready', field: 'Linha' },
    repair: { label: 'EM REPARAÇÃO', pill: 'repair', field: 'Reparador' },
    available: { label: 'DISPONÍVEL', pill: 'ready', field: 'Localização' }
  };

  // Demo Boquilhas register ("Ficheiros de Boquilhas"). Each register entry is mock fixture
  // data that references a canonical BQ tool_id owned by the Tool registry (tool-boundary.js).
  // This file does not mint or derive tool_id; it resolves the canonical identity from the
  // registry and returns it in the read model, exactly as the real backend read model would.
  const registers = [
    { tool_id: 'a1b2c3d4-5e6f-4a7b-8c9d-0e1f2a3b4c5d', state: 'production', quantity: 288, fieldValue: 'B1', vida: '68%' },
    { tool_id: 'b2c3d4e5-6f7a-4b8c-9d0e-1f2a3b4c5d6e', state: 'repair', quantity: 96, fieldValue: 'Externo A', vida: '81%' },
    { tool_id: 'c3d4e5f6-7a8b-4c9d-9e0f-1a2b3c4d5e6f', state: 'production', quantity: 144, fieldValue: 'B2', vida: '42%' },
    { tool_id: 'd4e5f6a7-8b9c-4d0e-9f1a-2b3c4d5e6f7a', state: 'available', quantity: 192, fieldValue: 'Fábrica', vida: '19%' }
  ];

  function listRegisters() {
    return ok(registers.map(r => {
      const tool = resolveTool(r.tool_id);
      const meta = REGISTER_STATE[r.state] || { label: '', pill: 'ready', field: '' };
      return {
        tool_id: r.tool_id,
        reference: tool ? tool.reference : '—',
        lot: tool ? tool.lot : '—',
        state: r.state,
        statusLabel: meta.label,
        pill: meta.pill,
        quantity: r.quantity,
        unit: 'BQ',
        fieldLabel: meta.field,
        fieldValue: r.fieldValue,
        vida: r.vida
      };
    }));
  }

  window.boquilhasBoundary = {
    listTraces,
    getTrace,
    createTrace,
    recordMovement,
    associateTrace,
    listProductions,
    listRegisters
  };
})();

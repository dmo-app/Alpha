/* Tool (Ferramentas) boundary — temporary MOCK backend / deterministic fixture source.
 *
 * This file is implementation scaffolding, NOT product authority. It is the single
 * deterministic fixture source for the Tool catalog. Its `tool_id` values are authored
 * mock fixture data that SIMULATE the production contract (where the backend issues a
 * UUID per Tool). They are never minted or derived by page/UI logic.
 *
 * It replaces page-owned Tool domain logic with a narrow boundary that pages consume as
 * if it were a real backend. Keep it small and replaceable.
 */
(function () {
  'use strict';

  function newId() {
    return crypto.randomUUID?.() || ('tool-' + Date.now().toString(36) + '-' + Math.random().toString(36).slice(2));
  }

  // Demo seed: canonical Tool registry.
  // Different lot = different Tool = different tool_id (identity invariant).
  const tools = [
    {
      tool_id: '3d9a0f2b-4c51-4e6a-8b7d-1c2e3f4a5b6c',
      type: 'CM',
      reference: '9389',
      lot: '4',
      quantity: 16,
      process: 'NNPB',
      machines: ['B3'],
      state: 'Ativa',
      mf_references: [],
      created_at: '2026-01-10T09:00:00Z'
    },
    {
      tool_id: '7f1e2d3c-8b5a-4c6d-8e9f-0a1b2c3d4e5f',
      type: 'CM',
      reference: '9389',
      lot: '3',
      quantity: 16,
      process: 'NNPB',
      machines: ['B3'],
      state: 'Ativa',
      mf_references: [],
      created_at: '2025-11-02T09:00:00Z'
    },
    {
      tool_id: '5a8b7c6d-9e0f-4a1b-8c2d-3e4f5a6b7c8d',
      type: 'CM',
      reference: '5809',
      lot: '1',
      quantity: 16,
      process: 'NNPB',
      machines: ['B1', 'B3'],
      state: 'Ativa',
      mf_references: ['5810'],
      created_at: '2026-02-15T10:00:00Z'
    },
    {
      tool_id: '9c8d7e6f-5a4b-4c3d-8e2f-1a0b9c8d7e6f',
      type: 'MF',
      reference: '5447',
      lot: '8',
      quantity: 16,
      process: 'NNPB',
      machines: ['B3'],
      state: 'Ativa',
      mf_references: [],
      created_at: '2026-03-01T11:00:00Z'
    },
    {
      tool_id: '2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d6e',
      type: 'BQ',
      reference: 'T194',
      lot: '4',
      quantity: 144,
      process: 'NNPB',
      machines: ['B3'],
      state: 'Ativa',
      mf_references: [],
      created_at: '2026-01-20T08:00:00Z'
    },
    {
      tool_id: '4e5f6a7b-8c9d-4e0f-8a1b-2c3d4e5f6a7b',
      type: 'BQ',
      reference: 'T173',
      lot: '4',
      quantity: 192,
      process: 'PS',
      machines: ['B3'],
      state: 'Ativa',
      mf_references: [],
      created_at: '2026-04-05T14:00:00Z'
    },
    // Demo-fixture BQ tools referenced by the Boquilhas register surface ("Ficheiros de
    // Boquilhas"). The second token after the reference is the register's displayed
    // distinguishing value; it is used here as the mock "lot". In production the lot and
    // other Tool master facts are returned by the real Ferramentas backend read model.
    {
      tool_id: 'a1b2c3d4-5e6f-4a7b-8c9d-0e1f2a3b4c5d',
      type: 'BQ',
      reference: 'T173',
      lot: '24/33',
      quantity: 288,
      process: null,
      machines: [],
      state: 'Ativa',
      mf_references: [],
      created_at: '2026-05-01T09:00:00Z'
    },
    {
      tool_id: 'b2c3d4e5-6f7a-4b8c-9d0e-1f2a3b4c5d6e',
      type: 'BQ',
      reference: 'P446',
      lot: '24/29',
      quantity: 96,
      process: null,
      machines: [],
      state: 'Ativa',
      mf_references: [],
      created_at: '2026-05-02T09:00:00Z'
    },
    {
      tool_id: 'c3d4e5f6-7a8b-4c9d-9e0f-1a2b3c4d5e6f',
      type: 'BQ',
      reference: 'V902',
      lot: '25/08',
      quantity: 144,
      process: null,
      machines: [],
      state: 'Ativa',
      mf_references: [],
      created_at: '2026-05-03T09:00:00Z'
    },
    {
      tool_id: 'd4e5f6a7-8b9c-4d0e-9f1a-2b3c4d5e6f7a',
      type: 'BQ',
      reference: 'M310',
      lot: '25/11',
      quantity: 192,
      process: null,
      machines: [],
      state: 'Ativa',
      mf_references: [],
      created_at: '2026-05-04T09:00:00Z'
    }
  ];

  function refusal(reason) {
    return { ok: false, reason };
  }

  function ok(data) {
    return { ok: true, data };
  }

  function matchesFilter(tool, filters) {
    if (filters.type && tool.type !== filters.type) return false;
    if (filters.reference) {
      const term = String(filters.reference).toLowerCase().trim();
      if (!tool.reference.toLowerCase().includes(term)) return false;
    }
    if (filters.lot) {
      const term = String(filters.lot).toLowerCase().trim();
      if (String(tool.lot).toLowerCase() !== term) return false;
    }
    if (filters.process && tool.process !== filters.process) return false;
    if (filters.state && tool.state !== filters.state) return false;
    if (filters.machine) {
      const machine = String(filters.machine).trim().toUpperCase();
      if (!tool.machines || !tool.machines.some(m => m.toUpperCase() === machine)) return false;
    }
    if (filters.mf_reference) {
      const term = String(filters.mf_reference).toLowerCase().trim();
      if (!tool.mf_references || !tool.mf_references.some(r => r.toLowerCase().includes(term))) return false;
    }
    return true;
  }

  function normalizeMachines(machines) {
    if (!machines) return [];
    if (Array.isArray(machines)) {
      return machines.map(m => String(m).trim().toUpperCase()).filter(Boolean);
    }
    return String(machines).split(/[,;]/).map(m => m.trim().toUpperCase()).filter(Boolean);
  }

  function listTools(filters) {
    const pool = filters ? tools.filter(t => matchesFilter(t, filters)) : tools.slice();
    return ok(pool.map(t => ({
      tool_id: t.tool_id,
      type: t.type,
      reference: t.reference,
      lot: t.lot,
      quantity: t.quantity,
      process: t.process,
      machines: t.machines,
      state: t.state,
      mf_references: t.mf_references || []
    })));
  }

  function getTool(toolId) {
    const tool = tools.find(t => t.tool_id === toolId);
    if (!tool) return refusal('Ferramenta não encontrada.');
    return ok({
      tool_id: tool.tool_id,
      type: tool.type,
      reference: tool.reference,
      lot: tool.lot,
      quantity: tool.quantity,
      process: tool.process,
      machines: tool.machines,
      state: tool.state,
      mf_references: tool.mf_references || [],
      created_at: tool.created_at
    });
  }

  function createTool(input) {
    const type = String(input.type || '').trim().toUpperCase();
    const reference = String(input.reference || '').trim();
    const lot = String(input.lot || '').trim();

    if (!type || !['CM', 'MF', 'BQ'].includes(type)) {
      return refusal('Tipo de ferramenta inválido. Use CM, MF ou BQ.');
    }
    if (!reference) return refusal('Referência da ferramenta é obrigatória.');
    if (!lot) return refusal('Lote é obrigatório.');

    const process = input.process === 'NNPB' || input.process === 'PS' ? input.process : null;
    const state = String(input.state || '').trim() || null;
    const machines = normalizeMachines(input.machines);
    const mf_references = type === 'CM' && Array.isArray(input.mf_references)
      ? input.mf_references.map(r => String(r).trim()).filter(Boolean)
      : [];

    let quantity = 1;
    const rawQuantity = input.quantity === undefined || input.quantity === null || String(input.quantity).trim() === '' ? '' : String(input.quantity).trim();
    if (rawQuantity !== '') {
      quantity = Number(rawQuantity);
      if (!Number.isFinite(quantity) || quantity <= 0 || !Number.isInteger(quantity)) {
        return refusal('Quantidade deve ser um número inteiro positivo.');
      }
    }

    const tool = {
      tool_id: newId(),
      type,
      reference,
      lot,
      quantity,
      process,
      machines,
      state,
      mf_references,
      created_at: new Date().toISOString()
    };
    tools.push(tool);
    return ok({
      tool_id: tool.tool_id,
      type: tool.type,
      reference: tool.reference,
      lot: tool.lot,
      quantity: tool.quantity,
      process: tool.process,
      machines: tool.machines,
      state: tool.state,
      mf_references: tool.mf_references
    });
  }

  window.toolBoundary = {
    list: listTools,
    get: getTool,
    create: createTool,
    // Canonical boundary names expected by Recovery-facing pages.
    searchTools: listTools,
    getTool: getTool,
    createTool: createTool
  };
})();

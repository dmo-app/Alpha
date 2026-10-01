/* Boquilhas repair trace flow.
 * All canonical IDs, trace/movement persistence and discrepancy calculation
 * are delegated to boquilhasBoundary. This script owns only UI actions.
 */
(() => {
  if(document.body.dataset.betaPage!=='boquilhas')return;
  const $=selector=>document.querySelector(selector);

  // Job On cross-page context is read as a prototype bridge; Boquilhas boundary owns traces/movements.
  const jobons=()=>{try{return JSON.parse(sessionStorage.getItem('betaJobOnSummaries')||'[]')}catch{return []}};
  const events=()=>{try{return JSON.parse(sessionStorage.getItem('betaBqProductionChanges')||'[]')}catch{return []}};

  function getBqCatalog(){
    const boundaryTools=(window.toolBoundary?.list({type:'BQ'}).data)||[];
    const catalog=boundaryTools.map(t=>({tool_id:t.tool_id,type:t.type,reference:t.reference,lot:t.lot}));
    // Also include BQ tools referenced by saved Job On summaries so contextual selection works in the prototype.
    for(const jobon of jobons()){
      const tool=jobon.contexts?.BQ?.tool;
      if(tool?.tool_id && !catalog.some(item=>item.tool_id===tool.tool_id)) catalog.push({tool_id:tool.tool_id,type:'BQ',reference:tool.reference,lot:tool.lot});
      if(tool?.toolId && !catalog.some(item=>item.tool_id===tool.toolId)) catalog.push({tool_id:tool.toolId,type:'BQ',reference:tool.reference,lot:tool.lot});
    }
    return catalog;
  }

  let selectedTool=null,activeTrace=null;
  $('#movementForm input[type="date"]').value=new Date().toISOString().slice(0,10);
  $('#recordResults').style.display='none';
  $('#recordEmpty').textContent='Pesquise uma BQ, abra um registo existente ou inicie um novo registo.';
  $('#recordDetail').querySelectorAll(':scope > .card').forEach(card=>card.hidden=true);
  $('#recordDetail .record-bar #closeLot')?.remove();
  $('#recordDetail .record-bar .btn:not([data-movement]):not(#closeLot)')?.remove();

  const register=$('#registo');
  const panel=document.createElement('section');panel.className='beta-bq-workflow card';panel.hidden=true;
  panel.innerHTML='<h3>Criar registo de Boquilhas</h3><p>Selecione a BQ exata. Se existir uma produção correspondente, pode selecioná-la agora ou continuar com a associação pendente.</p><div class="beta-bq-matches" id="bqToolMatches"></div><div class="beta-bq-association"><label for="bqJobonCandidate">Produção associada</label><select id="bqJobonCandidate"><option value="">Sem produção associada</option></select><small id="bqAssociationHelp">Selecione primeiro uma boquilha.</small></div><div class="beta-bq-actions"><button type="button" class="btn" id="cancelBqTrace">Cancelar</button><button type="button" class="btn primary" id="saveBqTrace" disabled>Iniciar registo</button></div>';
  register.querySelector('.register-search').closest('.card').insertAdjacentElement('afterend',panel);
  const traceList=document.createElement('section');traceList.className='beta-bq-workflow card';traceList.innerHTML='<h3>Registos de Boquilhas</h3><div id="bqTraceList"></div>';
  panel.insertAdjacentElement('afterend',traceList);
  const context=document.createElement('div');context.className='beta-bq-trace-context';context.hidden=true;$('#recordDetail').prepend(context);
  const search=$('#recordSearch');

  function candidates(tool){
    if(!tool) return [];
    const bqProductions=(window.boquilhasBoundary?.listProductions({tool_id:tool.tool_id}).data)||[];
    return bqProductions.map(p=>({id:p.bq_id,jobon:p.jobon_id,reference:p.reference,production:p.production,line:p.machine}));
  }
  function drawCandidates(){
    const select=$('#bqJobonCandidate');select.replaceChildren(new Option('Sem produção associada',''));
    if(!selectedTool) return;
    for(const item of candidates(selectedTool)){
      const option=new Option(`${item.reference} · ${item.production} · ${item.line}`,item.id);
      option.dataset.jobon=item.jobon;select.add(option);
    }
    $('#bqAssociationHelp').textContent=select.length>1?'Escolha explicitamente a produção correta. A mais recente não é atribuída automaticamente.':'Produção por associar — pode iniciar os movimentos e associar mais tarde.';
  }
  function drawTools(){
    const root=$('#bqToolMatches');root.replaceChildren();
    const q=search.value.trim().toLowerCase();
    const matching=getBqCatalog().filter(tool=>!q||`${tool.reference} ${tool.lot}`.toLowerCase().includes(q));
    for(const tool of matching){
      const button=document.createElement('button');button.type='button';
      button.textContent=`BQ ${tool.reference} · Lote ${tool.lot}`;
      button.className=selectedTool?.tool_id===tool.tool_id?'active':'';
      button.onclick=()=>{selectedTool=tool;drawTools();drawCandidates();$('#saveBqTrace').disabled=false};
      root.append(button);
    }
    if(!matching.length){const message=document.createElement('p');message.textContent='Nenhuma BQ registada com estes dados. A pesquisa não cria ferramentas.';root.append(message)}
  }
  function closePanel(){panel.hidden=true;selectedTool=null;$('#saveBqTrace').disabled=true}
  function begin(){panel.hidden=false;drawTools();drawCandidates();panel.scrollIntoView({block:'nearest'})}

  $('#newLot').textContent='Criar registo';$('#newLot').onclick=begin;
  $('#newLotBatches').textContent='Criar registo';$('#newLotBatches').onclick=()=>{$('[data-view="registo"]').click();begin()};
  $('#cancelBqTrace').onclick=closePanel;

  function productionLabel(trace){
    const p=trace.production;
    if(!p) return 'Produção por associar — o registo e os movimentos continuam disponíveis.';
    return `${p.reference} · Produção ${p.production} · ${p.machine}`;
  }

  function showTrace(trace){
    activeTrace=trace;
    search.value=trace.tool.reference;
    $('#recordEmpty').classList.add('hidden');
    $('#recordDetail').classList.remove('hidden');
    context.hidden=false;
    context.replaceChildren();
    const title=document.createElement('strong');title.textContent=`BQ ${trace.tool.reference} · Lote ${trace.tool.lot}`;
    const info=document.createElement('span');info.textContent=productionLabel(trace);
    context.append(title,info);
    const movementCount=document.createElement('small');movementCount.textContent=`${trace.movements.length} movimento(s) neste registo`;context.append(movementCount);
    $('.movement-subtitle').textContent=`BQ ${trace.tool.reference} · Lote ${trace.tool.lot}`;
    const modal=$('#movementModal .context');
    const facts=modal?.querySelectorAll('strong');
    if(facts?.length>=3){facts[0].textContent=trace.tool.reference;facts[1].textContent=trace.tool.lot;facts[2].textContent=trace.production?.machine||'Por associar'}
    const summary=$('#recordDetail .summary-line');
    if(summary)summary.textContent=`${title.textContent} · ${info.textContent}`;
    $('#recordDetail').scrollIntoView({block:'nearest'});
  }

  function drawTraces(){
    const root=$('#bqTraceList');root.replaceChildren();
    const q=search.value.trim().toLowerCase();
    const res=window.boquilhasBoundary?.listTraces();
    if(!res || !res.ok){const p=document.createElement('p');p.textContent=res?.reason||'Erro ao carregar registos.';root.append(p);return;}
    const matching=res.data.filter(trace=>!q||`${trace.tool.reference} ${trace.tool.lot}`.toLowerCase().includes(q));
    for(const trace of matching){
      const line=document.createElement('div');line.className='beta-bq-trace-row';
      const label=document.createElement('span');
      label.textContent=`BQ ${trace.tool.reference} · Lote ${trace.tool.lot} · ${trace.production?`${trace.production.production} · ${trace.production.machine}`:'Produção por associar'}`;
      const open=document.createElement('button');open.type='button';open.className='btn';open.textContent='Abrir registo';
      open.onclick=()=>showTrace(trace);
      line.append(label,open);
      if(!trace.bq_id){
        const matches=candidates(trace.tool);
        if(matches.length){
          const select=document.createElement('select');select.setAttribute('aria-label',`Associar produção a ${trace.tool.reference}`);select.add(new Option('Escolher produção',''));
          for(const item of matches)select.add(new Option(`${item.reference} · ${item.production} · ${item.line}`,item.id));
          const attach=document.createElement('button');attach.type='button';attach.className='btn';attach.textContent='Associar';
          attach.onclick=()=>{
            if(!select.value)return;
            const assocRes=window.boquilhasBoundary.associateTrace(trace.trace_id,{bq_id:select.value});
            if(!assocRes.ok){alert(assocRes.reason);return;}
            drawTraces();
            if(activeTrace?.trace_id===trace.trace_id)showTrace(assocRes.data);
          };
          line.append(select,attach);
        }
      }
      root.append(line);
    }
    if(!matching.length){const empty=document.createElement('p');empty.textContent='Nenhum registo encontrado. Pode iniciar um registo pela BQ mesmo sem Job On.';root.append(empty)}
  }

  function drawHistory(){
    const root=$('#movements');root.querySelectorAll('.movement[data-type]').forEach(row=>row.remove());
    const res=window.boquilhasBoundary?.listTraces();
    if(!res || !res.ok)return;
    for(const trace of res.data){
      for(const movement of trace.movements){
        const type=movement.type==='Saída'?'out':movement.type==='Entrada'?'in':'failed';
        const qty=Number(movement.quantity);
        const discrepancy=movement.discrepancy;
        const row=document.createElement('button');row.type='button';row.className='movement wide';
        row.dataset.type=type;row.dataset.text=`${trace.tool.reference} ${trace.tool.lot}`.toLowerCase();
        row.dataset.repairer='';row.dataset.state='current';row.dataset.date=movement.date||'';
        const saldo=(discrepancy===null||discrepancy===0)?'—':String(discrepancy).replace('-','−');
        const values=[trace.tool.reference,trace.tool.lot,movement.type,qty,saldo,'',trace.production?.machine||'—',movement.date||'—',window.betaDemoSession?.get()?.name||'Operador'];
        values.forEach((value,index)=>{const cell=document.createElement(index===0?'strong':'span');cell.textContent=value;if(index===2)cell.className=`type ${type}`;row.append(cell)});
        row.onclick=()=>{root.querySelectorAll('.movement').forEach(item=>item.classList.toggle('selected',item===row));};
        row.ondblclick=()=>{$('[data-view="registo"]').click();showTrace(trace)};
        root.append(row);
      }
    }
    window.betaBqHistoryRefresh?.();
  }

  $('#saveBqTrace').onclick=()=>{
    if(!selectedTool)return;
    const option=$('#bqJobonCandidate').selectedOptions[0];
    const res=window.boquilhasBoundary.createTrace({tool_id:selectedTool.tool_id,bq_id:option?.value||null});
    if(!res.ok){alert(res.reason);return;}
    closePanel();drawTraces();showTrace(res.data);
  };

  $('#movementForm').addEventListener('submit',(e)=>{
    e.preventDefault();
    if(!activeTrace)return;
    const form=$('#movementForm');
    if(!form.reportValidity())return;
    const type=$('#movementTitle').textContent;
    const quantity=Number(form.querySelector('input[type="number"]').value);
    const date=form.querySelector('input[type="date"]').value;
    const reason=$('#movementReason').value;
    const res=window.boquilhasBoundary.recordMovement(activeTrace.trace_id,{type,quantity,date,reason});
    if(!res.ok){alert(res.reason);return;}
    const traceRes=window.boquilhasBoundary.getTrace(activeTrace.trace_id);
    if(traceRes.ok)activeTrace=traceRes.data;
    drawHistory();showTrace(activeTrace);
  },true);

  search.addEventListener('input',()=>{if(!panel.hidden){selectedTool=null;$('#saveBqTrace').disabled=true;drawTools();drawCandidates()}drawTraces()});

  const notice=document.createElement('p');notice.className='beta-bq-notice';
  const latest=events()[0];
  notice.textContent=latest?`Job On atualizado: ${latest.reference} · ${latest.production} · ${latest.line}. A produção fica disponível para associar ao registo da BQ.`:'Selecione a BQ para iniciar o registo; as produções disponíveis surgem no registo quando existirem.';
  traceList.prepend(notice);

  // Movement edit/delete remain unsupported per slice scope.
  const editBtn=$('#editMovement');const deleteBtn=$('#deleteMovement');
  if(editBtn)editBtn.disabled=true;
  if(deleteBtn)deleteBtn.disabled=true;

  drawTraces();drawHistory();
})();

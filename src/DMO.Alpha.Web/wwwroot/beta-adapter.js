(function(){
  const page=document.body.dataset.betaPage||'';
  const file=n=>n;
  const normalNav=[
    ['20_JOB_ON_01_VISUAL_AUTHORITY_job-on.html','Job On','jobon'],
    ['22_PESO_OPERADOR_01_VISUAL_AUTHORITY_peso-operador.html','Controlo Criar','controlo-create'],
    ['31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html?view=registo','Boquilhas','boquilhas']
  ];
  const supervisorNav=[
    ['20_JOB_ON_01_VISUAL_AUTHORITY_job-on.html','Job On','jobon'],
    ['23_PESO_RESPONSAVEL_01_VISUAL_AUTHORITY_peso-responsavel.html','Controlo Aprovar','controlo-approve'],
    ['31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html?view=registo','Boquilhas','boquilhas']
  ];
  function applyNav(){
    if(document.body.dataset.razorShell==='true')return;
    const nav=document.querySelector('.dmo-primary-nav,.global-nav');
    if(!nav)return;
    // Retain the legacy shell styling while showing only Beta destinations.
    nav.querySelectorAll('a').forEach(a=>{
      const href=a.getAttribute('href')||'';
      if(/(?:32_ARMAZEM|34_REPARACAO)/.test(href))a.remove();
    });
  }
  applyNav();
  const header=document.querySelector('.dmo-app-header');
  const primary=document.querySelector('.dmo-primary-nav');
  if(page==='boquilhas'&&header&&primary&&header.contains(primary))header.insertAdjacentElement('afterend',primary);
  if(primary&&document.body.dataset.razorShell!=='true'){
    primary.querySelectorAll('a').forEach(link=>{if((link.getAttribute('href')||'').includes('JOB_ON'))link.textContent='Planeamento'});
    const order=['jobon','controlo-criar','controlo-aprovar','boquilhas'];
    const position=href=>href.includes('JOB_ON')?'jobon':href.includes('CONTROLO')?'controlo-criar':href.includes('BOQUILHAS')?'boquilhas':'admin';
    [...primary.querySelectorAll('a')].sort((a,b)=>{const rank=x=>{const id=position(x.getAttribute('href')||'');return order.indexOf(id)};return (rank(a)<0?99:rank(a))-(rank(b)<0?99:rank(b))}).forEach(link=>primary.append(link));
  }
  if(page==='login'){
    const id=document.querySelector('#email');
    if(id){
      id.type='text';
      id.placeholder='Email ou ID de operador (até 4 dígitos)';
      id.previousElementSibling.textContent='Utilizador';
      id.autocomplete='username';
      const isOperatorId=v=>/^[0-9]{1,4}$/.test((v||'').trim());
      const setMode=()=>{
        if(isOperatorId(id.value||'')){id.setAttribute('maxlength','4');id.setAttribute('pattern','[0-9]{1,4}');id.inputMode='numeric';}
        else{id.removeAttribute('maxlength');id.removeAttribute('pattern');id.inputMode='email';}
      };
      id.addEventListener('input',setMode);
      setMode();
    }
    const form=document.querySelector('#loginForm');
    form?.insertAdjacentHTML('beforeend','<div class="beta-demo-accounts"><strong>Contas para testar</strong><div class="beta-demo-account-list"></div><small>Palavra-passe das contas de demonstração: <code>Demo123</code>. Os dados desta demonstração ficam apenas na sessão do navegador.</small></div>');
    const examples=form?.querySelector('.beta-demo-account-list');
    window.betaDemoSession?.accounts.forEach(account=>{
      const button=document.createElement('button');
      button.type='button';
      button.className='beta-demo-account';
      const isAdmin=account.role==='admin';
      button.innerHTML=`<strong>${account.title}</strong><span>${isAdmin?'Email: admin@baglass.com · ':'Nº '+account.number+' · '}${account.template||'—'}</span>`;
      button.onclick=()=>{id.value=isAdmin?'admin@baglass.com':account.number;document.querySelector('#password').value='Demo123';id.focus()};
      examples.append(button)
    });
    if(form)form.addEventListener('submit',e=>{
      e.preventDefault();
      e.stopImmediatePropagation();
      const value=(id?.value||'').trim();
      const password=document.querySelector('#password').value;
      const message=document.querySelector('#formMessage');
      const looksLikeEmail=v=>/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test((v||'').trim());
      if(looksLikeEmail(value)){
        if(value.toLowerCase()!=='admin@baglass.com'||password!=='Demo123'){
          message.textContent='Email ou palavra-passe de administrador inválidos.';
          message.classList.add('show');
          return;
        }
        window.betaDemoSession.set('9000');
        location.href='13_ADMIN_01_VISUAL_AUTHORITY_admin.html';
        return;
      }
      if(!isOperatorId(value)){
        message.textContent='Introduza um email válido ou um ID de operador de 1 a 4 dígitos.';
        message.classList.add('show');
        return;
      }
      const account=window.betaDemoSession?.resolve(value);
      if(!account||password!=='Demo123'){
        message.textContent='Número ou palavra-passe de demonstração inválidos, ou utilizador inativo.';
        message.classList.add('show');
        return;
      }
      window.betaDemoSession.set(account.number);
      location.href=account.home
    },true);
  }
  if(page==='controlo-hub'){
    document.querySelector('#openResumo')?.addEventListener('click',()=>location.href='resumo.html');
    document.querySelectorAll('.beta-control-summary .tool-control-card').forEach(card=>{
      const action=card.querySelector('.text-action');if(action)action.onclick=()=>location.href='resumo.html';
      const menu=card.querySelector('.tool-menu-trigger');if(menu)menu.onclick=()=>location.href='resumo.html';
    });
  }
  if(page==='controlo-create'){
    if(new URLSearchParams(location.search).get('view')==='settings')document.querySelector('.tab[data-view="settings"]')?.click();
  }
  if(page==='controlo-approve'){
    const head=document.querySelector('.page-head p');if(head)head.textContent='Controlos submetidos para decisão do responsável autorizado.';
  }
  if(page==='jobon'){
    const title=document.querySelector('.sheet-reference-title');if(title)title.textContent='5447T137';
    const production=document.querySelector('#sheetProduction');if(production)production.value='202601';
    const machine=document.querySelector('#sheetMachine');if(machine)machine.value='B3';
    const productionSelect=document.querySelector('#productionSelect');
    if(productionSelect&&productionSelect.options[0]){productionSelect.options[0].value='202601';productionSelect.options[0].textContent='202601 · atual';}
    const tools={
      CM:{reference:'ST100',lot:'12',machines:'B1 · B3',process:'NNPB',quantity:'16',usage:'30%'},
      MF:{reference:'5447',lot:'08',machines:'B3',process:'NNPB',quantity:'16',usage:'22%'},
      BQ:{reference:'T173',lot:'04',machines:'B3',process:'—',quantity:'144',usage:'42%'}
    };
    Object.entries(tools).forEach(([type,facts])=>{
      const card=document.querySelector(`.tool-card[data-family="${type}"]`);
      if(!card)return;
      const fields=card.querySelector('.tool-fields');
      if(!fields)return;
      const original=fields.querySelectorAll('label');
      if(original[0]?.querySelector('input')){original[0].querySelector('span').textContent='Referência da ferramenta';original[0].querySelector('input').value=facts.reference;}
      if(original[1]){const select=original[1].querySelector('select');if(select){select.add(new Option(facts.lot,facts.lot,true,true));}}
      fields.insertAdjacentHTML('afterbegin',`<div class="beta-tool-identity"><span>Ferramenta selecionada</span><strong>${type} ${facts.reference} · Lote ${facts.lot}</strong><small>${facts.machines} · ${facts.process} · ${facts.quantity} peças · ${facts.usage} utilização</small></div>`);
    });
    const toolSection=document.querySelector('.priority-grid');
    if(toolSection)toolSection.insertAdjacentHTML('beforebegin','<div class="beta-module-note" style="margin-bottom:12px"><strong>Conjunto 5447T137 · Produção 202601 · Linha B3</strong>CM ST100 lote 12 · MF 5447 lote 08 · BQ T173 lote 04</div>');
    const pickerRows=document.querySelectorAll('#inventoryPicker table tbody tr');
    if(pickerRows[0]){pickerRows[0].children[0].innerHTML='<strong>ST100</strong>';pickerRows[0].children[1].textContent='12';}
    if(pickerRows[1]){pickerRows[1].children[0].innerHTML='<strong>ST21</strong>';pickerRows[1].children[1].textContent='07';}
  }
  if(document.body.dataset.razorShell!=='true'&&['jobon','controlo-create','controlo-hub','pegamentos','boquilhas'].includes(page)){
    document.querySelector('.dmo-app-header__user,.user')?.insertAdjacentHTML('beforeend','<div class="beta-user-nav"><a href="12_LOGIN_01_VISUAL_AUTHORITY_login.html">Sair</a></div>');
  }
  // Novas identidades Tool e lotes são criados exclusivamente no Job On.
  if(page==='boquilhas'){
    window.betaProductionOverview?.renderRail(document.querySelector('#bqCurrentLines'),{
      onSelect:(_line,row)=>{
        if(!row){document.querySelector('#recordEmpty').textContent='Sem produção atual nesta linha.';return}
        const reference=row.bq?.split(' · ')[0]||'';
        document.querySelector('[data-view="registo"]')?.click();
        document.querySelector('#recordSearch').value=reference;
        document.querySelector('#recordEmpty').textContent=`${row.line} · ${row.reference} · ${row.production} · ${row.bq}. Selecione a boquilha para abrir o registo.`;
        document.querySelector('#recordEmpty').classList.remove('hidden');
        document.querySelector('#recordDetail').classList.add('hidden');
      },
      onOpen:(_line,row)=>location.href='31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html?view=registo'
    });
    if(new URLSearchParams(location.search).get('view')==='registo'){document.querySelector('.tab[data-view="registo"]')?.click();}
    document.querySelector('#inlineCreate')?.remove();
  }
  if(page==='boquilhas'){
    const lotGrid=document.querySelector('#lots');
    if(lotGrid){
      const section=lotGrid.closest('.card');section?.insertAdjacentHTML('beforeend','<div class="beta-table-actions"><span id="selectedBqLabel">Selecione uma boquilha</span><button class="btn" type="button" id="openBqCard" disabled>Abrir ficha</button></div>');
      // Render the Boquilhas register read model. Each row receives its canonical tool_id
      // from the read-model item (item.tool_id); no tool_id is hardcoded in this markup.
      const renderLot=(reg)=>{
        const article=document.createElement('article');article.className='lot';
        article.dataset.state=reg.state;article.dataset.toolId=reg.tool_id;
        article.dataset.search=`${reg.reference} ${reg.lot} ${reg.fieldValue}`.toLowerCase().trim();
        const top=document.createElement('div');top.className='lot-top';
        const h4=document.createElement('h4');h4.textContent=`${reg.reference} · ${reg.lot}`;
        const pill=document.createElement('span');pill.className='pill '+reg.pill;pill.textContent=reg.statusLabel;
        top.append(h4,pill);
        const data=document.createElement('div');data.className='lot-data';
        const mk=(label,value)=>{const d=document.createElement('div');const s=document.createElement('span');s.textContent=label;const v=document.createElement('strong');v.textContent=value;d.append(s,v);return d};
        data.append(mk('Quantidade',`${reg.quantity} ${reg.unit}`),mk(reg.fieldLabel,reg.fieldValue),mk('Vida utilizada',reg.vida));
        article.append(top,data);return article;
      };
      lotGrid.replaceChildren();
      const regs=window.boquilhasBoundary?.listRegisters();
      (regs?.ok?regs.data:[]).forEach(reg=>lotGrid.append(renderLot(reg)));
      let selectedLot=null;
      const openCard=()=>{if(!selectedLot)return;const toolId=selectedLot.dataset.toolId;if(!toolId)return;location.href=`ferramentas.html?mode=detail&returnTo=31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html&tool_id=${encodeURIComponent(toolId)}`};
      lotGrid.querySelectorAll('.lot').forEach(lot=>{lot.tabIndex=0;lot.setAttribute('role','button');lot.onclick=()=>{selectedLot=lot;lotGrid.querySelectorAll('.lot').forEach(item=>item.classList.toggle('selected',item===lot));document.querySelector('#selectedBqLabel').textContent=lot.querySelector('h4').textContent;document.querySelector('#openBqCard').disabled=!lot.dataset.toolId};lot.ondblclick=()=>{lot.click();openCard()};lot.onkeydown=e=>{if(e.key==='Enter'){lot.click();openCard()}else if(e.key===' '){e.preventDefault();lot.click()}}});
      document.querySelector('#openBqCard').onclick=openCard;
    }
    const main=document.querySelector('main.main');
    if(main)main.insertAdjacentHTML('afterbegin',`<section class="beta-bq-context" aria-label="Contexto da boquilha e produção">
      <div><span>Boquilha</span><strong>T132</strong></div>
      <div><span>Referência</span><strong>5447T137</strong></div>
      <div><span>Produção</span><strong>202601</strong></div>
      <div><span>Lote</span><strong>04</strong></div>
      <div><span>Linha</span><strong>B3</strong></div>
      <div><span>Estado</span><strong>Aberto</strong></div>
    </section>`);
  }
})();

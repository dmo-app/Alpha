/* Shared production dates and current line status for Job On and Boquilhas. */
(() => {
  const fixtures = [
    {date:'2026-09-21',line:'B1',reference:'5774T173',production:'202603',bq:'T173 · Lote 04'},
    {date:'2026-09-21',line:'B2',reference:'5810T188',production:'202602',bq:'T188 · Lote 08'},
    {date:'2026-09-21',line:'B3',reference:'5447T173',production:'202602',bq:'T173 · Lote 04'},
    {date:'2026-09-21',line:'C1',reference:'9121T173',production:'202601',bq:'T173 · Lote 11'},
    {date:'2026-09-18',line:'C3',reference:'7080C002',production:'202602',bq:'C002 · Lote 18'}
  ];
  const lines=['B1','B2','B3','C1','C2','C3'];
  function records(){
    const result=fixtures.map(row=>({...row}));
    try{
      const detached=JSON.parse(sessionStorage.getItem('betaJobOnCalendarDetached')||'[]');
      for(const row of result)if(detached.includes(`${row.line}|${row.reference}|${row.production}`))row.date='';
    }catch(_){/* Ignore unavailable demo session. */}
    try{
      const created=JSON.parse(sessionStorage.getItem('betaJobOnSummaries')||'[]');
      if(Array.isArray(created))for(const summary of created){
        if(!summary.reference||!summary.production||!summary.machine)continue;
        const date=Object.hasOwn(summary,'plannedDate')?summary.plannedDate||'':'2026-09-21';
        const row={date,line:summary.machine,reference:summary.reference,production:summary.production,bq:summary.contexts?.BQ?.tool?`${summary.contexts.BQ.tool.reference} · Lote ${summary.contexts.BQ.tool.lot}`:'—',summary};
        const index=result.findIndex(item=>item.reference===row.reference&&item.production===row.production&&item.line===row.line);
        if(index<0)result.push(row);else result[index]=row;
      }
    }catch(_){/* Keep the fixtures if demo storage is unavailable. */}
    return result;
  }
  function weekBounds(anchor=new Date()){
    const d=new Date(anchor);d.setHours(12,0,0,0);
    const day=(d.getDay()+6)%7;
    const start=new Date(d);start.setDate(d.getDate()-day);
    const end=new Date(start);end.setDate(start.getDate()+6);
    return {start,end};
  }
  function fmtDay(date){return date.toLocaleDateString('pt-PT',{weekday:'short',day:'numeric'}).replace(/\./g,'');}
  function fmtRange(date){return date.toLocaleDateString('pt-PT',{day:'numeric',month:'short'}).replace(/\./g,'');}
  function renderRail(host,options={}){
    if(!host)return;
    host.classList.add('beta-production-rail');
    host.replaceChildren();
    const heading=document.createElement('div');heading.className='beta-production-rail-head';host.append(heading);
    const list=document.createElement('div');list.className='beta-production-lines';host.append(list);
    const {onSelect,onOpen,mode,currentWeekDate}=options;
    if(mode==='current-week'){
      const {start,end}=weekBounds(currentWeekDate);
      heading.innerHTML=`<strong>Semana atual</strong><span>${fmtRange(start)} – ${fmtRange(end)}</span>`;
      const items=records().filter(r=>r.date && new Date(`${r.date}T12:00:00`)>=start && new Date(`${r.date}T12:00:00`)<=end)
        .sort((a,b)=>a.date.localeCompare(b.date)||lines.indexOf(a.line)-lines.indexOf(b.line));
      if(items.length===0){
        const empty=document.createElement('span');
        empty.className='beta-production-rail-empty';
        empty.textContent='Sem produções previstas para esta semana.';
        empty.style.cssText='display:block;padding:12px 8px;color:#bcd3e4;font-size:12px';
        list.append(empty);
      } else {
        items.forEach((item,index)=>{
          const button=document.createElement('button');button.type='button';
          if(index===0)button.classList.add('active');
          const label=document.createElement('strong');
          label.textContent=fmtDay(new Date(`${item.date}T12:00:00`));
          const detail=document.createElement('span');detail.textContent=`${item.line} · ${item.reference} · ${item.production}`;
          button.append(label,detail);
          button.onclick=()=>{list.querySelectorAll('button').forEach(x=>x.classList.toggle('active',x===button));onSelect?.(item);};
          button.ondblclick=()=>onOpen?.(item);
          list.append(button);
        });
      }
      return;
    }
    heading.innerHTML='<strong>Linhas de produção</strong><span>Estado atual em máquina</span>';
    const current=records();
    for(const line of lines){
      const row=current.filter(item=>item.line===line).sort((a,b)=>(b.date||'').localeCompare(a.date||''))[0];
      const button=document.createElement('button');button.type='button';button.dataset.line=line;
      if(line==='B1')button.classList.add('active');
      const label=document.createElement('strong');label.textContent=line;
      const detail=document.createElement('span');detail.textContent=row?`${row.reference} · ${row.production}`:'Sem produção';
      button.append(label,detail);button.onclick=()=>{list.querySelectorAll('button').forEach(item=>item.classList.toggle('active',item===button));onSelect?.(line,row)};
      button.ondblclick=()=>{if(row)onOpen?.(line,row)};
      list.append(button);
    }
  }
  window.betaProductionOverview={records,lines,renderRail};
})();

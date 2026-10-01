(() => {
  const list=document.querySelector('#repairerLines'),form=document.querySelector('#repairerForm');
  if(!list||!form)return;
  const lines=['B1','B2','B3','C1','C2','C3'];
  const defaults={B1:'Externo A',B2:'Externo B',B3:'',C1:'Externo A',C2:'Externo B',C3:'Externo A'};
  let saved;
  try{saved=JSON.parse(sessionStorage.getItem('betaRepairers')||'{}')}catch{saved={}}
  const state={...defaults,...saved};let selected='';
  const select=form.querySelector('select'),button=form.querySelector('button');
  function render(){list.replaceChildren();for(const line of lines){const row=document.createElement('button');row.type='button';row.className='beta-settings-line'+(selected===line?' selected':'');row.setAttribute('role','option');row.setAttribute('aria-selected',selected===line?'true':'false');const name=document.createElement('strong');name.textContent=line;const current=document.createElement('span');current.textContent=state[line]||'Sem associação';row.append(name,current);row.onclick=()=>{selected=line;select.value=state[line]||'';select.disabled=button.disabled=false;form.querySelector('h3').textContent='Linha '+line;form.querySelector('[role=status]').textContent='';render()};list.append(row)}}
  form.onsubmit=e=>{e.preventDefault();if(!selected)return;state[selected]=select.value;sessionStorage.setItem('betaRepairers',JSON.stringify(state));form.querySelector('[role=status]').textContent='Associação da linha '+selected+' guardada.';render()};render();
})();

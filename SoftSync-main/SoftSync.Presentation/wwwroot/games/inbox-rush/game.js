'use strict';
const $=id=>document.getElementById(id),SLOTS=8,MAX_T=180,INT_DL=12,DL={5:14,4:20,3:28,2:38,1:50};
function hash(s){let h=2166136261;for(const c of String(s)){h^=c.charCodeAt(0);h=Math.imul(h,16777619)}return h>>>0}
// PRNG có seed (mulberry32): cùng bộ đề + cùng seed => cùng chuỗi task
function rng(s){return()=>{s|=0;s=s+0x6D2B79F5|0;let t=Math.imul(s^s>>>15,1|s);t=t+Math.imul(t^t>>>7,61|t)^t;return((t^t>>>14)>>>0)/4294967296}}
let S=null;
SETS.forEach((_,i)=>$('setSel').add(new Option('Bộ '+(i+1),i)));
const qs=new URLSearchParams(location.search),qSet=parseInt(qs.get('set'),10);
if(qSet>=1&&qSet<=SETS.length)$('setSel').value=qSet-1;
if(qs.get('seed'))$('seedIn').value=qs.get('seed').slice(0,20);
$('startBtn').onclick=()=>{
  const v=$('seedIn').value.trim();
  start(+$('setSel').value,v===''?Math.floor(Math.random()*1e5):/^\d{1,9}$/.test(v)?+v:hash(v),v);
};

function build(setIdx,seed){
  const r=rng(seed+setIdx*977),bag={};let at=0;
  return SETS[setIdx].split(' ').map((tok,n)=>{
    const k=tok[0],U=+tok[1],I=+tok[2],D=+tok[3];
    let name=NAMES[k];
    if(!name){if(!bag[k]||!bag[k].length)bag[k]=POOL[k].slice();name=bag[k].splice(Math.floor(r()*bag[k].length),1)[0]}
    const intr='iln'.includes(k)||(k==='d'&&U===5),task={id:n,name,U,I,D,intr,spawn:at,dl:at+(intr?INT_DL:DL[U])};
    at+=6+r()*6;                                    // nhịp 6–12 giây/task
    return task;
  });
}
function start(setIdx,seed,seedText){
  if(S&&S.timer)clearInterval(S.timer);
  S={setIdx,seed,seedText,tasks:build(setIdx,seed),i:0,t:-3,tray:[],slots:Array(SLOTS).fill(null),placed:[],failed:[],
     skipped:[],bumped:[],est:[],react:[],sel:null,paused:false,ended:false,els:{},pend:null,last:performance.now()};
  $('start').hidden=true;$('report').hidden=true;$('game').hidden=false;$('estimate').hidden=true;
  $('tray').innerHTML='';$('log').innerHTML='&nbsp;';
  const sl=$('slots');sl.innerHTML='';S.slotEls=[];
  for(let n=0;n<SLOTS;n++){
    const el=document.createElement('div');el.className='slot';el.tabIndex=0;
    el.innerHTML=`<b>Tiết ${n+1}</b><span class="nm"></span>`;
    el.onclick=()=>place(n);
    el.onkeydown=e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();place(n)}};
    el.onmouseenter=()=>preview(n);el.onmouseleave=clearPrev;
    el.ondragover=e=>{e.preventDefault();preview(n)};el.ondragleave=clearPrev;
    el.ondrop=e=>{e.preventDefault();clearPrev();place(n)};
    sl.append(el);S.slotEls.push(el);
  }
  paintSlots();select(null);
  S.timer=setInterval(()=>{const now=performance.now(),dt=Math.min(.25,(now-S.last)/1000);S.last=now;tick(dt)},100);
}
function say(m){$('log').textContent=m}
function select(k){
  S.sel=k;Object.values(S.els).forEach(e=>e.classList.remove('sel'));
  if(k&&S.els[k.id])S.els[k.id].classList.add('sel');
  $('hint').textContent=k?`Đã chọn "${k.name}" (${k.D} tiết) – bấm hoặc thả vào khung giờ.`:'Bấm hoặc kéo một việc, rồi chọn khung giờ.';
  clearPrev();
}
function addCard(k){
  const el=document.createElement('div');
  el.className='card'+(k.intr?' int':'');el.tabIndex=0;el.draggable=true;el.setAttribute('role','button');
  el.innerHTML=`<div class="top"><span class="nm"></span><button type="button" class="skip">Bỏ qua</button></div>
   <div class="chips"><span>Khẩn ${k.U}</span><span>Quan trọng ${k.I}</span><span>${k.D} tiết</span></div><i class="bar"></i>`;
  el.querySelector('.nm').textContent=(k.intr?'⚠ ':'')+k.name;
  el.querySelector('.skip').onclick=e=>{e.stopPropagation();skip(k)};
  el.onclick=()=>select(S.sel===k?null:k);
  el.onkeydown=e=>{if(e.target===el&&(e.key==='Enter'||e.key===' ')){e.preventDefault();select(S.sel===k?null:k)}};
  el.ondragstart=e=>{select(k);if(e.dataTransfer){e.dataTransfer.effectAllowed='move';e.dataTransfer.setData('text/plain',String(k.id))}};
  el.ondragend=clearPrev;
  S.els[k.id]=el;$('tray').append(el);k.el=el;
}
function dropCard(k){const i=S.tray.indexOf(k);if(i>=0)S.tray.splice(i,1);if(S.els[k.id]){S.els[k.id].remove();delete S.els[k.id]}if(S.sel===k)select(null)}
function paintSlots(){
  S.slotEls.forEach((el,n)=>{
    const k=S.slots[n],first=k&&S.slots[n-1]!==k;
    el.className='slot'+(k?' used':'')+(k&&k.intr?' int':'')+(k&&!first?' cont':'');
    el.querySelector('.nm').textContent=k?(first?k.name:'↳ (tiếp tục)'):'Trống';
  });
}
function preview(n){
  clearPrev();const k=S&&S.sel;if(!k||S.paused)return;
  n=Math.min(n,SLOTS-k.D);const bad=!k.intr&&S.slots.slice(n,n+k.D).some(Boolean);
  for(let j=n;j<n+k.D;j++)S.slotEls[j].classList.add(bad?'pv-bad':'pv-ok');
}
function clearPrev(){if(S&&S.slotEls)S.slotEls.forEach(e=>e.classList.remove('pv-ok','pv-bad'))}
function fail(k){dropCard(k);S.failed.push(k);if(k.intr)S.react.push(20)}   // bỏ mặc việc gấp = 0 điểm thích ứng

function tick(dt){
  if(!S||S.paused||S.ended)return;
  S.t+=dt;
  while(S.i<S.tasks.length&&S.tasks[S.i].spawn<=S.t){
    const k=S.tasks[S.i++];S.tray.push(k);addCard(k);
    if(k.intr)say('⚠ Việc phát sinh: '+k.name);
  }
  S.tray.filter(k=>S.t>=k.dl).forEach(k=>{fail(k);say('Trễ hạn: '+k.name)});
  Object.entries(S.els).forEach(([id,el])=>{
    const k=S.tasks[id],f=Math.max(0,Math.min(1,(k.dl-S.t)/(k.dl-k.spawn)));
    el.querySelector('.bar').style.width=f*100+'%';el.classList.toggle('late',f<.25);
  });
  const free=S.slots.filter(x=>!x).length;
  $('clock').textContent=S.t<0?'Bắt đầu sau '+Math.ceil(-S.t)+'…':Math.floor(S.t/60)+':'+String(Math.floor(S.t%60)).padStart(2,'0');
  $('progress').textContent=`Khung giờ ${SLOTS-free}/${SLOTS} · Việc ${S.i}/${S.tasks.length}`;
  if(!free)return end('Lịch ngày đã đầy.');
  if(S.t>=MAX_T){S.tray.slice().forEach(fail);return end('Hết thời gian.')}
  if(S.i>=S.tasks.length&&!S.tray.length)return end('Đã hết việc.');
}
function skip(k){
  if(S.ended||S.paused||!S.tray.includes(k))return;
  if(k.intr)S.react.push(Math.min(INT_DL,S.t-k.spawn));
  dropCard(k);
  if(k.I<=2){S.skipped.push(k);say('Đã từ chối: '+k.name)}   // từ chối việc không quan trọng: quyết định đúng
  else{S.failed.push(k);say('Bỏ lỡ việc quan trọng: '+k.name)}
}
function place(n){
  const k=S.sel;if(!S||S.ended||S.paused||!k||!S.tray.includes(k))return;
  n=Math.min(n,SLOTS-k.D);
  const clash=[...new Set(S.slots.slice(n,n+k.D).filter(Boolean))];
  if(clash.length&&!k.intr)return say('Khung giờ đó đã có việc – hãy chọn khung trống.');
  const left=(k.dl-S.t)/(k.dl-k.spawn),factor=left>=.5?1.5:left>=.2?1:.5;
  const commit=est=>{
    S.paused=false;S.pend=null;$('estimate').hidden=true;S.last=performance.now();
    clash.forEach(c=>{                              // việc gấp đè lịch: việc bị đè quay lại khay nếu còn hạn
      S.slots.forEach((x,j)=>{if(x===c)S.slots[j]=null});
      S.placed=S.placed.filter(p=>p.k!==c);S.bumped.push(c);
      if(S.t<c.dl){S.tray.push(c);addCard(c)}else S.failed.push(c);
    });
    for(let j=n;j<n+k.D;j++)S.slots[j]=k;
    if(est!==null&&!k.asked)S.est.push(Math.abs(est-k.D));
    k.asked=true;
    if(k.intr)S.react.push(Math.min(INT_DL,S.t-k.spawn));
    S.placed.push({k,factor});dropCard(k);paintSlots();
    say((clash.length?'Đã chèn (đè '+clash.length+' việc): ':'Đã xếp: ')+k.name);
  };
  if(k.intr||k.asked)return commit(null);          // không hỏi ước lượng cho việc gấp / việc đã hỏi
  S.paused=true;S.pend=commit;clearPrev();
  $('estQ').textContent=`"${k.name}" – bạn nghĩ việc này mất mấy tiết?`;
  $('estimate').hidden=false;$('estBtns').querySelector('button').focus();
}
$('estimate').onclick=e=>{
  const b=e.target.closest('button');if(!b||!S||!S.pend)return;
  if(b.dataset.v){const p=S.pend;p(+b.dataset.v)}
  else{S.pend=null;S.paused=false;S.last=performance.now();$('estimate').hidden=true}
};
document.addEventListener('keydown',e=>{
  if($('estimate').hidden)return;
  if(e.key==='Escape')$('estBtns').querySelector('[data-c]').click();
  else if('123'.includes(e.key)&&e.key)$('estBtns').querySelector(`[data-v="${e.key}"]`).click();
});

function end(reason){
  S.ended=true;clearInterval(S.timer);$('estimate').hidden=true;
  const val=k=>k.U+2*k.I;
  let cap=SLOTS,ideal=0;                           // PS chuẩn hoá theo phương án lý tưởng (greedy giá trị/tiết)
  [...S.tasks].sort((a,b)=>val(b)/b.D-val(a)/a.D).forEach(k=>{if(k.D<=cap){ideal+=val(k)*1.5;cap-=k.D}});
  const PS=Math.min(100,S.placed.reduce((s,p)=>s+val(p.k)*p.factor,0)/ideal*100);
  const avg=(a,f)=>a.length?a.reduce((s,x)=>s+f(x),0)/a.length:null;
  const CS=avg(S.est,d=>Math.max(0,100-d*20)),AS=avg(S.react,t=>Math.max(0,100-t*5));
  const parts=[[.5,PS],[.3,CS],[.2,AS]].filter(p=>p[1]!==null);   // thiếu dữ liệu thì bỏ thành phần, chia lại trọng số
  const base=parts.reduce((s,p)=>s+p[0]*p[1],0)/parts.reduce((s,p)=>s+p[0],0);
  const empty=S.slots.filter(x=>!x).length,pen=S.failed.length*3+empty;
  const total=Math.max(0,base-pen),r=Math.round,hi=v=>v>=70,msgs=[];
  if(!hi(PS)&&CS!==null&&hi(CS))msgs.push('Em ước lượng thời gian khá tốt, nhưng dễ bị cuốn theo việc gấp mà quên việc quan trọng hơn.');
  else if(!hi(PS))msgs.push('Em cần ưu tiên theo mức độ quan trọng thay vì chỉ theo độ gấp.');
  if(CS!==null&&!hi(CS))msgs.push('Em có xu hướng đánh giá sai thời gian cần cho một việc – nên tập chia nhỏ việc để ước lượng chính xác hơn.');
  if(AS!==null&&!hi(AS))msgs.push('Em cần thời gian dài hơn để lấy lại nhịp học sau khi bị gián đoạn – có thể luyện tập tập trung sâu (deep work).');
  if(!msgs.length)msgs.push('Kỹ năng quản lý thời gian tốt, biết cân bằng giữa lập kế hoạch và xử lý tình huống bất ngờ.');
  const names=a=>a.map(k=>k.name).join('; ')||'không có';
  const wasted=S.placed.filter(p=>p.k.I<=2).map(p=>p.k),missed=S.failed.filter(k=>k.I>=4);
  const data={set:S.setIdx+1,seed:S.seedText||S.seed,PS:r(PS),CS:CS===null?null:r(CS),AS:AS===null?null:r(AS),penalty:pen,total:r(total),
    placed:S.placed.length,failed:S.failed.length,declined:S.skipped.length,bumped:S.bumped.length,emptySlots:empty,seconds:r(Math.max(0,S.t)),endReason:reason};
  const outcome=k=>S.placed.some(p=>p.k===k)?'Đã xếp lịch':S.skipped.includes(k)?'Đã từ chối':S.failed.includes(k)?'Bỏ lỡ hoặc trễ hạn':S.bumped.includes(k)?'Bị đẩy khỏi lịch':'Chưa xếp';
  data.title=`Inbox Rush · Bộ ${data.set}`;data.score=data.total;data.unit='/100';
  data.summary=`Seed ${data.seed} · Đã xếp ${data.placed} việc, trễ hoặc bỏ lỡ ${data.failed}, từ chối ${data.declined}.`;
  data.metrics=[{label:'Ưu tiên hóa',value:`${data.PS}%`},{label:'Ước lượng',value:data.CS===null?'—':`${data.CS}%`},{label:'Thích ứng',value:data.AS===null?'—':`${data.AS}%`},{label:'Điểm phạt',value:`-${data.penalty}`}];
  data.tasks=S.tasks.map(k=>({name:k.name,urgency:k.U,importance:k.I,duration:k.D,interruption:k.intr,outcome:outcome(k)}));
  data.details=[...msgs,...data.tasks.map(k=>`${k.name} — ${k.outcome} · Khẩn ${k.urgency}, quan trọng ${k.importance}, ${k.duration} tiết`)];
  data.missedImportant=missed.map(k=>k.name);data.lowValueScheduled=wasted.map(k=>k.name);data.feedback=msgs;
  window.SoftSyncGameHistory?.append('inbox-rush',data,'softsync.inbox-rush.history.v1');
  const sc=(v,l)=>`<div class="${v===null?'na':''}"><strong>${v===null?'–':r(v)}</strong>${l}</div>`;
  $('game').hidden=true;$('report').hidden=false;
  $('report').innerHTML=`<h1>Kết quả: ${r(total)}/100</h1><p class="small">${reason}</p>
  <div class="scores">${sc(PS,'Ưu tiên hoá')}${sc(CS,'Ước lượng')}${sc(AS,'Thích ứng')}${sc(-pen,'Điểm phạt')}</div>
  <ul>${msgs.map(m=>`<li>${m}</li>`).join('')}</ul>
  <p><b>Việc quan trọng bị bỏ lỡ:</b> ${names(missed)}<br><b>Việc ít giá trị đã xếp vào lịch:</b> ${names(wasted)}</p>
  <p>Đã xếp ${S.placed.length} · Trễ/bỏ lỡ ${S.failed.length} · Từ chối đúng ${S.skipped.length} · Bị đè lịch ${S.bumped.length} · Trống ${empty} khung</p>
  <div class="btns"><button class="big" id="again" type="button">Chơi lại</button><button class="big" id="dl" type="button">Tải kết quả (.json)</button><button class="big secondary" id="historyFromResult" type="button">Xem lịch sử</button></div>`;
  $('again').onclick=()=>{S=null;$('report').hidden=true;$('start').hidden=false};
  $('dl').onclick=()=>downloadResult(data);
  $('historyFromResult').onclick=()=>{const button=document.querySelector('#game-history .ss-gh-open');button?.click();document.getElementById('game-history')?.scrollIntoView({behavior:'smooth'});};
  S.result=data;
}


window.SoftSyncGameHistory?.mount({gameKey:'inbox-rush',storageKey:'softsync.inbox-rush.history.v1',title:'Inbox Rush'});
